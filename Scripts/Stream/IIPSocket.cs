namespace ArmyAnt.Stream
{

    /// <summary>
    /// ���� IP Э�����紦����Ĺ����ӿ�
    /// </summary>
    public interface IIPSocket : ISocket
    {
        /// <summary>
        /// ������IP����λ��, ֻ���ڿ������������
        /// </summary>
        System.Net.IPEndPoint IPEndPoint { get; }
    }
}
