using ArmyAnt.Stream.SingleWriteStreams;
using ArmyAnt.Stream.Exception;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.Generic {
    /// <summary>
    /// UDP Socket 处理类
    /// TODO: 多播及广播功能暂未测试
    /// </summary>
    public class Udp : IUdp {
        private const int DEFAULT_PORT = 32100;

        /// <summary>
        /// 服务器是否在运行
        /// </summary>
        public bool IsRunning => self != null;

        public bool IsPausing { get; private set; }

        public IPEndPoint DefaultAddress { get; set; }

        private IPEndPoint _endPoint = new IPEndPoint(IPAddress.Any, DEFAULT_PORT);
        /// <summary>
        /// 本机的IP网络位置, 只有在开启监听后可用
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
        /// 服务器本机的网络位置
        /// </summary>
        public EndPoint EndPoint => IPEndPoint;

        /// <summary>
        /// 开启UDP监听, 端口使用默认值 <see cref="DEFAULT_PORT"/>
        /// </summary>
        public void Open() => Open(IPEndPoint);

        /// <summary>
        /// 开启UDP监听
        /// </summary>
        /// <param name="port"> 要开启的端口 </param>
        public void Open(ushort port) => Open(new IPEndPoint(IPEndPoint.Address, port));

        /// <summary>
        /// 开启UDP监听
        /// </summary>
        /// <param name="ep"> 要监听的本机网络位置, 调整此参数以决定本机端口号及可访问方式 </param>
        /// <param name="broadcast"> 是否接收广播 </param>
        /// <param name="multicast"> 是否接收多播 </param>
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
        /// 停止UDP监听和消息接收
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
        /// async 向某客户端发送消息
        /// 未开启监听的情况下, 调用此函数等同于调用static函数 <see cref="Send(IPEndPoint, byte[], IPEndPoint)"/>
        /// </summary>
        /// <param name="ep"> 客户端网络位置 </param>
        /// <param name="content"> 消息正文 </param>
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
        /// async 阻塞式向某客户端发送UDP消息
        /// </summary>
        /// <param name="remote"> 远程网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        /// <param name="local"> 本机网络位置, 若填null, 则分配默认地址 <seealso cref="IPAddress.Any"/> 和随机端口 </param>
        public static int Write(IPEndPoint remote, byte[] content, IPEndPoint local) {
            if (local != null) {
                return new UdpClient(local).Send(content, content.Length, remote);
            } else {
                return new UdpClient().Send(content, content.Length, remote);
            }
        }

        /// <summary>
        /// async 向某客户端发送UDP消息
        /// </summary>
        /// <param name="remote"> 远程网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        /// <param name="local"> 本机网络位置, 若填null, 则分配默认地址 <seealso cref="IPAddress.Any"/> 和随机端口 </param>
        public static async Task<int> WriteAsync(IPEndPoint remote, byte[] content, IPEndPoint local) {
            if (local != null) {
                return await new UdpClient(local).SendAsync(content, content.Length, remote);
            } else {
                return await new UdpClient().SendAsync(content, content.Length, remote);
            }
        }

        /// <summary>
        /// 加入多播组
        /// </summary>
        /// <param name="addr"> 要加入的多播组地址 </param>
        /// <param name="timeToLive"> 生存时间 </param>
        /// <returns> 是否加入成功 </returns>
        public bool JoinMulticastGroup(IPAddress addr, int timeToLive) {
            try {
                self.JoinMulticastGroup(addr, timeToLive);
            } catch (ArgumentException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 加入多播组
        /// </summary>
        /// <param name="addr"> 要加入的多播组地址 </param>
        /// <returns> 是否全部加入成功 </returns>
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
        /// 退出多播组
        /// </summary>
        /// <param name="addr"> 多播组地址 </param>
        public void LeaveMulticastGroup(params IPAddress[] addr) {
            foreach (var i in addr) {
                self.DropMulticastGroup(i);
            }
        }

        /// <summary> 收到数据时回调 </summary>
        public event System.Action<IPEndPoint, byte[]> OnReceived;
        /// <summary> 收到来自服务器的消息时，仅分析数据的回调 </summary>
        public event Action<byte[]> OnReading;

        /// <summary>
        /// (内部) 接收数据的线程/任务函数体
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
                            case 10060:     // 接收超时
                                break;
                            default:
                                break;
                        }
                    }
                }
                await Task.Yield();
            }
        }

        /// <summary> UDP Socket 处理体 </summary>
        private UdpClient self;
        /// <summary> 收发消息任务句柄 </summary>
        private Task receiveTask;
        /// <summary> 资源锁 </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
    }
}
