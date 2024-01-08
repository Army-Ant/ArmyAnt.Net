using System.Net;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.SingleWriteStreams
{
    /// <summary>
    /// TCP �ͻ��˽ӿ�
    /// </summary>
    public interface ITcpClient : IIPSocket, ISingleWriteStream {
        /// <summary> �Ƿ������� </summary>
        bool Connected { get; }
        IPEndPoint ServerIPEndPoint { get; }
        /// <summary>
        /// �������ӵ�ָ��������
        /// </summary>
        /// <param name="server"> ������λ�� </param>
        void Connect(IPAddress addr, int port);
        /// <summary>
        /// async �첽���ӵ�ָ��������
        /// </summary>
        /// <param name="server"> ������λ�� </param>
        Task ConnectAsync(IPAddress addr, int port);
        /// <summary>
        /// ���ӶϿ�ʱ�Ļص�
        /// </summary>
        event System.Action OnDisconnected;
    }
}
