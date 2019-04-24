using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Network {
    /// <summary>
    /// UDP Socket 基础类
    /// TODO: 多播及广播功能暂未测试
    /// </summary>
    public class SocketUdp : IUdpNetwork {
        private const int DEFAULT_PORT = 32767;

        /// <summary>
        /// 服务器是否在运行
        /// </summary>
        public bool IsStarting => self != null;

        /// <summary>
        /// 本机的IP网络位置, 只有在开启监听后可用
        /// </summary>
        public IPEndPoint IPEndPoint => self?.Client?.LocalEndPoint as IPEndPoint;

        /// <summary>
        /// 开启UDP监听, 端口使用默认值 <see cref="DEFAULT_PORT"/>
        /// </summary>
        public void Start() => Start(DEFAULT_PORT);

        /// <summary>
        /// 开启UDP监听
        /// </summary>
        /// <param name="port"> 要开启的端口 </param>
        public void Start(ushort port) => Start(new IPEndPoint(IPAddress.Any, port));

        /// <summary>
        /// 开启UDP监听
        /// </summary>
        /// <param name="ep"> 要监听的本机网络位置, 调整此参数以决定本机端口号及可访问方式 </param>
        /// <param name="broadcast"> 是否接收广播 </param>
        /// <param name="multicast"> 是否接收多播 </param>
        public void Start(IPEndPoint ep, bool broadcast = true, bool multicast = true) {
            mutex.Lock();
            if(self != null) {
                mutex.Unlock();
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            var udp = new UdpClient(ep);
            udp.EnableBroadcast = broadcast;
            udp.MulticastLoopback = multicast;
            self = udp;
            mutex.Unlock();
            receiveTask = new Task(ReceiveAsync);
            receiveTask.Start();
        }

        /// <summary>
        /// 停止UDP监听和消息接收
        /// </summary>
        public void Stop() {
            mutex.Lock();
            self.Close();
            self.Dispose();
            self = null;
            receiveTask?.Wait();
            receiveTask = null;
            mutex.Unlock();
        }

        /// <summary>
        /// async 向某客户端发送消息
        /// 未开启监听的情况下, 调用此函数等同于调用static函数 <see cref="Send(IPEndPoint, byte[], IPEndPoint)"/>
        /// </summary>
        /// <param name="ep"> 客户端网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        public async Task<int> Send(IPEndPoint ep, byte[] content) {
            if(self == null) {
                return await Send(ep, content, IPEndPoint);
            }
            mutex.Lock();
            var ret = await self.SendAsync(content, content.Length, ep);
            mutex.Unlock();
            return ret;
        }

        /// <summary>
        /// async 向某客户端发送UDP消息
        /// </summary>
        /// <param name="remote"> 远程网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        /// <param name="local"> 本机网络位置, 若填null, 则分配默认地址 <seealso cref="IPAddress.Any"/> 和随机端口 </param>
        public static async Task<int> Send(IPEndPoint remote, byte[] content, IPEndPoint local) {
            if(local != null) {
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
            } catch(System.ArgumentException) {
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
                foreach(var i in addr) {
                    self.JoinMulticastGroup(i);
                }
            } catch(System.ArgumentException) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 退出多播组
        /// </summary>
        /// <param name="addr"> 多播组地址 </param>
        public void LeaveMulticastGroup(params IPAddress[] addr) {
            foreach(var i in addr) {
                self.DropMulticastGroup(i);
            }
        }

        /// <summary> 收到数据时回调 </summary>
        public Callback.OnIPClientReceived OnClientReceived { get; set; }

        /// <summary>
        /// (内部) 接收数据的线程/任务函数体
        /// </summary>
        private void ReceiveAsync() {
            while(self != null) {
                System.Threading.Thread.Sleep(1);
                var buffer = new byte[self.Client.ReceiveBufferSize]; // TODO: 优化内存使用
                IPEndPoint remote = new IPEndPoint(IPEndPoint.Address, IPEndPoint.Port);
                try {
                    var result = self.Receive(ref remote);
                    if(result.Length > 0) {
                        OnClientReceived(remote, result);
                    }
                } catch(SocketException) {
                    // TODO: Resolve socket exceptions
                }
            }
        }

        /// <summary> UDP Socket 处理体 </summary>
        private UdpClient self;
        /// <summary> 收发消息任务句柄 </summary>
        private Task receiveTask;
        /// <summary> 资源锁 </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock(1, 1);
    }
}
