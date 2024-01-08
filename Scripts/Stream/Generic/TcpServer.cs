using ArmyAnt.Stream.MultiWriteStreams;
using ArmyAnt.Stream.Exception;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic
{
    /// <summary>
    /// TCP ������������
    /// </summary>
    public class TcpServer : ITcpServer {
        private const int DEFAULT_PORT = 32766;
        protected const int BUFFER_SIZE = 8192;

        /// <summary>
        /// �������Ƿ�������
        /// </summary>
        public bool IsRunning => self != null;

        public bool IsPausing { get; private set; }

        /// <summary>
        /// ����������������λ��
        /// </summary>
        private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, DEFAULT_PORT);

        /// <summary> ��������λ�� </summary>
        public IPEndPoint IPEndPoint {
            get {
                if (self != null) {
                    _endPoint = self.LocalEndpoint as IPEndPoint;
                }
                return _endPoint;
            }
            set {
                if (self == null) {
                    _endPoint = value;
                } else {
                    throw new NetworkException(ExceptionType.ServerHasConnected);
                }
            }
        }

        /// <summary>
        /// ����������������λ��
        /// </summary>
        public EndPoint EndPoint => IPEndPoint;

        /// <summary>
        /// ����TCP������
        /// </summary>
        public void Open() => Open(IPEndPoint);

        /// <summary>
        /// ����TCP������
        /// </summary>
        /// <param name="ep"> �������ն�����λ��, �����˲����Ծ����������˿ںż��ɷ��ʷ�ʽ </param>
        public void Open(IPEndPoint local) {
            mutex.Lock();
            if (self != null) {
                mutex.Unlock();
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            var listener = new TcpListener(local);
            listener.Start();
            self = listener;
            mutex.Unlock();
            acceptTask = AcceptAsync();
        }

        /// <summary>
        /// ֹͣTCP������, �Ͽ����пͻ�������
        /// </summary>
        public void Close(bool nowait = false) {
            mutex.Lock();
            foreach (var i in clients) {
                i.Value.cancellationToken.Cancel();
                i.Value.client.Close();
                if (!nowait) {
                    i.Value.receiveTask.Wait();
                }
            }
            clients.Clear();
            self.Stop();
            self = null;
            if (!nowait) {
                acceptTask?.Wait();
            }
            acceptTask = null;
            mutex.Unlock();
        }

        public void Pause() {
            IsPausing = true;
        }

        public void Resume() {
            IsPausing = false;
        }

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
        /// async �ߵ�ָ���ͻ���
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        public async Task KickOut(int index) {
            mutex.Lock();
            var client = clients[index];
            await KickOut(index, client);
            mutex.Unlock();
        }

        /// <summary>
        /// async ��ĳ�ͻ��˷�����Ϣ
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        /// <param name="content"> ��Ϣ���� </param>
        public async Task Write(int index, byte[] content) {
            mutex.Lock();
            var client = clients[index];
            mutex.Unlock();
            client.mutex.Lock();
            await client.client.GetStream().WriteAsync(content, 0, content.Length);
            client.mutex.Unlock();
        }

        /// <summary>
        /// ��ѯ�Ƿ����ָ�����кŵĿͻ���
        /// </summary>
        /// <param name="index"> �ͻ������к�, ������ʱ��� </param>
        /// <returns> ������������, �򷵻�true </returns>
        public bool IsClientExist(int index) => clients.ContainsKey(index);

        public object GetClient(int index) {
            if (clients.TryGetValue(index, out var ret)) {
                return ret.client.Client.RemoteEndPoint;
            }
            return null;
        }

        public IPEndPoint GetTcpClient(int index) {
            if (clients.TryGetValue(index, out var ret)) {
                return ret.client.Client.RemoteEndPoint as IPEndPoint;
            }
            return null;
        }

        /// <summary>
        /// ���¿ͻ��˽���ʱ�Ļص�
        /// </summary>
        public event Action<int, object> OnClientConnectedIn;
        /// <summary>
        /// ���¿ͻ��˽���ʱ�Ļص�
        /// </summary>
        public event Action<int, IPEndPoint> OnTCPClientConnectedIn;
        /// <summary>
        /// �пͻ��˹ر�����, �Ͽ����ӻ��ߵ�ʱ�Ļص�
        /// </summary>
        public event Action<int> OnClientDisconnected;
        /// <summary>
        /// �յ����Կͻ��˵�����ʱ�ص�
        /// </summary>
        public event Action<int, byte[]> OnReadingClient;
        /// <summary> �յ����Կͻ��˵���Ϣʱ�����������ݵĻص� </summary>
        public event Action<byte[]> OnReading;

        /// <summary>
        /// (�ڲ�) async �ߵ�ָ���Ŀͻ���
        /// </summary>
        /// <param name="index">�ͻ������к�</param>
        /// <param name="client">�ͻ�����Ϣ</param>
        /// <param name="wait"> �Ƿ�ȴ�, �ڲ���������ͻ��˽��鲻Ҫ�ȴ� </param>
        private async Task KickOut(int index, ClientInfo client, bool wait = true) {
            client.cancellationToken.Cancel();
            client.client.Close();
            clients.Remove(index);
            if (wait) {
                await client.receiveTask;
            }
        }

        /// <summary>
        /// (�ڲ�) async �������ӵ��������庯��
        /// </summary>
        private async Task AcceptAsync() {
            while (IsRunning) {
                if (!IsPausing) {
                    var client = await self.AcceptTcpClientAsync();
                    var index = 0;
                    mutex.Lock();
                    while (clients.ContainsKey(++index)) {
                    }
                    OnClientConnectedIn?.Invoke(index, client.Client.RemoteEndPoint);
                    OnTCPClientConnectedIn?.Invoke(index, client.Client.RemoteEndPoint as IPEndPoint);
                    client.ReceiveBufferSize = BUFFER_SIZE;
                    var newClient = new ClientInfo() {
                        client = client,
                        cancellationToken = new CancellationTokenSource(),
                        receiveTask = null,
                    };
                    clients.Add(index, newClient);
                    mutex.Unlock();
                    newClient.receiveTask = ReceiveAsync(index, newClient);
                    await CheckToRemoveObsoleteConnections();
                }
                await Task.Yield();
            }
        }

        /// <summary>
        /// (�ڲ�) async ����ĳ�ͻ�����Ϣ���������庯��
        /// </summary>
        /// <param name="index"> �ͻ������к� </param>
        /// <param name="client"> �ͻ�����Ϣ </param>
        private async Task ReceiveAsync(int index, ClientInfo client) {
            while (client.client.Connected) {
                if (!IsPausing && client.mutex != null) {
                    var buffer = new byte[client.client.ReceiveBufferSize]; // TODO: �Ż��ڴ�ʹ��
                    var result = await Task.Run(() => {
                        try {
                            return client.client.Client.Receive(buffer);
                        } catch (SocketException e) {
                            switch (e.ErrorCode) {
                                case 10054: // Զ������ǿ�ȹر���һ�����е�����
                                    return 0;
                                default:
                                    throw;
                            }
                        }
                    });
                    if (result > 0) {
                        var data = buffer.Take(result).ToArray();
                        OnReadingClient?.Invoke(index, data);
                        OnReading?.Invoke(data);
                    } else {
                        await KickOut(index, client, false);
                    }
                }
                await Task.Yield();
            }
            await CheckToRemoveObsoleteConnections();
            OnClientDisconnected?.Invoke(index);
        }

        /// <summary>
        /// (�ڲ�) async ��������ͻ��˵�ִ����
        /// </summary>
        private async Task CheckToRemoveObsoleteConnections() {
            mutex.Lock();
            var needKicked = new Queue<KeyValuePair<int, ClientInfo>>();
            foreach (var i in clients) {
                if (!i.Value.client.Connected) {
                    needKicked.Enqueue(new KeyValuePair<int, ClientInfo>(i.Key, i.Value));
                }
            }
            foreach (var i in needKicked) {
                await KickOut(i.Key, i.Value, false);
            }
            mutex.Unlock();
        }

        /// <summary> �����ͻ��˵���Ϣ�� </summary>
        private class ClientInfo {
            /// <summary> �ͻ��������� </summary>
            public System.Net.Sockets.TcpClient client;
            /// <summary> �ͻ�����ֹ������Ʒ�, �ڶϿ���ÿͻ��˵�����ʱ�趨��ȡ���շ����� </summary>
            public CancellationTokenSource cancellationToken;
            /// <summary> �ͻ��˽�����Ϣ������ </summary>
            public Task receiveTask;
            /// <summary> ��Դ�� </summary>
            public readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        }

        /// <summary> ������������ </summary>
        private TcpListener self;
        /// <summary> �������������������� </summary>
        private Task acceptTask;
        /// <summary> ��������Դ�� </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        /// <summary> �ͻ����б� </summary>
        private readonly Dictionary<int, ClientInfo> clients = new Dictionary<int, ClientInfo>();
    }
}
