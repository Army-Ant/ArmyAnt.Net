using System.Threading.Tasks;

namespace ArmyAnt.Stream {
    public interface IMultiWriteStream : IStream {
        /// <summary>
        /// async 踢掉指定客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        Task KickOut(int index);
        /// <summary>
        /// async 向某客户端发送消息
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <param name="content"> 消息正文 </param>
        Task Write(int index, byte[] content);
        /// <summary>
        /// 查询是否存在指定序列号的客户端
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <returns> 存在于连接中, 则返回true </returns>
        bool IsClientExist(int index);
        /// <summary>
        /// 获取指定序列的客户端的信息
        /// </summary>
        /// <param name="index"> 客户端序列号, 在连接时获得 </param>
        /// <returns></returns>
        object GetClient(int index);
        /// <summary>
        /// 有新客户端接入时的回调
        /// </summary>
        event System.Action<int, object> OnClientConnectedIn;
        /// <summary>
        /// 有客户端关闭连接, 断开连接或被踢掉时的回调
        /// </summary>
        event System.Action<int> OnClientDisconnected;
        /// <summary>
        /// 收到来自客户端的数据时回调
        /// </summary>
        event System.Action<int, byte[]> OnReadingClient;
    }
}
