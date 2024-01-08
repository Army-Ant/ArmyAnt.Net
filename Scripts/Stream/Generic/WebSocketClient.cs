using ArmyAnt.Stream.SingleWriteStreams;
using ArmyAnt.Stream.Exception;

using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic
{
    /// <summary>
    /// WebSocket 客户端处理类
    /// </summary>
    public class WebSocketClient : IWebSocketClient {
        protected const int BUFFER_SIZE = 8192;

        /// <summary> 收到来自服务器的消息时，仅分析数据的回调 </summary>
        public event Action<byte[]> OnReading;
        /// <summary> 连接断开时的回调 </summary>
        public event Action OnDisconnected;
        /// <summary> 服务器Uri </summary>
        private Uri _serverUri = new Uri("wss://127.0.0.1:8800");
        public Uri ServerUri {
            get {
                return _serverUri;
            }
            set {
                if (!Connected) {
                    _serverUri = value;
                } else {
                    throw new NetworkException(ExceptionType.ServerHasConnected);
                }
            }
        }
        /// <summary> 是否已连接 </summary>
        public bool IsRunning => Connected;

        public bool IsPausing { get; private set; }

        /// <summary> 是否已连接 </summary>
        public bool Connected => self.State == WebSocketState.Open;

        /// <summary>
        /// 连接到本机上 80 端口的 Websocket 服务器
        /// </summary>
        /// <param name="port"> 要连接的服务器端口号 </param>
        public void Open() => Open(_serverUri).Wait();

        /// <summary>
        /// async 异步连接到指定Uri的Websocket服务器
        /// </summary>
        /// <param name="serverUri"> 服务器Uri </param>
        public async Task Open(Uri serverUri) {
            _serverUri = serverUri;
            await self.ConnectAsync(serverUri, cancellationTokenSource.Token);
            receiveTask = ReceiveAsync();
        }

        /// <summary>
        /// <para> 断开连接 </para>
        /// </summary>
        public void Close(bool nowait) {
            var waiter = Close(WebSocketCloseStatus.NormalClosure);
            if (!nowait) {
                waiter.Wait();
            }
        }

        /// <summary>
        /// async 断开Websocket连接
        /// </summary>
        /// <param name="status"> 断开原因 </param>
        /// <param name="statusDescription"> 断开原因描述 </param>
        public async Task Close(WebSocketCloseStatus status, string statusDescription = "") {
            cancellationTokenSource.Cancel();
            await self.CloseAsync(status, statusDescription, CancellationToken.None);
            await receiveTask;
            self.Dispose();
        }

        public void Pause() {
            IsPausing = true;
        }

        public void Resume() {
            IsPausing = false;
        }

        public Task WaitingTask => receiveTask;

        /// <summary>
        /// 发送消息到已连接的Websocket服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        public int Write(byte[] content) {
            var task = WriteAsync(content);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// 发送消息到已连接的Websocket服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        public async Task<int> WriteAsync(byte[] content) {
            return await Write(content, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// async 发送消息到已连接的Websocket服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        /// <param name="type"> Web消息类型 </param>
        public async Task<int> Write(byte[] content, WebSocketMessageType type) {
            await self.SendAsync(new ArraySegment<byte>(content), type, true, cancellationTokenSource.Token);
            return content.Length;
        }

        /// <summary>
        /// async (内部) 接收数据的线程/任务函数体
        /// </summary>
        private async Task ReceiveAsync() {
            while (!cancellationTokenSource.Token.IsCancellationRequested) {
                bool connected = false;
                while (Connected) {
                    if (!IsPausing) {
                        connected = true;
                        var buffer = new byte[BUFFER_SIZE]; // TODO: 优化内存使用
                        var result = await self.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                        if (result.MessageType == WebSocketMessageType.Close) {
                            cancellationTokenSource.Cancel();
                            await self.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                            self.Dispose();
                        } else if (result.Count > 0) {
                            var data = buffer.Take(result.Count).ToArray();
                            OnReading?.Invoke(data);
                        }
                    }
                    await Task.Yield();
                }
                if (connected) {
                    OnDisconnected?.Invoke();
                }
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
