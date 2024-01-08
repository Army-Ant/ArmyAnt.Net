using ArmyAnt.Stream.SingleWriteStreams;
using ArmyAnt.Stream.Exception;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic {
    /// <summary>
    /// UDP Socket ������
    /// TODO: �ಥ���㲥������δ����
    /// </summary>
    public class Udp : IUdp {
        private const int DEFAULT_PORT = 32100;

        /// <summary>
        /// �������Ƿ�������
        /// </summary>
        public bool IsRunning => self != null;

        public bool IsPausing { get; private set; }

        public IPEndPoint DefaultAddress { get; set; }

        private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, DEFAULT_PORT);
        /// <summary>
        /// ������IP����λ��, ֻ���ڿ������������
        /// </summary>
        public IPEndPoint IPEndPoint {
            get {
                if (self?.Client != null) {
                    _endPoint = self?.Client?.LocalEndPoint as IPEndPoint;
                }
                return _endPoint;
            }
            set {
                if (self?.Client == null) {
                    _endPoint = value;
                } else {
                    throw new NetworkException(ExceptionType.ServerHasNotStopped);
                }
            }
        }

        /// <summary>
        /// ����������������λ��
        /// </summary>
        public EndPoint EndPoint => IPEndPoint;

        /// <summary>
        /// ����UDP����, �˿�ʹ��Ĭ��ֵ <see cref="DEFAULT_PORT"/>
        /// </summary>
        public void Open() => Open(IPEndPoint);

        /// <summary>
        /// ����UDP����
        /// </summary>
        /// <param name="port"> Ҫ�����Ķ˿� </param>
        public void Open(ushort port) => Open(new IPEndPoint(IPEndPoint.Address, port));

        /// <summary>
        /// ����UDP����
        /// </summary>
        /// <param name="ep"> Ҫ�����ı�������λ��, �����˲����Ծ��������˿ںż��ɷ��ʷ�ʽ </param>
        /// <param name="broadcast"> �Ƿ���չ㲥 </param>
        /// <param name="multicast"> �Ƿ���նಥ </param>
        public void Open(IPEndPoint ep, bool broadcast = true, bool multicast = true) {
            mutex.Lock();
            if (self != null) {
                mutex.Unlock();
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            var udp = new UdpClient(ep);
            udp.EnableBroadcast = broadcast;
            udp.MulticastLoopback = multicast;
            //udp.Client.ReceiveTimeout = RECEIVE_TIMEOUT;
            self = udp;
            mutex.Unlock();
            receiveTask = Task.Run(ReceiveAsync);
        }

        /// <summary>
        /// ֹͣUDP��������Ϣ����
        /// </summary>
        public void Close(bool nowait = false) {
            mutex.Lock();
            var udp = self;
            self = null;
            mutex.Unlock();
            udp.Client.Blocking = true;
            udp.Close();
            udp.Dispose();
            if (!nowait && receiveTask != null) {
                receiveTask.Wait();
            }
            receiveTask = null;
        }

        public void Pause() {
            IsPausing = true;
        }
        public void Resume() {
            IsPausing = false;
        }

        public Task WaitingTask => receiveTask;

        public int Write(IPEndPoint ep, byte[] content) {
            if (self == null) {
                return Write(ep, content, IPEndPoint);
            }
            mutex.Lock();
            var ret = self.Send(content, content.Length, ep);
            mutex.Unlock();
            return ret;
        }

        /// <summary>
        /// async ��ĳ�ͻ��˷�����Ϣ
        /// δ���������������, ���ô˺�����ͬ�ڵ���static���� <see cref="Send(IPEndPoint, byte[], IPEndPoint)"/>
        /// </summary>
        /// <param name="ep"> �ͻ�������λ�� </param>
        /// <param name="content"> ��Ϣ���� </param>
        public async Task<int> WriteAsync(IPEndPoint ep, byte[] content) {
            if (self == null) {
                return await WriteAsync(ep, content, IPEndPoint);
            }
            mutex.Lock();
            var ret = await self.SendAsync(content, content.Length, ep);
            mutex.Unlock();
            return ret;
        }

        public int Write(byte[] content) {
            if (self == null) {
                return Write(DefaultAddress, content, IPEndPoint);
            }
            mutex.Lock();
            var ret = self.Send(content, content.Length, DefaultAddress);
            mutex.Unlock();
            return ret;
        }

        public async Task<int> WriteAsync(byte[] content) {
            if (self == null) {
                return await WriteAsync(DefaultAddress, content, IPEndPoint);
            }
            mutex.Lock();
            var ret = await self.SendAsync(content, content.Length, DefaultAddress);
            mutex.Unlock();
            return ret;
        }

        /// <summary>
        /// async ����ʽ��ĳ�ͻ��˷���UDP��Ϣ
        /// </summary>
        /// <param name="remote"> Զ������λ�� </param>
        /// <param name="content"> ��Ϣ���� </param>
        /// <param name="local"> ��������λ��, ����null, �����Ĭ�ϵ�ַ <seealso cref="IPAddress.Any"/> ������˿� </param>
        public static int Write(IPEndPoint remote, byte[] content, IPEndPoint local) {
            if (local != null) {
                return new UdpClient(local).Send(content, content.Length, remote);
            } else {
                return new UdpClient().Send(content, content.Length, remote);
            }
        }

        /// <summary>
        /// async ��ĳ�ͻ��˷���UDP��Ϣ
        /// </summary>
        /// <param name="remote"> Զ������λ�� </param>
        /// <param name="content"> ��Ϣ���� </param>
        /// <param name="local"> ��������λ��, ����null, �����Ĭ�ϵ�ַ <seealso cref="IPAddress.Any"/> ������˿� </param>
        public static async Task<int> WriteAsync(IPEndPoint remote, byte[] content, IPEndPoint local) {
            if (local != null) {
                return await new UdpClient(local).SendAsync(content, content.Length, remote);
            } else {
                return await new UdpClient().SendAsync(content, content.Length, remote);
            }
        }

        /// <summary>
        /// ����ಥ��
        /// </summary>
        /// <param name="addr"> Ҫ����Ķಥ���ַ </param>
        /// <param name="timeToLive"> ����ʱ�� </param>
        /// <returns> �Ƿ����ɹ� </returns>
        public bool JoinMulticastGroup(IPAddress addr, int timeToLive) {
            try {
                self.JoinMulticastGroup(addr, timeToLive);
            } catch (ArgumentException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// ����ಥ��
        /// </summary>
        /// <param name="addr"> Ҫ����Ķಥ���ַ </param>
        /// <returns> �Ƿ�ȫ������ɹ� </returns>
        public bool JoinMulticastGroup(params IPAddress[] addr) {
            try {
                foreach (var i in addr) {
                    self.JoinMulticastGroup(i);
                }
            } catch (ArgumentException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// �˳��ಥ��
        /// </summary>
        /// <param name="addr"> �ಥ���ַ </param>
        public void LeaveMulticastGroup(params IPAddress[] addr) {
            foreach (var i in addr) {
                self.DropMulticastGroup(i);
            }
        }

        /// <summary> �յ�����ʱ�ص� </summary>
        public event System.Action<IPEndPoint, byte[]> OnReceived;
        /// <summary> �յ����Է���������Ϣʱ�����������ݵĻص� </summary>
        public event Action<byte[]> OnReading;

        /// <summary>
        /// (�ڲ�) �������ݵ��߳�/��������
        /// </summary>
        private async Task ReceiveAsync() {
            while (IsRunning) {
                if (!IsPausing) {
                    try {
                        var ep = new IPEndPoint(IPEndPoint.Address, IPEndPoint.Port);
                        var result = self.Receive(ref ep);
                        if (result.Length > 0) {
                            OnReceived?.Invoke(ep, result);
                            OnReading?.Invoke(result);
                        } else {
                            Close(false);
                        }
                    } catch (SocketException e) {
                        // TODO: Resolve socket exceptions
                        switch (e.ErrorCode) {
                            case 10060:     // ���ճ�ʱ
                                break;
                            default:
                                break;
                        }
                    }
                }
                await Task.Yield();
            }
        }

        /// <summary> UDP Socket ������ </summary>
        private UdpClient self;
        /// <summary> �շ���Ϣ������ </summary>
        private Task receiveTask;
        /// <summary> ��Դ�� </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
    }
}
