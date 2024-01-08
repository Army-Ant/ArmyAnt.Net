using System.Net;
using System.Threading.Tasks;

namespace ArmyAnt.Stream.SingleWriteStreams {

    /// <summary>
    /// UDP 接口
    /// </summary>
    public interface IUdp : IIPSocket, ISingleWriteStream
    {
        IPEndPoint DefaultAddress { get; set; }
        /// <summary>
        /// 收到数据时回调
        /// </summary>
        event System.Action<IPEndPoint, byte[]> OnReceived;
        /// <summary>
        /// 向某客户端发送消息
        /// </summary>
        /// <param name="ep"> 客户端网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        int Write(IPEndPoint ep, byte[] content);
        /// <summary>
        /// async 向某客户端发送消息
        /// </summary>
        /// <param name="ep"> 客户端网络位置 </param>
        /// <param name="content"> 消息正文 </param>
        Task<int> WriteAsync(IPEndPoint ep, byte[] content);
        /// <summary>
        /// 加入多播组
        /// </summary>
        /// <param name="addr"> 要加入的多播组地址 </param>
        /// <param name="timeToLive"> 生存时间 </param>
        /// <returns> 是否加入成功 </returns>
        bool JoinMulticastGroup(IPAddress addr, int timeToLive);
        /// <summary>
        /// 加入多播组
        /// </summary>
        /// <param name="addr"> 要加入的多播组地址 </param>
        /// <returns> 是否全部加入成功 </returns>
        bool JoinMulticastGroup(params IPAddress[] addr);
        /// <summary>
        /// 退出多播组
        /// </summary>
        /// <param name="addr"> 多播组地址 </param>
        void LeaveMulticastGroup(params IPAddress[] addr);
    }
}
