using System.Net;

namespace ArmyAnt.Stream.MultiWriteStreams
{
    /// <summary>
    /// HTTP�յ�����ʱ�Ļص�
    /// </summary>
    /// <param name="request"> ������ </param>
    /// <param name="response"> �ظ���, ���ڴ˻ص���ƴ�ӻظ��岢���е���Close </param>
    /// <param name="user"> ��������Ϣ </param>
    public delegate void OnHttpServerReceived(HttpListenerRequest request, HttpListenerResponse response, System.Security.Principal.IPrincipal user);

    /// <summary>
    /// ���� HTTP ������������ WebSocket ���������Ľӿ�
    /// </summary>
    public interface IHttpServer : IMultiWriteStream {
        /// <summary> ��Э���� </summary>
        string SubProtocol { get; set; }
        /// <summary> �������Ŀ���URI���� </summary>
        public HttpListenerPrefixCollection Prefixes { get; }

        void Open(params string[] prefixes);

        System.Uri GetClientUrl(int index);

        /// <summary>
        /// ���¿ͻ��˽���ʱ�Ļص�
        /// </summary>
        event System.Action<int, System.Uri> OnWebSocketClientConnectedIn;

        /// <summary>
        /// �յ�HTTP���� (��Websocket) ʱ�Ļص�, ���ڴ˻ص���ƴ��response�ظ��岢���е���Close
        /// </summary>
        event OnHttpServerReceived OnHttpServerReceived;
    }
}
