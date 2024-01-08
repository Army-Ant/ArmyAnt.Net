using System.Net;

namespace ArmyAnt.Stream.MultiWriteStreams {
    /// <summary>
    /// TCP 服务器接口
    /// </summary>
    public interface ITcpServer : IMultiWriteStream, IIPSocket {
        /// <summary>
        /// 有新客户端接入时的回调
        /// </summary>
        event System.Action<int, IPEndPoint> OnTCPClientConnectedIn;

        IPEndPoint GetTcpClient(int index);
    }
}
