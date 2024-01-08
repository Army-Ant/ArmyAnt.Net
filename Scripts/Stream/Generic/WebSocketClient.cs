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
    /// WebSocket �ͻ��˴�����
    /// </summary>
    public class WebSocketClient : IWebSocketClient {
        protected const int BUFFER_SIZE = 8192;

        /// <summary> �յ����Է���������Ϣʱ�����������ݵĻص� </summary>
        public event Action<byte[]> OnReading;
        /// <summary> ���ӶϿ�ʱ�Ļص� </summary>
        public event Action OnDisconnected;
        /// <summary> ������Uri </summary>
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
        /// <summary> �Ƿ������� </summary>
        public bool IsRunning => Connected;

        public bool IsPausing { get; private set; }

        /// <summary> �Ƿ������� </summary>
        public bool Connected => self.State == WebSocketState.Open;

        /// <summary>
        /// ���ӵ������� 80 �˿ڵ� Websocket ������
        /// </summary>
        /// <param name="port"> Ҫ���ӵķ������˿ں� </param>
        public void Open() => Open(_serverUri).Wait();

        /// <summary>
        /// async �첽���ӵ�ָ��Uri��Websocket������
        /// </summary>
        /// <param name="serverUri"> ������Uri </param>
        public async Task Open(Uri serverUri) {
            _serverUri = serverUri;
            await self.ConnectAsync(serverUri, cancellationTokenSource.Token);
            receiveTask = ReceiveAsync();
        }

        /// <summary>
        /// <para> �Ͽ����� </para>
        /// </summary>
        public void Close(bool nowait) {
            var waiter = Close(WebSocketCloseStatus.NormalClosure);
            if (!nowait) {
                waiter.Wait();
            }
        }

        /// <summary>
        /// async �Ͽ�Websocket����
        /// </summary>
        /// <param name="status"> �Ͽ�ԭ�� </param>
        /// <param name="statusDescription"> �Ͽ�ԭ������ </param>
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
        /// ������Ϣ�������ӵ�Websocket������
        /// </summary>
        /// <param name="content"> ��Ϣ���� </param>
        public int Write(byte[] content) {
            var task = WriteAsync(content);
            task.Wait();
            return task.Result;
        }

        /// <summary>
        /// ������Ϣ�������ӵ�Websocket������
        /// </summary>
        /// <param name="content"> ��Ϣ���� </param>
        public async Task<int> WriteAsync(byte[] content) {
            return await Write(content, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// async ������Ϣ�������ӵ�Websocket������
        /// </summary>
        /// <param name="content"> ��Ϣ���� </param>
        /// <param name="type"> Web��Ϣ���� </param>
        public async Task<int> Write(byte[] content, WebSocketMessageType type) {
            await self.SendAsync(new ArraySegment<byte>(content), type, true, cancellationTokenSource.Token);
            return content.Length;
        }

        /// <summary>
        /// async (�ڲ�) �������ݵ��߳�/��������
        /// </summary>
        private async Task ReceiveAsync() {
            while (!cancellationTokenSource.Token.IsCancellationRequested) {
                bool connected = false;
                while (Connected) {
                    if (!IsPausing) {
                        connected = true;
                        var buffer = new byte[BUFFER_SIZE]; // TODO: �Ż��ڴ�ʹ��
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

        /// <summary> Websocket �ͻ��˶��� </summary>
        private readonly ClientWebSocket self = new ClientWebSocket();
        /// <summary> �շ���Ϣ������ </summary>
        private Task receiveTask;
        /// <summary> �ͻ�����ֹ������Ʒ�, �ڶϿ���ÿͻ��˵�����ʱ�趨��ȡ���շ����� </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    }
}
