using System.Net;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.SingleWriteStreams {

    /// <summary>
    /// UDP �ӿ�
    /// </summary>
    public interface IUdp : IIPSocket, ISingleWriteStream
    {
        IPEndPoint DefaultAddress { get; set; }
        /// <summary>
        /// �յ�����ʱ�ص�
        /// </summary>
        event System.Action<IPEndPoint, byte[]> OnReceived;
        /// <summary>
        /// ��ĳ�ͻ��˷�����Ϣ
        /// </summary>
        /// <param name="ep"> �ͻ�������λ�� </param>
        /// <param name="content"> ��Ϣ���� </param>
        int Write(IPEndPoint ep, byte[] content);
        /// <summary>
        /// async ��ĳ�ͻ��˷�����Ϣ
        /// </summary>
        /// <param name="ep"> �ͻ�������λ�� </param>
        /// <param name="content"> ��Ϣ���� </param>
        Task<int> WriteAsync(IPEndPoint ep, byte[] content);
        /// <summary>
        /// ����ಥ��
        /// </summary>
        /// <param name="addr"> Ҫ����Ķಥ���ַ </param>
        /// <param name="timeToLive"> ����ʱ�� </param>
        /// <returns> �Ƿ����ɹ� </returns>
        bool JoinMulticastGroup(IPAddress addr, int timeToLive);
        /// <summary>
        /// ����ಥ��
        /// </summary>
        /// <param name="addr"> Ҫ����Ķಥ���ַ </param>
        /// <returns> �Ƿ�ȫ������ɹ� </returns>
        bool JoinMulticastGroup(params IPAddress[] addr);
        /// <summary>
        /// �˳��ಥ��
        /// </summary>
        /// <param name="addr"> �ಥ���ַ </param>
        void LeaveMulticastGroup(params IPAddress[] addr);
    }
}
