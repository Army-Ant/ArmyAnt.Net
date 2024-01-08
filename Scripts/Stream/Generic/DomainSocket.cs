using ArmyAnt.Stream.Exception;
using ArmyAnt.Stream.SingleWriteStreams;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic {
    /// <summary>
    /// UDP Socket 处理类
    /// TODO: 多播及广播功能暂未测试
    /// </summary>
    public class DomainSocket : IDomainSocket {
        /// <summary>
        /// 服务器是否在运行
        /// </summary>
        public bool IsRunning => self != null;

        public bool IsPausing { get; private set; }

        private EndPoint _endPoint;
        /// <summary>
        /// 本机的IP网络位置, 只有在开启监听后可用
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
        /// 开启UDP监听, 端口使用默认值 <see cref="DEFAULT_PORT"/>
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
        /// 停止UDP监听和消息接收
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
        /// async 向某客户端发送消息
        /// 未开启监听的情况下, 调用此函数等同于调用static函数 <see cref="Send(IPEndPoint, byte[], IPEndPoint)"/>
        /// </summary>
        /// <param name="ep"> 客户端网络位置 </param>
        /// <param name="content"> 消息正文 </param>
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

        /// <summary> 收到来自服务器的消息时，仅分析数据的回调 </summary>
        public event Action<byte[]> OnReading;

        /// <summary>
        /// (内部) 接收数据的线程/任务函数体
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

        /// <summary> UDP Socket 处理体 </summary>
        private Socket self;
        /// <summary> 收发消息任务句柄 </summary>
        private Task receiveTask;
        /// <summary> 资源锁 </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
    }
}
