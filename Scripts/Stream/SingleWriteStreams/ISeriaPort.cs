namespace ArmyAnt.Stream.SingleWriteStreams
{
    /// <summary>
    /// ���ڽ�������ʱ�Ļص�
    /// </summary>
    /// <param name="portName"> ������ </param>
    /// <param name="baudate"> ������ </param>
    /// <param name="data"> ��Ϣ���� </param>
    public delegate void OnSerialPortReceived(string portName, int baudRate, byte[] data);
    /// <summary>
    /// ���ڶϿ�ʱ�Ļص�
    /// </summary>
    public delegate void OnSerialPortDisonnected();

#if NET || NETFRAMEWORK || NET_4_6
    /// <summary>
    /// ���ڴ�����ӿ�
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
