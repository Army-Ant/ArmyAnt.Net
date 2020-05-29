using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArmyAnt.Network {
    /// <summary>
    /// Http以及WebSocket的处理类
    /// TODO: SSL加密功能暂未验证, 应该需要进一步处理
    /// </summary>
    public class HttpServer : ITcpNetworkServer {
        protected const int BUFFER_SIZE = 8192;

        /// <summary>
        /// 服务器是否在运行
        /// </summary>
        public bool IsStarting => self != null;

        /// <summary> 子协议名 </summary>
        public string SubProtocol { get; set; }
        /// <summary> 服务器的可用URI集合 </summary>
        public HttpListenerPrefixCollection Prefixes => self?.Prefixes;

        /// <summary>
        /// 开启Http服务器, 使用URI "http://localhost" 和默认(80)端口
        /// </summary>
        public void Start() => Start("http://localhost/");

        /// <summary>
        /// 开启Http服务器, 使用URI "http://localhost" 和指定端口
        /// </summary>
        public void Start(ushort port) => Start("http://localhost:" + port + "/");

        /// <summary>
        /// 开启Http服务器
        /// </summary>
        /// <param name="prefixes">可用URI集合, 调整URI以决定服务器的端口号及可访问方式, 前缀请使用 http(无SSL)或 https(有SSL), 并以"/"结尾</param>
        public void Start(params string[] prefixes) {
            if(prefixes == null || prefixes.Length == 0) {
                throw new ArgumentNullException();
            }
            mutex.Lock();
            if(self != null) {
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            self = new HttpListener();
            foreach(var i in prefixes) {
                self.Prefixes.Add(i);
            }
            self.Start();
            mutex.Unlock();
            acceptTask = AcceptAsync();
        }

        /// <summary>
        /// 停止HTTP服务器, 断开所有WebSocket客户端连接
        /// </summary>
        public void Stop(bool nowait = false) {
            mutex.Lock();
            foreach(var i in clients) {
                i.Value.cancellationToken.Cancel();
                i.Value.client?.WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server is closing", default)?.Wait();
                i.Value.client?.WebSocket?.Dispose();
                if(!nowait) {
                    i.Value.receiveTask?.Wait();
                }
            }
            clients.Clear();
            try {
                self.Stop();
            } catch(ObjectDisposedException) {

            }
            self = null;
            if(!nowait) {
                acceptTask?.Wait();
            }
            acceptTask = null;
            mutex.Unlock();
        }


        public (Task main, List<Task> clients) WaitingTask {
            get {
                var ret = new List<Task>();
                mutex.Lock();
                foreach(var i in clients) {
                    ret.Add(i.Value.receiveTask);
                }
                mutex.Unlock();
                return (acceptTask, ret);
            }
        }

        /// <summary>
        /// async 踢掉指定WebSocket客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        public async Task KickOut(int index) => await KickOut(index, WebSocketCloseStatus.Empty, "Server kicked you out", true);

        /// <summary>
        /// async 踢掉指定WebSocket客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <param name="reason"> 回复给客户端的踢掉理由 </param>
        /// <param name="info"> 回复给客户端的最后信息 </param>
        public async Task KickOut(int index, WebSocketCloseStatus reason, string info) {
            mutex.Lock();
            await KickOut(index, reason, info, true);
            mutex.Unlock();
        }

        /// <summary>
        /// async 向某Websocket客户端推送消息
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <param name="content"> 消息正文 </param>
        public async Task Send(int index, byte[] content) => await Send(index, content, true);

        /// <summary>
        /// async 向某Websocket客户端推送消息
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <param name="content"> 消息正文 </param>
        /// <param name="binary"> true指示该消息为二进制, false为文本 </param>
        /// <param name="isEndOfMessage"> 指示此条消息是否已经完结 </param>
        public async Task Send(int index, byte[] content, bool binary, bool isEndOfMessage = true) {
            mutex.Lock();
            var info = clients[index];
            mutex.Unlock();
            info.mutex.Lock();
            await info.client.WebSocket.SendAsync(new ArraySegment<byte>(content), binary ? WebSocketMessageType.Binary : WebSocketMessageType.Text, isEndOfMessage, info.cancellationToken.Token);
            info.mutex.Unlock();
        }

        /// <summary>
        /// 查询是否存在指定序列号的Websocket客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <returns> 存在于连接中, 则返回true </returns>
        public bool IsClientExist(int index) => clients.ContainsKey(index);

        /// <summary>
        /// 有新WebSocket客户端接入时的回调
        /// </summary>
        public OnTcpServerConnected OnTcpServerConnected { get; set; }
        /// <summary>
        /// 有WebSocket客户端关闭连接, 断开连接或被踢掉时的回调
        /// </summary>
        public OnTcpServerDisonnected OnTcpServerDisonnected { get; set; }
        /// <summary>
        /// 收到来自WebSocket客户端的数据时回调
        /// </summary>
        public OnTcpServerReceived OnTcpServerReceived { get; set; }
        /// <summary>
        /// 收到HTTP请求 (非Websocket) 时的回调, 需在此回调中拼接response回复体并自行调用Close
        /// </summary>
        public OnHttpServerReceived OnHttpServerReceived { get; set; }

        /// <summary>
        /// (内部) async 踢掉指定的客户端
        /// </summary>
        /// <param name="index"> 客户端序列号 </param>
        /// <param name="reason"> 回复给客户端的踢掉理由 </param>
        /// <param name="info"> 回复给客户端的最后信息 </param>
        /// <param name="wait"> 是否等待, 内部清理废弃客户端建议不要等待 </param>
        /// <returns></returns>
        private async Task KickOut(int index, WebSocketCloseStatus reason, string info, bool wait = true) {
            if (clients.ContainsKey(index))
            {
                var websocket = clients[index];
                clients.Remove(index);
                websocket.cancellationToken.Cancel();
                await websocket.client?.WebSocket?.CloseAsync(reason, info, default);
                if (wait && websocket.receiveTask != null)
                {
                    await websocket.receiveTask;
                }
                websocket.client?.WebSocket?.Dispose();
            }
        }

        /// <summary>
        /// (内部) async 接受HTTP请求的任务主体函数
        /// </summary>
        private async Task AcceptAsync() {
            while (self != null)
            {
                var context = await self.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var newClient = new ClientInfo()
                    {
                        client = null,
                        cancellationToken = new CancellationTokenSource(),
                        receiveTask = null,
                    };
                    var index = 0;
                    mutex.Lock();
                    while (clients.ContainsKey(++index))
                    {
                    }
                    clients.Add(index, newClient);
                    mutex.Unlock();
                    newClient.receiveTask = WebSocketAcceptAsync(index, newClient, context);
                }
                else
                {
                    OnHttpServerReceived(context.Request, context.Response, context.User);
                }
                await CheckToRemoveObsoleteConnections();
            }
        }

        /// <summary>
        /// (内部) async 接受websocket连接请求的任务主体函数
        /// </summary>
        /// <param name="index"> 客户端序列号 </param>
        /// <param name="client"> 客户端信息 </param>
        /// <param name="context"> Http监听者上下文 </param>
        private async Task WebSocketAcceptAsync(int index, ClientInfo client, HttpListenerContext context)
        {
            var websocketContext = await context.AcceptWebSocketAsync(SubProtocol);
            client.client = websocketContext;
            if (!OnTcpServerConnected(index, context.Request.RemoteEndPoint))
            {
                await KickOut(index, WebSocketCloseStatus.EndpointUnavailable, "Server lost your connection", false);
            }
            else
            {
                await Task.WhenAll(ReceiveAsync(index, client, context), Task.Run(()=> {
                    while (true)
                    {
                        Console.WriteLine(client.client.WebSocket.State.ToString());
                        if (client.client.WebSocket == null || client.client.WebSocket.State == WebSocketState.Aborted || client.client.WebSocket.State == WebSocketState.Closed)
                        {
                            client.cancellationToken.Token.Register(() =>
                            {
                                KickOut(index, WebSocketCloseStatus.EndpointUnavailable, "Server lost your connection", false).Wait();
                                client.client.WebSocket.Abort();
                                Console.WriteLine("called");
                            });
                            client.cancellationToken.Cancel();
                            break;
                        }
                }
                    while (true)
                    {
                        Console.WriteLine(client.client.WebSocket.State.ToString());
                    }
                }));

            }

        }

        /// <summary>
        /// (内部) async 接受某Websocket客户端消息的任务主体函数
        /// </summary>
        /// <param name="index"> 客户端序列号 </param>
        /// <param name="client"> 客户端信息 </param>
        /// <param name="context"> Http监听者上下文 </param>
        private async Task ReceiveAsync(int index, ClientInfo client, HttpListenerContext context)
        {
            while (client.client.WebSocket != null && client.client.WebSocket.State == WebSocketState.Open)
            {
                if (client.mutex != null)
                {
                    var buffer = new byte[BUFFER_SIZE];
                    var result = await client.client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), client.cancellationToken.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await KickOut(index, WebSocketCloseStatus.EndpointUnavailable, "Server lost your connection", false);
                        OnTcpServerDisonnected(index);
                    }
                    else if (result.Count > 0)
                    {
                        OnTcpServerReceived(index, buffer.Take(result.Count).ToArray());
                    }
                }
            }
            await CheckToRemoveObsoleteConnections();
            OnTcpServerDisonnected(index);
        }

        /// <summary>
        /// (内部) async 清理废弃客户端的执行体
        /// </summary>
        private async Task CheckToRemoveObsoleteConnections() {
            mutex.Lock();
            var needKicked = new Queue<int>();
            foreach(var i in clients) {
                if(i.Value.client.WebSocket == null || (i.Value.client.WebSocket.State != WebSocketState.Connecting && i.Value.client.WebSocket.State != WebSocketState.Open)) {
                    needKicked.Enqueue(i.Key);
                }
            }
            foreach(var i in needKicked) {
                await KickOut(i, WebSocketCloseStatus.EndpointUnavailable, "Server lost your connection", false);
            }
            mutex.Unlock();
        }

        /// <summary> 单个客户端的信息体 </summary>
        private class ClientInfo {
            /// <summary> Websocket连接上下文, 其中包含Websocket客户端连接套接字 </summary>
            public HttpListenerWebSocketContext client;
            /// <summary> Websocket客户端终止任务控制符, 在断开与该客户端的连接时设定以取消收发任务 </summary>
            public CancellationTokenSource cancellationToken;
            /// <summary> 客户端接收消息任务句柄 </summary>
            public Task receiveTask;
            /// <summary> 资源锁 </summary>
            public readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        }

        /// <summary> HTTP服务器连接体 </summary>
        private HttpListener self;
        /// <summary> 服务器接收HTTP任务句柄 </summary>
        private Task acceptTask;
        /// <summary> 服务器资源锁 </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        /// <summary> Websocket客户端列表 </summary>
        private readonly Dictionary<int, ClientInfo> clients = new Dictionary<int, ClientInfo>();
    }
}
