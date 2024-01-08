using System.Net;

namespace ArmyAnt.Stream.MultiWriteStreams
{
    /// <summary>
    /// HTTP收到请求时的回调
    /// </summary>
    /// <param name="request"> 请求体 </param>
    /// <param name="response"> 回复体, 需在此回调中拼接回复体并自行调用Close </param>
    /// <param name="user"> 请求者信息 </param>
    public delegate void OnHttpServerReceived(HttpListenerRequest request, HttpListenerResponse response, System.Security.Principal.IPrincipal user);

    /// <summary>
    /// 代表 HTTP 服务器（包含 WebSocket 服务器）的接口
    /// </summary>
    public interface IHttpServer : IMultiWriteStream {
        /// <summary> 子协议名 </summary>
        string SubProtocol { get; set; }
        /// <summary> 服务器的可用URI集合 </summary>
        public HttpListenerPrefixCollection Prefixes { get; }

        void Open(params string[] prefixes);

        System.Uri GetClientUrl(int index);

        /// <summary>
        /// 有新客户端接入时的回调
        /// </summary>
        event System.Action<int, System.Uri> OnWebSocketClientConnectedIn;

        /// <summary>
        /// 收到HTTP请求 (非Websocket) 时的回调, 需在此回调中拼接response回复体并自行调用Close
        /// </summary>
        event OnHttpServerReceived OnHttpServerReceived;
    }
}
