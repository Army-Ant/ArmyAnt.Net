namespace ArmyAnt.Stream.Exception
{
    public enum ExceptionType
    {
        /// <summary> δ�����쳣 </summary>
        Unknown,
        /// <summary> ��������δ�ر� </summary>
        ServerHasNotStopped,
        /// <summary> ��������δ���� </summary>
        ServerHasNotStarted,
        /// <summary> ����������������δ�ر� </summary>
        ServerHasConnected,
    }

    /// <summary>
    /// ���ڴ��� Stream �������Զ����쳣������쳣��
    /// </summary>
    public class NetworkException : System.Exception {
        public NetworkException(ExceptionType type) : base(GetMessageByType(type))
        {
            Type = type;
        }

        private static string GetMessageByType(ExceptionType type)
        {
            switch (type)
            {
                case ExceptionType.Unknown:
                    return "Unknown Exception";
                case ExceptionType.ServerHasNotStopped:
                    return "The server has started";
                default:
                    return "Unknown type";
            }
        }

        public ExceptionType Type;
    }
}
