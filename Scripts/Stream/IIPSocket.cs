namespace ArmyAnt.Stream
{

    /// <summary>
    /// 所有 IP 协议网络处理类的公共接口
    /// </summary>
    public interface IIPSocket : ISocket
    {
        /// <summary>
        /// 本机的IP网络位置, 只有在开启监听后可用
        /// </summary>
        System.Net.IPEndPoint IPEndPoint { get; }
    }
}
