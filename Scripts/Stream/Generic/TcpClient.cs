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
    /// TCP 客户端处理类
    /// </summary>
    public class TcpClient : System.Net.Sockets.TcpClient, ITcpClient {
        private const int DEFAULT_SERVER_PORT = 80;

        /// <summary>
        /// 构造空TCP客户端对象.
        /// 参见 <seealso cref="TcpClient()"/>
        /// </summary>
        public TcpClient() : base() {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// 用指定的 AddressFamily 构造TCP客户端对象.
        /// 参见 <seealso cref="TcpClient(AddressFamily)"/>
        /// </summary>
        /// <param name="family"></param>
        public TcpClient(AddressFamily family) : base(family) {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// 用指定的主机名和端口号, 构造TCP客户端对象.
        /// 参见 <seealso cref="TcpClient(string, int)"/>
        /// </summary>
        /// <param name="hostname"> 要绑定的主机名, 等价于对应的IP地址 </param>
        /// <param name="port"> 要绑定的端口号 </param>
        public TcpClient(string hostname, ushort port) : base(hostname, port) {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// 用指定的本地网络位置, 构造TCP客户端对象
        /// 参见 <seealso cref="TcpClient(IPEndPoint)"/>
        /// </summary>
        /// <param name="local"> 要绑定的本机网络位置 </param>
        public TcpClient(IPEndPoint local) : base(local) {
            WaitingTask = ReceiveAsync();
        }

        /// <summary>
        /// (析构) 关掉接收消息的线程
        /// todo 关线程依赖于析构会导致严重的退出时卡顿，需要修改
        /// </summary>
        ~TcpClient() {
            // receiveTask.Wait(); // warning 这个等待无关紧要, 却有可能阻塞垃圾回收器, 所以删除
            Close();
        }

        /// <summary> 收到来自服务器的消息时的回调 </summary>
        public event Action<byte[]> OnReading;
        /// <summary> 连接断开时的回调 </summary>
        public event Action OnDisconnected;
        /// <summary> 本机网络位置 </summary>
        public EndPoint EndPoint => Client.LocalEndPoint;
        public IPEndPoint IPEndPoint => EndPoint as IPEndPoint;
        private IPEndPoint _serverEndPoint = new IPEndPoint(IPAddress.Any, DEFAULT_SERVER_PORT);
        /// <summary> 服务器网络位置 </summary>
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
        /// <summary> 是否已连接, 等同于 <seealso cref="TcpClient.Connected"/> </summary>
        public bool IsRunning => Connected;

        public bool IsPausing { get; private set; }

        /// <summary>
        /// 连接到本机上指定 80 端口的 TCP 服务器
        /// </summary>
        public void Open() => Connect(ServerIPEndPoint);

        /// <summary>
        /// <para> 断开连接, 断开后本对象所有资源将被释放 </para>
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
        /// 获取用于监听消息的任务（线程）
        /// </summary>
        public Task WaitingTask { get; }

        /// <summary>
        /// 发送消息到已连接的服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        public int Write(byte[] content) => Client.Send(content);

        public async Task<int> WriteAsync(byte[] content) {
            var buffer = new ArraySegment<byte>(content);
            return await Client.SendAsync(buffer, SocketFlags.None);
        }

        /// <summary>
        /// (内部) 接收数据的线程/任务函数体
        /// </summary>
        private async Task ReceiveAsync() {
            while (true) {
                bool connected = false;
                while (Connected) {
                    connected = true;
                    if (!IsPausing) {
                        var buffer = new byte[Client.ReceiveBufferSize]; // TODO: 优化内存使用
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
                                case 10054: // 远程主机强迫关闭了一个现有的连接
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
