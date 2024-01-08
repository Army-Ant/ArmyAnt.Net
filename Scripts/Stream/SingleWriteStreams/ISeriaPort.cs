namespace ArmyAnt.Stream.SingleWriteStreams
{
    /// <summary>
    /// 串口接收数据时的回调
    /// </summary>
    /// <param name="portName"> 串口名 </param>
    /// <param name="baudate"> 比特率 </param>
    /// <param name="data"> 消息正文 </param>
    public delegate void OnSerialPortReceived(string portName, int baudRate, byte[] data);
    /// <summary>
    /// 串口断开时的回调
    /// </summary>
    public delegate void OnSerialPortDisonnected();

#if NET || NETFRAMEWORK || NET_4_6
    /// <summary>
    /// 串口处理类接口
    /// </summary>
    public interface ISerialPort : ISingleWriteStream
    {
        event OnSerialPortReceived OnReceived;
        event System.Action OnDisconnected;
        string[] PortNames { get; }
        string PortName { get; set; }
        int BaudRate { get; set; }
        System.IO.Ports.Parity Parity { get; set; }
        int DataBits { get; set; }
        System.IO.Ports.StopBits StopBits { get; set; }
    }
#endif
}
