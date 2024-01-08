using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArmyAnt.Stream.SingleWriteStreams;

namespace ArmyAnt.Stream.Generic
{
#if NET || NETFRAMEWORK || NET_4_6
    /// <summary>
    /// ���ڴ�����
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
        /// async (�ڲ�) �������ݵ��߳�/��������
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

        /// <summary> �շ���Ϣ������ </summary>
        private Task receiveTask;
        /// <summary> �ͻ�����ֹ������Ʒ�, �ڶϿ���ÿͻ��˵�����ʱ�趨��ȡ���շ����� </summary>
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    }
#endif
}
