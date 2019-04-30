using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ArmyAnt.Network {
    /// <summary>
    /// TCP Socket 基础类
    /// </summary>
    public class SocketTcpServer : ITcpNetworkServer, IPSocket {
        private const int DEFAULT_PORT = 32766;
        protected const int BUFFER_SIZE = 8192;

        /// <summary>
        /// 服务器是否在运行
        /// </summary>
        public bool IsStarting => self != null;

        /// <summary>
        /// 服务器本机的网络位置, 只有在开启运行后可用
        /// </summary>
        public IPEndPoint IPEndPoint => self?.LocalEndpoint as IPEndPoint;

        /// <summary>
        /// 开启TCP服务器, 端口使用默认值 <see cref="DEFAULT_PORT"/>
        /// </summary>
        public void Start() => Start(DEFAULT_PORT);

        /// <summary>
        /// 开启TCP服务器
        /// </summary>
        /// <param name="port"> 服务器端口号 </param>
        public void Start(ushort port) => Start(new IPEndPoint(IPAddress.Any, port));

        /// <summary>
        /// 开启TCP服务器
        /// </summary>
        /// <param name="ep"> 服务器终端网络位置, 调整此参数以决定服务器端口号及可访问方式 </param>
        public void Start(IPEndPoint ep) {
            mutex.Lock();
            if(self != null) {
                mutex.Unlock();
                throw new NetworkException(ExceptionType.ServerHasNotStopped);
            }
            var listener = new TcpListener(ep);
            listener.Start();
            self = listener;
            mutex.Unlock();
            acceptTask = AcceptAsync();
        }

        /// <summary>
        /// 停止TCP服务器, 断开所有客户端连接
        /// </summary>
        public void Stop(bool nowait = false) {
            mutex.Lock();
            foreach(var i in clients) {
                i.Value.cancellationToken.Cancel();
                i.Value.client.Close();
                if(!nowait) {
                    i.Value.receiveTask.Wait();
                }
            }
            clients.Clear();
            self.Stop();
            self = null;
            if(!nowait) {
                acceptTask?.Wait();
            }
            acceptTask = null;
            mutex.Unlock();
        }

        public (Task main, List<Task> clients) WaitingTask {
            get {
                var ret = new List<Task>();
                mutex.Lock();
                foreach(var i in clients) {
                    ret.Add(i.Value.receiveTask);
                }
                mutex.Unlock();
                return (acceptTask, ret);
            }
        }

        /// <summary>
        /// async 踢掉指定客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        public async Task KickOut(int index) {
            mutex.Lock();
            var client = clients[index];
            await KickOut(index, client);
            mutex.Unlock();
        }

        /// <summary>
        /// async 向某客户端发送消息
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <param name="content"> 消息正文 </param>
        public async Task Send(int index, byte[] content) {
            mutex.Lock();
            var client = clients[index];
            mutex.Unlock();
            client.mutex.Lock();
            await client.client.GetStream().WriteAsync(content, 0, content.Length);
            client.mutex.Unlock();
        }

        /// <summary>
        /// 查询是否存在指定序列号的客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <returns> 存在于连接中, 则返回true </returns>
        public bool IsClientExist(int index) => clients.ContainsKey(index);

        /// <summary>
        /// 有新客户端接入时的回调
        /// </summary>
        public OnTcpServerConnected OnTcpServerConnected { get; set; }
        /// <summary>
        /// 有客户端关闭连接, 断开连接或被踢掉时的回调
        /// </summary>
        public OnTcpServerDisonnected OnTcpServerDisonnected { get; set; }
        /// <summary>
        /// 收到来自客户端的数据时回调
        /// </summary>
        public OnTcpServerReceived OnTcpServerReceived { get; set; }

        /// <summary>
        /// (内部) async 踢掉指定的客户端
        /// </summary>
        /// <param name="index">客户端序列号</param>
        /// <param name="client">客户端信息</param>
        /// <param name="wait"> 是否等待, 内部清理废弃客户端建议不要等待 </param>
        private async Task KickOut(int index, ClientInfo client, bool wait = true) {
            client.cancellationToken.Cancel();
            client.client.Close();
            clients.Remove(index);
            if(wait) {
                await client.receiveTask;
            }
        }

        /// <summary>
        /// (内部) async 接受连接的任务主体函数
        /// </summary>
        private async Task AcceptAsync() {
            var client = await self.AcceptTcpClientAsync();
            var index = 0;
            mutex.Lock();
            while(clients.ContainsKey(++index)) {
            }
            if(!OnTcpServerConnected(index, client.Client.RemoteEndPoint as IPEndPoint)) {
                mutex.Unlock();
                client.Close();
            } else {
                client.ReceiveBufferSize = BUFFER_SIZE;
                var newClient = new ClientInfo() {
                    client = client,
                    cancellationToken = new CancellationTokenSource(),
                    receiveTask = null,
                };
                clients.Add(index, newClient);
                mutex.Unlock();
                newClient.receiveTask = ReceiveAsync(index, newClient);
            }
            await CheckToRemoveObsoleteConnections();
            if(self != null) {
                await AcceptAsync();
            }
        }

        /// <summary>
        /// (内部) async 接受某客户端消息的任务主体函数
        /// </summary>
        /// <param name="index"> 客户端序列号 </param>
        /// <param name="client"> 客户端信息 </param>
        private async Task ReceiveAsync(int index, ClientInfo client) {
            if(client.mutex != null) {
                var buffer = new byte[client.client.ReceiveBufferSize]; // TODO: 优化内存使用
                var result = await Task.Run(() => {
                    try {
                        return client.client.Client.Receive(buffer);
                    } catch(SocketException e) {
                        switch(e.ErrorCode) {
                            case 10054: // 远程主机强迫关闭了一个现有的连接
                                return 0;
                            default:
                                throw e;
                        }
                    }
                });
                if(result > 0) {
                    OnTcpServerReceived(index, buffer.Take(result).ToArray());
                } else {
                    await KickOut(index, client, false);
                }
            }
            if(client.client.Connected) {
                await ReceiveAsync(index, client);
            } else {
                await CheckToRemoveObsoleteConnections();
                OnTcpServerDisonnected(index);
            }
        }

        /// <summary>
        /// (内部) async 清理废弃客户端的执行体
        /// </summary>
        private async Task CheckToRemoveObsoleteConnections() {
            mutex.Lock();
            var needKicked = new Queue<KeyValuePair<int, ClientInfo>>();
            foreach(var i in clients) {
                if(!i.Value.client.Connected) {
                    needKicked.Enqueue(new KeyValuePair<int, ClientInfo>(i.Key, i.Value));
                }
            }
            foreach(var i in needKicked) {
                await KickOut(i.Key, i.Value, false);
            }
            mutex.Unlock();
        }

        /// <summary> 单个客户端的信息体 </summary>
        private class ClientInfo {
            /// <summary> 客户端连接体 </summary>
            public TcpClient client;
            /// <summary> 客户端终止任务控制符, 在断开与该客户端的连接时设定以取消收发任务 </summary>
            public CancellationTokenSource cancellationToken;
            /// <summary> 客户端接收消息任务句柄 </summary>
            public Task receiveTask;
            /// <summary> 资源锁 </summary>
            public readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        }

        /// <summary> 服务器连接体 </summary>
        private TcpListener self;
        /// <summary> 服务器接收连接任务句柄 </summary>
        private Task acceptTask;
        /// <summary> 服务器资源锁 </summary>
        private readonly Thread.SimpleLock mutex = new Thread.SimpleLock();
        /// <summary> 客户端列表 </summary>
        private readonly Dictionary<int, ClientInfo> clients = new Dictionary<int, ClientInfo>();
    }
}
