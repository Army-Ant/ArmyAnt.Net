namespace ArmyAnt.Stream.SingleWriteStreams
{
    /// <summary>
    /// ���� Unix �� Socket �Ľӿ�
    /// </summary>
    public interface IDomainSocket : ISocket, ISingleWriteStream {
        string Path { get; set; }
    }
}
