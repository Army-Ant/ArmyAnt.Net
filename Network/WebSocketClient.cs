using System;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArmyAnt.Network {
    public class WebSocketClient : ITcpNetworkClient {
        protected const int BUFFER_SIZE = 8192;

        /// <summary> 收到来自服务器的消息时的回调 </summary>
        public OnWebsocketClientReceived OnWebsocketClientReceived { get; set; }
        /// <summary> 连接断开时的回调 </summary>
        public OnTcpClientDisonnected OnTcpClientDisonnected { get; set; }
        /// <summary> 服务器Uri </summary>
        public Uri ServerUri { get; private set; }
        /// <summary> 是否已连接 </summary>
        public bool IsStarting => Connected;
        /// <summary> 是否已连接 </summary>
        public bool Connected => self.State == WebSocketState.Open;

        /// <summary>
        /// 连接到本机上指定端口号的 Websocket 服务器
        /// </summary>
        /// <param name="port"> 要连接的服务器端口号 </param>
        public void Start(ushort port) => Connect(IPAddress.Loopback, port);

        /// <summary>
        /// 连接到指定网络地址的服务器
        /// </summary>
        /// <param name="server"> 服务器的IP网络地址 </param>
        public void Connect(IPAddress addr, int port) => ConnectAsync(addr, port).Wait();

        /// <summary>
        /// async 异步连接到指定网络地址的Websocket服务器.
        /// </summary>
        /// <param name="server"> 服务器的IP网络地址 </param>
        public async Task ConnectAsync(IPAddress addr, int port) {
            await ConnectAsync(new UriBuilder("ws", addr.ToString(), port).Uri);
        }

        /// <summary>
        /// async 异步连接到指定Uri的Websocket服务器
        /// </summary>
        /// <param name="serverUri"> 服务器Uri </param>
        public async Task ConnectAsync(Uri serverUri) {
            ServerUri = serverUri;
            await self.ConnectAsync(serverUri, cancellationTokenSource.Token);
            receiveTask = ReceiveAsync();
        }

        /// <summary>
        /// <para> 断开连接 </para>
        /// </summary>
        public void Stop(bool nowait) {
            var waiter = Stop(WebSocketCloseStatus.NormalClosure);
            if(!nowait) {
                waiter.Wait();
            }
        }

        /// <summary>
        /// async 断开Websocket连接
        /// </summary>
        /// <param name="status"> 断开原因 </param>
        /// <param name="statusDescription"> 断开原因描述 </param>
        public async Task Stop(WebSocketCloseStatus status, string statusDescription = "") {
            cancellationTokenSource.Cancel();
            await self.CloseAsync(status, statusDescription, CancellationToken.None);
            await receiveTask;
            self.Dispose();
        }

        public Task WaitingTask => receiveTask;

        /// <summary>
        /// 发送消息到已连接的Websocket服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        public void Send(byte[] content) => Send(content, WebSocketMessageType.Binary).Wait();

        /// <summary>
        /// async 发送消息到已连接的Websocket服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        /// <param name="type"> Web消息类型 </param>
        public Task Send(byte[] content, WebSocketMessageType type) => self.SendAsync(new ArraySegment<byte>(content), type, true, cancellationTokenSource.Token);

        /// <summary>
        /// async (内部) 接收数据的线程/任务函数体
        /// </summary>
        private async Task ReceiveAsync()
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (Connected)
                {
                    var buffer = new byte[BUFFER_SIZE]; // TODO: 优化内存使用
                    var result = await self.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        cancellationTokenSource.Cancel();
                        await self.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                        self.Dispose();
                    }
                    else if (result.Count > 0)
                    {
                        OnWebsocketClientReceived(buffer.Take(result.Count).ToArray());
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(1);
                }
                OnTcpClientDisonnected();
            }
        }

        /// <summary> Websocket 客户端对象 </summary>
        private readonly ClientWebSocket self = new ClientWebSocket();
        /// <summary> 收发消息任务句柄 </summary>
        private Task receiveTask;
        /// <summary> 客户端终止任务控制符, 在断开与该客户端的连接时设定以取消收发任务 </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    }
}
