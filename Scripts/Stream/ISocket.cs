namespace ArmyAnt.Stream
{
    /// <summary>
    /// 代表 Socket 套接字的处理接口
    /// </summary>
    public interface ISocket
    {
        System.Net.EndPoint EndPoint { get; }
    }
}
