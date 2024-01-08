using System.Net;

namespace ArmyAnt.Stream.MultiWriteStreams {
    /// <summary>
    /// TCP �������ӿ�
    /// </summary>
    public interface ITcpServer : IMultiWriteStream, IIPSocket {
        /// <summary>
        /// ���¿ͻ��˽���ʱ�Ļص�
        /// </summary>
        event System.Action<int, IPEndPoint> OnTCPClientConnectedIn;

        IPEndPoint GetTcpClient(int index);
    }
}
