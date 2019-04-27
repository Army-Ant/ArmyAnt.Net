using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ArmyAnt.Network {
    /// <summary>
    /// TCP有新客户端接入时的回调
    /// </summary>
    /// <param name="index"> 客户端的序列号 </param>
    /// <param name="point"> 客户端的终端位置 </param>
    /// <returns></returns>
    public delegate bool OnTcpServerConnected(int index, IPEndPoint point);
    /// <summary>
    /// TCP有客户端关闭连接, 断开连接或被踢掉时的回调
    /// </summary>
    /// <param name="index"> 客户端的序列号 </param>
    public delegate void OnTcpServerDisonnected(int index);
    /// <summary>
    /// TCP收到来自客户端的数据时回调
    /// </summary>
    /// <param name="index"> 客户端的序列号 </param>
    /// <param name="data"> 消息正文 </param>
    public delegate void OnTcpServerReceived(int index, byte[] data);
    /// <summary>
    /// HTTP收到请求时的回调
    /// </summary>
    /// <param name="request"> 请求体 </param>
    /// <param name="response"> 回复体, 需在此回调中拼接回复体并自行调用Close </param>
    /// <param name="user"> 请求者信息 </param>
    public delegate void OnHttpServerReceived(HttpListenerRequest request, HttpListenerResponse response, System.Security.Principal.IPrincipal user);
    /// <summary>
    /// Socket 客户端收到数据时回调
    /// </summary>
    /// <param name="ep"> 远程计算机的网络地址 </param>
    /// <param name="data"> 消息正文 </param>
    public delegate void OnIPClientReceived(IPEndPoint ep, byte[] data);
    /// <summary>
    /// TCP客户端连接断开时的回调
    /// </summary>
    /// <param name="ep"> 服务器的网络地址 </param>
    public delegate void OnTcpClientDisonnected();
    /// <summary>
    /// WebSocket 客户端收到数据时回调
    /// </summary>
    /// <param name="data"> 消息正文 </param>
    public delegate void OnWebsocketClientReceived(byte[] data);

    public enum NetworkType {
        Unknown,
        Tcp,
        Http,
        Websocket,
        Udp,
    }

    public interface INetwork {
        /// <summary>
        /// 网络是否在运行
        /// </summary>
        bool IsStarting { get; }
        /// <summary>
        /// 启动网络, 指定端口号
        /// </summary>
        /// <param name="port"></param>
        void Start(ushort port);
        /// <summary>
        /// 关闭网络, 断开所有连接
        /// </summary>
        void Stop(bool nowait = false);
    }

    public interface INetworkServer : INetwork {
        /// <summary>
        /// 启动服务器, 所需参数均取默认值
        /// </summary>
        void Start();
    }

    public interface IPSocket {
        /// <summary>
        /// 本机的IP网络位置, 只有在开启监听后可用
        /// </summary>
        IPEndPoint IPEndPoint { get; }
    }

    public interface ISocketNetworkClient : IPSocket, INetwork {
        /// <summary>
        /// 收到数据时回调
        /// </summary>
        OnIPClientReceived OnClientReceived { get; set; }
    }

    public interface ITcpNetworkServer : INetworkServer {
        /// <summary>
        /// async 踢掉指定客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        Task KickOut(int index);
        /// <summary>
        /// async 向某客户端发送消息
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <param name="content"> 消息正文 </param>
        Task Send(int index, byte[] content);
        /// <summary>
        /// 查询是否存在指定序列号的客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <returns> 存在于连接中, 则返回true </returns>
        bool IsClientExist(int index);
        /// <summary>
        /// 有新客户端接入时的回调
        /// </summary>
        OnTcpServerConnected OnTcpServerConnected { get; set; }
        /// <summary>
        /// 有客户端关闭连接, 断开连接或被踢掉时的回调
        /// </summary>
        OnTcpServerDisonnected OnTcpServerDisonnected { get; set; }
        /// <summary>
        /// 收到来自客户端的数据时回调
        /// </summary>
        OnTcpServerReceived OnTcpServerReceived { get; set; }
    }

    public interface ITcpNetworkClient : INetwork {
        /// <summary> 是否已连接 </summary>
        bool Connected { get; }
        /// <summary>
        /// 阻塞连接到指定服务器
        /// </summary>
        /// <param name="server"> 服务器位置 </param>
        void Connect(IPEndPoint server);
        /// <summary>
        /// async 异步连接到指定服务器
        /// </summary>
        /// <param name="server"> 服务器位置 </param>
        Task ConnectAsync(IPEndPoint server);
        /// <summary>
        /// 向服务器发送消息
        /// </summary>
        /// <param name="content"> 消息正文 </param>
        void Send(byte[] content);
        /// <summary>
        /// 连接断开时的回调
        /// </summary>
        OnTcpClientDisonnected OnTcpClientDisonnected { get; set; }
    }

    public interface IUdpNetwork : ISocketNetworkClient, INetworkServer, IPSocket {
        /// <summary>
        /// async 向某客户端发送消息
        /// </summary>
        /// <param name="ep"> 客户端网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        Task<int> Send(IPEndPoint ep, byte[] content);
        /// <summary>
        /// 加入多播组
        /// </summary>
        /// <param name="addr"> 要加入的多播组地址 </param>
        /// <param name="timeToLive"> 生存时间 </param>
        /// <returns> 是否加入成功 </returns>
        bool JoinMulticastGroup(IPAddress addr, int timeToLive);
        /// <summary>
        /// 加入多播组
        /// </summary>
        /// <param name="addr"> 要加入的多播组地址 </param>
        /// <returns> 是否全部加入成功 </returns>
        bool JoinMulticastGroup(params IPAddress[] addr);
        /// <summary>
        /// 退出多播组
        /// </summary>
        /// <param name="addr"> 多播组地址 </param>
        void LeaveMulticastGroup(params IPAddress[] addr);
    }


}
