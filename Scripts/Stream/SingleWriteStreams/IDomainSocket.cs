namespace ArmyAnt.Stream.SingleWriteStreams
{
    /// <summary>
    /// 代表 Unix 域 Socket 的接口
    /// </summary>
    public interface IDomainSocket : ISocket, ISingleWriteStream {
        string Path { get; set; }
    }
}
