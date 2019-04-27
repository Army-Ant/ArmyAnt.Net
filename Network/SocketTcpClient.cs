using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ArmyAnt.Network {
    public class SocketTcpClient : TcpClient, ITcpNetworkClient, ISocketNetworkClient {
        /// <summary> 收到来自服务器的消息时的回调 </summary>
        public OnIPClientReceived OnClientReceived { get; set; }
        /// <summary> 连接断开时的回调 </summary>
        public OnTcpClientDisonnected OnTcpClientDisonnected { get; set; }
        /// <summary> 本机网络位置 </summary>
        public IPEndPoint IPEndPoint => Client.LocalEndPoint as IPEndPoint;
        /// <summary> 服务器网络位置 </summary>
        public IPEndPoint ServerIPEndPoint => Client.RemoteEndPoint as IPEndPoint;
        /// <summary> 是否已连接, 等同于 <seealso cref="SocketTcpClient.Connected"/> </summary>
        public bool IsStarting => Connected;

        /// <summary>
        /// 构造空TCP客户端对象.
        /// 参见 <seealso cref="TcpClient()"/>
        /// </summary>
        public SocketTcpClient() : base() {
            WaitingTask = Task.Run(ReceiveAsync);
        }

        /// <summary>
        /// 用指定的 AddressFamily 构造TCP客户端对象.
        /// 参见 <seealso cref="TcpClient(AddressFamily)"/>
        /// </summary>
        /// <param name="family"></param>
        public SocketTcpClient(AddressFamily family) : base(family) {
            WaitingTask = Task.Run(ReceiveAsync);
        }

        /// <summary>
        /// 用指定的主机名和端口号, 构造TCP客户端对象.
        /// 参见 <seealso cref="TcpClient(string, int)"/>
        /// </summary>
        /// <param name="hostname"> 要绑定的主机名, 等价于对应的IP地址 </param>
        /// <param name="port"> 要绑定的端口号 </param>
        public SocketTcpClient(string hostname, ushort port) : base(hostname, port) {
            WaitingTask = Task.Run(ReceiveAsync);
        }

        /// <summary>
        /// 用指定的本地网络位置, 构造TCP客户端对象
        /// 参见 <seealso cref="TcpClient(IPEndPoint)"/>
        /// </summary>
        /// <param name="local"> 要绑定的本机网络位置 </param>
        public SocketTcpClient(IPEndPoint local) : base(local) {
            WaitingTask = Task.Run(ReceiveAsync);
        }

        /// <summary>
        /// (析构) 关掉接收消息的线程
        /// </summary>
        ~SocketTcpClient() {
            // receiveTask.Wait(); // 这个等待无关紧要, 倒是有可能阻塞垃圾回收器, 所以删除
            Stop();
        }

        /// <summary>
        /// 连接到本机上指定端口号的 TCP 服务器
        /// </summary>
        /// <param name="port"> 要连接的服务器端口号 </param>
        public void Start(ushort port) => Connect(IPAddress.Loopback, port);

        /// <summary>
        /// <para> 断开连接, 断开后本对象所有资源将被释放 </para>
        /// <seealso cref="TcpClient.Close()"/>
        /// </summary>
        public void Stop(bool nowait = false) {
            taskEnd = true;
            Close();
            Dispose();
            if(!nowait) {
                WaitingTask.Wait();
            }
        }

        public Task WaitingTask { get; }

        /// <summary>
        /// async 异步连接到指定服务器, 这是对标准库 ConnectAsync 重载参数的一个补充.
        /// 参见<seealso cref="TcpClient.Connect(IPEndPoint)"/>
        /// </summary>
        /// <param name="server"> 服务器网络位置 </param>
        public async Task ConnectAsync(IPEndPoint server) => await ConnectAsync(server.Address, server.Port);

        /// <summary>
        /// 发送消息到已连接的服务器
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        public void Send(byte[] content) => GetStream().Write(content, 0, content.Length);

        /// <summary>
        /// (内部) 接收数据的线程/任务函数体
        /// </summary>
        private void ReceiveAsync() {
            if(Connected) {
                var buffer = new byte[Client.ReceiveBufferSize]; // TODO: 优化内存使用
                try {
                    var result = GetStream().Read(buffer, 0, Client.ReceiveBufferSize);
                    if(result > 0) {
                        OnClientReceived(ServerIPEndPoint, buffer);
                    } else {
                        Stop();
                    }
                } catch(SocketException) {
                    // TODO: Resolve exceptions
                }
            } else {
                System.Threading.Thread.Sleep(1);
            }
            if(taskEnd) {
                OnTcpClientDisonnected();
            } else {
                ReceiveAsync();
            }
        }

        private bool taskEnd = false;
    }
}
