using System.Threading.Tasks;

namespace ArmyAnt.Stream.SingleWriteStreams {
    /// <summary>
    /// WebSocket 客户端接口
    /// </summary>
    public interface IWebSocketClient : ISingleWriteStream {
        /// <summary> 是否已连接 </summary>
        bool Connected { get; }
        System.Uri ServerUri { get; set; }
        /// <summary>
        /// 阻塞连接到指定服务器
        /// </summary>
        /// <param name="server"> 服务器位置 </param>
        Task Open(System.Uri serverUri);
        /// <summary>
        /// 连接断开时的回调
        /// </summary>
        event System.Action OnDisconnected;
    }
}
