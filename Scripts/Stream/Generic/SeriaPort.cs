using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArmyAnt.Stream.SingleWriteStreams;

namespace ArmyAnt.Stream.Generic
{
#if NET || NETFRAMEWORK || NET_4_6
    /// <summary>
    /// 串口处理类
    /// </summary>
    public class SerialPort : System.IO.Ports.SerialPort, ISerialPort {
        public SerialPort() : base() {
            receiveTask = Task.Run(ReceiveAsync);
        }
        public event OnSerialPortReceived OnReceived;
        public event Action<byte[]> OnReading;
        public event Action OnDisconnected;

        public string[] PortNames {
            get {
                return GetPortNames();
            }
        }

        public bool IsRunning => IsOpen;
        public bool IsPausing { get; private set; }

        public int Write(byte[] content) {
            Write(content, 0, content.Length);
            return content.Length;
        }

        public async Task<int> WriteAsync(byte[] content) {
            return await Task.Run(() => Write(content));
        }

        public void Open(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits) {
            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
            base.Open();
        }

        public new void Open() {
            base.Open();
        }

        public void Close(bool nowait = false) {
            base.Close();
        }

        public void Pause() {
            IsPausing = true;
        }

        public void Resume() {
            IsPausing = false;
        }

        /// <summary>
        /// async (内部) 接收数据的线程/任务函数体
        /// </summary>
        private async Task ReceiveAsync() {
            while (!cancellationTokenSource.Token.IsCancellationRequested) {
                if (IsRunning && !IsPausing) {
                    var data = Encoding.Default.GetBytes(ReadExisting());
                    if (data.Length > 0) {
                        OnReceived?.Invoke(PortName, BaudRate, data);
                        OnReading?.Invoke(data);
                    }
                } else {
                    await Task.Yield();
                }
            }
            OnDisconnected?.Invoke();
        }

        /// <summary> 收发消息任务句柄 </summary>
        private Task receiveTask;
        /// <summary> 客户端终止任务控制符, 在断开与该客户端的连接时设定以取消收发任务 </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    }
#endif
}
