using ArmyAnt.Stream.MultiWriteStreams;
using ArmyAnt.Stream.Exception;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using ArmyAnt.Stream.SingleWriteStreams;

namespace ArmyAnt.Stream.Generic
{
    /// <summary>
    /// HTTP ������������ WebSocket ��������������
    /// TODO: SSL���ܹ�����δ��֤, Ӧ����Ҫ��һ������
    /// </summary>
    public class HttpServer : IHttpServer {
        protected const int BUFFER_SIZE = 8192;

        /// <summary>
        /// �������Ƿ�������
        /// </summary>
        public bool IsRunning => self != null;

        public bool IsPausing { get; private set; }

        /// <summary> ��Э���� </summary>
        public string SubProtocol { get; set; } = null;

        /// <summary> �������Ŀ���URI���� </summary>
        public HttpListenerPrefixCollection Prefixes => self?.Prefixes;

        /// <summary>
        /// ����Http������, ʹ��URI "http://localhost" ��Ĭ��(80)�˿�
        /// </summary>
        public void Open() => Open("http://localhost/");

        /// <summary>
        /// ����Http������
        /// </summary>
        /// <param name="prefixes">����URI����, ����URI�Ծ����������Ķ˿ںż��ɷ��ʷ�ʽ, ǰ׺��ʹ�� http(��SSL)�� https(��SSL), ����"/"��β</param>
        /// <exception cref="ArgumentNullException"> �� <paramref name="prefixes"/> ���� null �������, ���������Ա����Ϊ null ��ֵʱ���� </exception>
        /// <exception cref="ArgumentException"> �� <paramref name="prefixes"/> ����������Ϸ�ʱ����, �μ� <seealso cref="HttpListenerPrefixCollection.Add(string)"/> </exception>
        /// <exception cref="ObjectDisposedException"> �μ� <seealso cref="HttpListenerPrefixCollection.Add(string)"/> �Լ� <seealso cref="HttpListener.Start()"/> </exception>
        /// <exception cref="HttpListenerException"> �μ� <seealso cref="HttpListenerPrefixCollection.Add(string)"/> �Լ� <seealso cref="HttpListener.Start()"/> </exception>
        /// <exception cref="NetworkException"> ���������Ѿ���������ʱ���� ��<seealso cref="ExceptionType.ServerHasNotStopped"/>�� </exception>
        public void Open(params string[] prefixes) {
            if (prefixes == null || prefixes.Length == 0) {
                throw new ArgumentNullException();
            }
            mutex.Lock();
            if (self != null) {
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            self = new HttpListener();
            foreach (var i in prefixes) {
                self.Prefixes.Add(i);
            }
            self.Start();
            mutex.Unlock();
            acceptTask = AcceptAsync();
        }

        /// <summary>
        /// ֹͣHTTP������, �Ͽ�����WebSocket�ͻ�������
        /// </summary>
        /// <exception cref="ObjectDisposedException"> �μ� <seealso cref="CancellationTokenSource.Cancel()"/> </exception>
        /// <exception cref="AggregateException"> �μ� <seealso cref="CancellationTokenSource.Cancel()"/> </exception>
        public void Close(bool nowait = false) {
            mutex.Lock();
            var waitingList = new List<Task>() { };
            foreach (var i in clients) {
                i.Value.cancellationToken.Cancel();
                i.Value.client?.WebSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server is closing", default)?.Wait();
                i.Value.client?.WebSocket?.Dispose();
                if (!nowait && i.Value.receiveTask != null) {
                    waitingList.Add(i.Value.receiveTask);
                }
            }
            if (waitingList.Count > 0) {
                Task.WaitAll(waitingList.ToArray());
            }
            clients.Clear();
            try {
                self.Stop();
            } catch (ObjectDisposedException) {

            }
            self = null;
            if (!nowait && acceptTask != null) {
                acceptTask.Wait();
                acceptTask = null;
            }
            mutex.Unlock();
        }

        public void Pause() {
            IsPausing = true;
        }

        public void Resume() {
            IsPausing = false;
        }

        /// <summary>
        /// ��ȡ��ǰ�������ڵȴ��е��߳�
        /// </summary>
        public (Task main, List<Task> clients) WaitingTask {
            get {
                var ret = new List<Task>();
                mutex.Lock();
                foreach (var i in clients) {
                    ret.Add(i.Value.receiveTask);
                }
                mutex.Unlock();
                return (acceptTask, ret);
            }
        }

        /// <summary>
        /// async �ߵ�ָ��WebSocket�ͻ���
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        public async Task KickOut(int index) => await KickOut(index, WebSocketCloseStatus.Empty, "Server kicked you out", true);

        /// <summary>
        /// async �ߵ�ָ��WebSocket�ͻ���
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        /// <param name="reason"> �ظ����ͻ��˵��ߵ����� </param>
        /// <param name="info"> �ظ����ͻ��˵������Ϣ </param>
        public async Task KickOut(int index, WebSocketCloseStatus reason, string info) {
            mutex.Lock();
            await KickOut(index, reason, info, true);
            mutex.Unlock();
        }

        /// <summary>
        /// async ��ĳWebsocket�ͻ���������Ϣ
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        /// <param name="content"> ��Ϣ���� </param>
        public async Task Write(int index, byte[] content) => await Write(index, content, true);

        /// <summary>
        /// async ��ĳWebsocket�ͻ���������Ϣ
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        /// <param name="content"> ��Ϣ���� </param>
        /// <param name="binary"> trueָʾ����ϢΪ������, falseΪ�ı� </param>
        /// <param name="isEndOfMessage"> ָʾ������Ϣ�Ƿ��Ѿ���� </param>
        public async Task Write(int index, byte[] content, bool binary, bool isEndOfMessage = true) {
            mutex.Lock();
            var info = clients[index];
            mutex.Unlock();
            info.mutex.Lock();
            await info.client.WebSocket.SendAsync(new ArraySegment<byte>(content), binary ? WebSocketMessageType.Binary : WebSocketMessageType.Text, isEndOfMessage, info.cancellationToken.Token);
            info.mutex.Unlock();
        }

        /// <summary>
        /// ��ѯ�Ƿ����ָ�����кŵ�Websocket�ͻ���
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        /// <returns> ������������, �򷵻�true </returns>
        public bool IsClientExist(int index) => clients.ContainsKey(index);

        public object GetClient(int index) => GetClientUrl(index);

        public Uri GetClientUrl(int index) {
            var exist = clients.TryGetValue(index, out var value);
            if (exist) {
                return value.client.RequestUri;
            } else {
                return null;
            }
        }

        /// <summary>
        /// ���¿ͻ��˽���ʱ�Ļص�
        /// </summary>
        public event Action<int, object> OnClientConnectedIn;
        /// <summary>
        /// ���¿ͻ��˽���ʱ�Ļص�(WebSocket)
        /// </summary>
        public event Action<int, Uri> OnWebSocketClientConnectedIn;
        /// <summary>
        /// �пͻ��˹ر�����, �Ͽ����ӻ��ߵ�ʱ�Ļص�
        /// </summary>
        public event Action<int> OnClientDisconnected;
        /// <summary>
        /// �յ����Կͻ��˵�����ʱ�ص�
        /// </summary>
        public event Action<int, byte[]> OnReadingClient;

        /// <summary>
        /// �յ�HTTP���� (��Websocket) ʱ�Ļص�, ���ڴ˻ص���ƴ��response�ظ��岢���е���Close
        /// </summary>
        public event OnHttpServerReceived OnHttpServerReceived;
        /// <summary>
        /// �յ����� WebSocked �� HTTP �ͻ��˵�����ʱ�����������ݵĻص����ڱ�����һ�㲻����
        /// </summary>
        public event Action<byte[]> OnReading;

        /// <summary>
        /// (�ڲ�) async �ߵ�ָ���Ŀͻ���
        /// </summary>
        /// <param name="index"> �ͻ������к� </param>
        /// <param name="reason"> �ظ����ͻ��˵��ߵ����� </param>
        /// <param name="info"> �ظ����ͻ��˵������Ϣ </param>
        /// <param name="wait"> �Ƿ�ȴ�, �ڲ���������ͻ��˽��鲻Ҫ�ȴ� </param>
        /// <returns></returns>
        private async Task KickOut(int index, WebSocketCloseStatus reason, string info, bool wait = true) {
            mutex.Lock();
            if (clients.TryGetValue(index, out var websocket)) {
                clients.Remove(index);
                websocket.cancellationToken.Cancel();
                await websocket.client?.WebSocket?.CloseAsync(reason, info, default);
                if (wait && websocket.receiveTask != null) {
                    await websocket.receiveTask;
                }
                websocket.client?.WebSocket?.Dispose();
            } else {
                throw new ArgumentException("index is inexist", "index");
            }
            mutex.Unlock();
        }

        /// <summary>
        /// (�ڲ�) async ����HTTP������������庯��
        /// </summary>
        private async Task AcceptAsync() {
            while (IsRunning) {
                HttpListenerContext context = null;
                while (IsPausing) {
                    await Task.Yield();
                }
                try {
                    context = await self.GetContextAsync();
                } catch (System.Exception) {
                    // todo Ӧ���Դ˴����쳣����ϸ�֣����ֱ���д���ͼ�¼
                    await CheckToRemoveObsoleteConnections();
                    return;
                }
                if (context == null) {

                } else if (context.Request.IsWebSocketRequest) {
                    var newClient = new ClientInfo() {
                        client = null,
                        cancellationToken = new CancellationTokenSource(),
                        receiveTask = null,
                    };
                    var index = 0;
                    mutex.Lock();
                    while (clients.ContainsKey(++index)) {
                    }
                    clients.Add(index, newClient);
                    mutex.Unlock();
                    newClient.receiveTask = WebSocketAcceptAsync(index, newClient, context);
                } else {
                    if (OnHttpServerReceived != null) {
                        // todo ����Ҫ�������߳��Թ�HTTP��Ϣresponse����
                        OnHttpServerReceived.Invoke(context.Request, context.Response, context.User);
                        var buffer = new byte[context.Response.OutputStream.Length];
                        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                        OnReading(buffer);
                    } else {
                        // todo û�м��� HTTP ������Ҫ����
                    }
                }
                await CheckToRemoveObsoleteConnections();
            }
        }

        /// <summary>
        /// (�ڲ�) async ����websocket����������������庯��
        /// </summary>
        /// <param name="index"> �ͻ������к� </param>
        /// <param name="client"> �ͻ�����Ϣ </param>
        /// <param name="context"> Http������������ </param>
        private async Task WebSocketAcceptAsync(int index, ClientInfo client, HttpListenerContext context) {
            var websocketContext = await context.AcceptWebSocketAsync(SubProtocol);
            client.client = websocketContext;
            OnClientConnectedIn?.Invoke(index, context.Request.Url);
            OnWebSocketClientConnectedIn?.Invoke(index, context.Request.Url);
            await ReceiveAsync(index, client, context);
        }

        /// <summary>
        /// (�ڲ�) async ����ĳWebsocket�ͻ�����Ϣ���������庯��
        /// </summary>
        /// <param name="index"> �ͻ������к� </param>
        /// <param name="client"> �ͻ�����Ϣ </param>
        /// <param name="context"> Http������������ </param>
        private async Task ReceiveAsync(int index, ClientInfo client, HttpListenerContext context) {
            while (client.client.WebSocket != null && client.client.WebSocket.State == WebSocketState.Open) {
                if (!IsPausing && client.mutex != null) {
                    var buffer = new byte[BUFFER_SIZE]; // TODO: �Ż��ڴ�ʹ��
                    WebSocketReceiveResult result;
                    try {
                        result = await client.client.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), client.cancellationToken.Token);
                    } catch (WebSocketException) {
                        result = null;
                    }
                    if (result == null || result.MessageType == WebSocketMessageType.Close) {
                        await KickOut(index, WebSocketCloseStatus.EndpointUnavailable, "Server lost your connection", false);
                    } else if (result.Count > 0) {
                        var data = buffer.Take(result.Count).ToArray();
                        OnReadingClient?.Invoke(index, data);
                        OnReading?.Invoke(data);
                    }
                } else {
                    await Task.Yield();
                }
            }
            OnClientDisconnected?.Invoke(index);
            await CheckToRemoveObsoleteConnections();
        }

        /// <summary>
        /// (�ڲ�) async ��������ͻ��˵�ִ����
        /// </summary>
        private async Task CheckToRemoveObsoleteConnections() {
            mutex.Lock();
            var needKicked = new Queue<int>();
            foreach (var i in clients) {
                if (i.Value.client.WebSocket == null || (i.Value.client.WebSocket.State != WebSocketState.Connecting && i.Value.client.WebSocket.State != WebSocketState.Open)) {
                    needKicked.Enqueue(i.Key);
                }
            }
            foreach (var i in needKicked) {
                await KickOut(i, WebSocketCloseStatus.EndpointUnavailable, "Server lost your connection", false);
            }
            mutex.Unlock();
        }

        /// <summary> �����ͻ��˵���Ϣ�� </summary>
        private class ClientInfo {
            /// <summary> Websocket����������, ���а���Websocket�ͻ��������׽��� </summary>
            public HttpListenerWebSocketContext client;
            /// <summary> Websocket�ͻ�����ֹ������Ʒ�, �ڶϿ���ÿͻ��˵�����ʱ�趨��ȡ���շ����� </summary>
            public CancellationTokenSource cancellationToken;
            /// <summary> �ͻ��˽�����Ϣ������ </summary>
            public Task receiveTask;
            /// <summary> ��Դ�� </summary>
            public readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        }

        /// <summary> HTTP������������ </summary>
        private HttpListener self;
        /// <summary> ����������HTTP������ </summary>
        private Task acceptTask;
        /// <summary> ��������Դ�� </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        /// <summary> Websocket�ͻ����б� </summary>
        private readonly Dictionary<int, ClientInfo> clients = new Dictionary<int, ClientInfo>();
    }
}
