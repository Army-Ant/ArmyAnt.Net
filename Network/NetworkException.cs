using System;

namespace ArmyAnt.Network {
    public enum ExceptionType {
        /// <summary> 未定义异常 </summary>
        Unknown,
        /// <summary> 服务器尚未关闭, 不能重新开启 </summary>
        ServerHasNotStopped,
    }

    public class NetworkException : Exception {
        public NetworkException(ExceptionType type) : base(GetMessageByType(type)) {
            Type = type;
        }

        private static string GetMessageByType(ExceptionType type) {
            switch(type) {
                case ExceptionType.Unknown:
                    return "Unknown Exception";
                case ExceptionType.ServerHasNotStopped:
                    return "Cannot restart this server, this server is still running now";
                default:
                    return "Unknown type";
            }
        }

        public ExceptionType Type;
    }
}
