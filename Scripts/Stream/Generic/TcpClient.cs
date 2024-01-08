using ArmyAnt.Stream.SingleWriteStreams;
using ArmyAnt.Stream.Exception;

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic
{
    /// <summary>
    /// TCP �ͻ��˴�����
    /// </summary>
    public class TcpClient : System.Net.Sockets.TcpClient, ITcpClient {
        private const int DEFAULT_SERVER_PORT = 80;

        /// <summary>
        /// �����TCP�ͻ��˶���.
        /// �μ� <seealso cref="TcpClient()"/>
        /// </summary>
        public TcpClient() : base() {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// ��ָ���� AddressFamily ����TCP�ͻ��˶���.
        /// �μ� <seealso cref="TcpClient(AddressFamily)"/>
        /// </summary>
        /// <param name="family"></param>
        public TcpClient(AddressFamily family) : base(family) {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// ��ָ�����������Ͷ˿ں�, ����TCP�ͻ��˶���.
        /// �μ� <seealso cref="TcpClient(string, int)"/>
        /// </summary>
        /// <param name="hostname"> Ҫ�󶨵�������, �ȼ��ڶ�Ӧ��IP��ַ </param>
        /// <param name="port"> Ҫ�󶨵Ķ˿ں� </param>
        public TcpClient(string hostname, ushort port) : base(hostname, port) {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// ��ָ���ı�������λ��, ����TCP�ͻ��˶���
        /// �μ� <seealso cref="TcpClient(IPEndPoint)"/>
        /// </summary>
        /// <param name="local"> Ҫ�󶨵ı�������λ�� </param>
        public TcpClient(IPEndPoint local) : base(local) {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// (����) �ص�������Ϣ���߳�
        /// todo ���߳������������ᵼ�����ص��˳�ʱ���٣���Ҫ�޸�
        /// </summary>
        ~TcpClient() {
            // receiveTask.Wait(); // warning ����ȴ��޹ؽ�Ҫ, ȴ�п�����������������, ����ɾ��
            Close();
        }

        /// <summary> �յ����Է���������Ϣʱ�Ļص� </summary>
        public event Action<byte[]> OnReading;
        /// <summary> ���ӶϿ�ʱ�Ļص� </summary>
        public event Action OnDisconnected;
        /// <summary> ��������λ�� </summary>
        public EndPoint EndPoint => Client.LocalEndPoint;
        public IPEndPoint IPEndPoint => EndPoint as IPEndPoint;
        private IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Any, DEFAULT_SERVER_PORT);
        /// <summary> ����������λ�� </summary>
        public IPEndPoint ServerIPEndPoint {
            get {
                if (Connected) {
                    _serverEndPoint = Client.RemoteEndPoint as IPEndPoint;
                }
                return _serverEndPoint;
            }
            set {
                if (!Connected) {
                    _serverEndPoint = value;
                } else {
                    throw new NetworkException(ExceptionType.ServerHasConnected);
                }
            }
        }
        /// <summary> �Ƿ�������, ��ͬ�� <seealso cref="TcpClient.Connected"/> </summary>
        public bool IsRunning => Connected;

        public bool IsPausing { get; private set; }

        /// <summary>
        /// ���ӵ�������ָ�� 80 �˿ڵ� TCP ������
        /// </summary>
        public void Open() => Connect(ServerIPEndPoint);

        /// <summary>
        /// <para> �Ͽ�����, �Ͽ��󱾶���������Դ�����ͷ� </para>
        /// <seealso cref="TcpClient.Close()"/>
        /// </summary>
        public void Close(bool nowait = false) {
            taskEnd = true;
            base.Close();
            Dispose();
            if (!nowait) {
                WaitingTask.Wait();
            }
        }

        public void Pause() {
            IsPausing = true;
        }

        public void Resume() {
            IsPausing = false;
        }

        /// <summary>
        /// ��ȡ���ڼ�����Ϣ�������̣߳�
        /// </summary>
        public Task WaitingTask { get; }

        /// <summary>
        /// ������Ϣ�������ӵķ�����
        /// </summary>
        /// <param name="content"> ��Ϣ���� </param>
        public int Write(byte[] content) => Client.Send(content);

        public async Task<int> WriteAsync(byte[] content) {
            var buffer = new ArraySegment<byte>(content);
            return await Client.SendAsync(buffer, SocketFlags.None);
        }

        /// <summary>
        /// (�ڲ�) �������ݵ��߳�/��������
        /// </summary>
        private async Task ReceiveAsync() {
            while (true) {
                bool connected = false;
                while (Connected) {
                    connected = true;
                    if (!IsPausing) {
                        var buffer = new byte[Client.ReceiveBufferSize]; // TODO: �Ż��ڴ�ʹ��
                        try {
                            var result = Client.Receive(buffer);
                            if (result > 0) {
                                var data = buffer.Take(result).ToArray();
                                OnReading?.Invoke(data);
                            } else {
                                Close();
                            }
                        } catch (SocketException e) {
                            switch (e.ErrorCode) {
                                case 10054: // Զ������ǿ�ȹر���һ�����е�����
                                    Close(true);
                                    break;
                                default:
                                    throw;
                            }
                        }
                    }
                    await Task.Yield();
                }
                if (connected) {
                    OnDisconnected?.Invoke();
                }
                if (taskEnd) {
                    break;
                }
            }
        }

        private bool taskEnd = false;
    }
}
