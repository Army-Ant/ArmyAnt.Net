namespace ArmyAnt.Stream {

    /// <summary>
    /// 所有即时读取流的公共接口，包括网络、管道、硬件数据口（如串口）、无线数据口（如蓝牙）
    /// </summary>
    public interface IStream {
        /// <summary>
        /// 是否在运行，只有在运行中才能发送、接收数据，才能监听和接收连接
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// 启动, 所需参数均取默认值
        /// </summary>
        void Open();
        /// <summary>
        /// 关闭流, 断开所有连接
        /// </summary>
        void Close(bool nowait = false);
        /// <summary>
        /// 是否在暂停中
        /// </summary>
        bool IsPausing { get; }
        /// <summary>
        /// 暂停监听线程，暂停消息的接收和连接接收
        /// </summary>
        void Pause();
        /// <summary>
        /// 恢复监听线程
        /// </summary>
        void Resume();
        /// <summary>
        /// 读取回调
        /// </summary>
        event System.Action<byte[]> OnReading;
    }
}
