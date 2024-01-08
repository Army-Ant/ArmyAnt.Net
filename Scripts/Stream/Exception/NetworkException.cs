namespace ArmyAnt.Stream.Exception
{
    public enum ExceptionType
    {
        /// <summary> 未定义异常 </summary>
        Unknown,
        /// <summary> 服务器尚未关闭 </summary>
        ServerHasNotStopped,
        /// <summary> 服务器尚未开启 </summary>
        ServerHasNotStarted,
        /// <summary> 到服务器的连接尚未关闭 </summary>
        ServerHasConnected,
    }

    /// <summary>
    /// 用于处理 Stream 中所有自定义异常情况的异常类
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
