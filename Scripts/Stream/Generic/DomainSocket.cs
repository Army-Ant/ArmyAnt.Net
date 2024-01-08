using ArmyAnt.Stream.Exception;
using ArmyAnt.Stream.SingleWriteStreams;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic {
    /// <summary>
    /// UDP Socket ������
    /// TODO: �ಥ���㲥������δ����
    /// </summary>
    public class DomainSocket : IDomainSocket {
        /// <summary>
        /// �������Ƿ�������
        /// </summary>
        public bool IsRunning => self != null;

        public bool IsPausing { get; private set; }

        private EndPoint _endPoint;
        /// <summary>
        /// ������IP����λ��, ֻ���ڿ������������
        /// </summary>
        public EndPoint EndPoint => _endPoint;

        public string Path {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// ����UDP����, �˿�ʹ��Ĭ��ֵ <see cref="DEFAULT_PORT"/>
        /// </summary>
        public void Open() {
            mutex.Lock();
            if (self != null) {
                mutex.Unlock();
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            // self = new Socket();
            mutex.Unlock();
            receiveTask = ReceiveAsync();
        }

        /// <summary>
        /// ֹͣUDP��������Ϣ����
        /// </summary>
        public void Close(bool nowait = false) {
            mutex.Lock();
            self.Close();
            self.Dispose();
            self = null;
            if (!nowait && receiveTask != null) {
                receiveTask.Wait();
            }
            receiveTask = null;
            mutex.Unlock();
        }

        public void Pause() {
            IsPausing = true;
        }

        public void Resume() {
            IsPausing = false;
        }

        public Task WaitingTask => receiveTask;

        /// <summary>
        /// async ��ĳ�ͻ��˷�����Ϣ
        /// δ���������������, ���ô˺�����ͬ�ڵ���static���� <see cref="Send(IPEndPoint, byte[], IPEndPoint)"/>
        /// </summary>
        /// <param name="ep"> �ͻ�������λ�� </param>
        /// <param name="content"> ��Ϣ���� </param>
        public async Task<int> WriteAsync(byte[] content) {
            if (self == null) {
                throw new NetworkException(ExceptionType.ServerHasNotStarted);
            }
            mutex.Lock();
            var ret = await self.SendAsync(new ArraySegment<byte>(content), SocketFlags.None);
            mutex.Unlock();
            return ret;
        }

        public int Write(byte[] content) {
            if (self == null) {
                throw new NetworkException(ExceptionType.ServerHasNotStarted);
            }
            mutex.Lock();
            var ret = self.Send(new ArraySegment<byte>(content), SocketFlags.None);
            mutex.Unlock();
            return ret;
        }

        /// <summary> �յ����Է���������Ϣʱ�����������ݵĻص� </summary>
        public event Action<byte[]> OnReading;

        /// <summary>
        /// (�ڲ�) �������ݵ��߳�/��������
        /// </summary>
        private async Task ReceiveAsync() {
            var buffer = new ArraySegment<byte>();
            while (IsRunning) {
                if (IsPausing) {
                    await Task.Yield();
                } else {
                    try {
                        var result = await self.ReceiveAsync(buffer, SocketFlags.None);
                        if (buffer.Count > 0) {
                            var data = buffer.ToArray();
                            OnReading?.Invoke(data);
                        } else {
                            Close(false);
                        }
                    } catch (SocketException) {
                        // TODO: Resolve socket exceptions
                    }
                }
            }
        }

        /// <summary> UDP Socket ������ </summary>
        private Socket self;
        /// <summary> �շ���Ϣ������ </summary>
        private Task receiveTask;
        /// <summary> ��Դ�� </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
    }
}
