using System.Threading.Tasks;

namespace ArmyAnt.Stream.SingleWriteStreams {
    /// <summary>
    /// WebSocket �ͻ��˽ӿ�
    /// </summary>
    public interface IWebSocketClient : ISingleWriteStream {
        /// <summary> �Ƿ������� </summary>
        bool Connected { get; }
        System.Uri ServerUri { get; set; }
        /// <summary>
        /// �������ӵ�ָ��������
        /// </summary>
        /// <param name="server"> ������λ�� </param>
        Task Open(System.Uri serverUri);
        /// <summary>
        /// ���ӶϿ�ʱ�Ļص�
        /// </summary>
        event System.Action OnDisconnected;
    }
}
