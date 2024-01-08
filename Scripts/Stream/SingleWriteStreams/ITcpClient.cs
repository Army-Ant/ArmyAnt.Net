using System.Net;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.SingleWriteStreams
{
    /// <summary>
    /// TCP 客户端接口
    /// </summary>
    public interface ITcpClient : IIPSocket, ISingleWriteStream {
        /// <summary> 是否已连接 </summary>
        bool Connected { get; }
        IPEndPoint ServerIPEndPoint { get; }
        /// <summary>
        /// 阻塞连接到指定服务器
        /// </summary>
        /// <param name="server"> 服务器位置 </param>
        void Connect(IPAddress addr, int port);
        /// <summary>
        /// async 异步连接到指定服务器
        /// </summary>
        /// <param name="server"> 服务器位置 </param>
        Task ConnectAsync(IPAddress addr, int port);
        /// <summary>
        /// 连接断开时的回调
        /// </summary>
        event System.Action OnDisconnected;
    }
}
