using ArmyAnt.IO;

using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.Logger {
    /// <summary>
    /// ��־�������� ��־���Ĺ�����־������������
    /// ��־���������ļ�������̨���������������д�� Stream
    /// </summary>
    public class Logger
    {
        public enum LogLevel
        {
            /// <summary> ��ͼ���־�����ڼ�¼ÿһ��ϸ�ڣ�����־������ͨ��������ʾ���¼ </summary>
            Verbose,
            /// <summary> ���Լ���־�����ڼ�¼��Ҫϸ�ڣ�ͨ��������ʾ���¼��ֻ����ר�ſ�������ģʽ�����ʾ����¼ </summary>
            Debug,
            /// <summary> ��Ϣ����־�����ڼ�¼��Ҫ״̬������Ϣ��ͨ�����¼������ʾ </summary>
            Info,
            /// <summary> ��Ҫ����־�����ڼ�¼��־����Ҫ״̬������Ϣ��ͨ�����¼������ʾ </summary>
            Import,
            /// <summary> ��ʾ����־�����ڷ��ֿ��ܲ�����������ȫ�ɹ��Ĳ���������������״̬ʱ��¼��ͨ�����¼���һ�ɫ�ص���ʾ </summary>
            Warning,
            /// <summary> ������־�����ڷ��ִ������������״̬���߲���ʧ��ʱ��¼��ͨ�����¼���Һ�ɫ�ص���ʾ </summary>
            Error,
            /// <summary> ���ش�����־�����ڷ��ֵ��������жϵĴ�������͹���״̬ʱ��¼��ͨ�����¼�������ɫ�ص���ʾ </summary>
            Fatal,
        }

        /// <summary>
        /// �� <see cref="LogLevel"/> ö�ٳ�Ա�Ĵ����ַ�����ת�������Ա
        /// </summary>
        /// <param name="str"> <see cref="LogLevel"/> ö�ٳ�Ա��Ӧ���ַ�������Сд������ </param>
        /// <returns> �� <paramref name="str"/> ����Ӧ�� <see cref="LogLevel"/> ö��ֵ </returns>
        /// <exception cref="ArgumentNullException"> �� <paramref name="str"/> ���� null ʱ���� </exception>
        /// <exception cref="ArgumentTextNotAllowException"> �� <paramref name="str"/> ���벻�Ϸ��ַ���ʱ���� </exception>
        public static LogLevel LevelFromString(string str)
        {
            foreach (var i in Enum.GetValues(typeof(LogLevel)))
            {
                if (i.ToString().ToLower() == str.ToLower())
                {
                    return (LogLevel)i;
                }
            }
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            else
            {
                throw new ArgumentTextNotAllowException("str", str);
            }
        }

        public event Action<byte[]> OnReading;
        public event Action<DateTime, LogLevel, byte[]> OnReadingWithParam;

        /// <summary>
        /// Ĭ�ϵ���־��Ϣ�ַ������룬һ������ϵͳ ��<seealso cref="Encoding.UTF8"/>��
        /// </summary>
        public Encoding Encoding
        {
            get => encoding;
            set
            {
                encoding = value;
                if (withConsole)
                {
                    Console.InputEncoding = encoding;
                    Console.OutputEncoding = encoding;
                }
            }
        }
        private Encoding encoding = Encoding.Default;

        /// <summary> �Ƿ���п���̨. �粻������̨, ����־�������������̨, ����������Ч </summary>
        public bool WithConsole
        {
            get => withConsole;
            set
            {
                withConsole = value;
                if (withConsole)
                {
                    Console.InputEncoding = encoding;
                    Console.OutputEncoding = encoding;
                }
            }
        }
        private bool withConsole = true;

        /// <summary> ����̨�����־�ȼ�, Ĭ��Ϊ Import </summary>
        public LogLevel ConsoleLevel { get; set; } = LogLevel.Import;

        /// <summary> ����̨�Ƿ����ʱ���ʶ </summary>
        public bool IsConsoleShowTimestamp { get; set; } = true;

        /// <summary>
        /// д��һ���ַ������Զ�����
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException"> �� content ���� null ���ֵʱ���� </exception>
        public void WriteLine(LogLevel level, string content)
        {
            var str = new StringBuilder(content);
            str.Append(Environment.NewLine);
            Write(level, str.ToString());
        }

        /// <summary>
        /// д��һ�����ַ���, ��ָ������
        /// </summary>
        /// <param name="content"> Ҫд������� </param>
        /// <exception cref="ArgumentNullException"> �� content ���� null ���ֵʱ���� </exception>
        public void Write(LogLevel level, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }
            var bts = Encoding.GetBytes(content);
            Write(level, bts, 0, 0);
        }

        /// <summary>
        /// д���Զ��岻��ǰ׺�Ķ���������
        /// �÷���һ�㲻����д��־��ֻΪ��ʵ�ֽӿ�
        /// </summary>
        /// <param name="buffer"> Ҫд������� </param>
        /// <exception cref="NotImplementedException"></exception>
        public int Write(byte[] buffer)
        {
            return Write(LogLevel.Verbose, buffer, 0, 0);
        }

        /// <summary>
        /// д���Զ��岻��ǰ׺�Ķ���������
        /// </summary>
        /// <param name="level"> ��־�ȼ� </param>
        /// <param name="buffer"> Ҫд������� </param>
        /// <param name="offset"> ��ѡ����, Ҫ�� buffer ���ĸ���ʼλ�ÿ�ʼд�� </param>
        /// <param name="count"> ��ѡ����,д��೤������ </param>
        /// <exception cref="ArgumentNullException"> �� buffer ���� null ʱ���� </exception>
        /// <exception cref="ArgumentException"> �� count + offset ������ buffer ���ܳ���ʱ���� </exception>
        protected int Write(LogLevel level, byte[] buffer, int offset, int count = 0)
        {
            var ret = 0;
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (count == 0)
            {
                count = buffer.Length - offset;
            }
            if (count == 0 || count + offset > buffer.Length)
            {
                throw new ArgumentException("The argument \"count + offset\" is larger than length of the argument \"buffer\"", nameof(count));
            }
            var time = DateTime.Now;
            var timeBytes = Encoding.Default.GetBytes("[ " + DateTime.Now.ToString() + " ] ");
            var levelBytes = Encoding.Default.GetBytes("[ " + level.ToString() + " ] ");
            var data = new List<byte>(count + timeBytes.Length + levelBytes.Length);
            OnReadingWithParam(time, level, data.ToArray());
            data.InsertRange(0, levelBytes);
            if (ConsoleLevel <= level)
            {
                if (IsConsoleShowTimestamp)
                {
                    data.InsertRange(0, timeBytes);
                    Console.Write(data.ToArray());
                }
                else
                {
                    Console.Write(data.ToArray());
                    data.InsertRange(0, timeBytes);
                }
            }
            else
            {
                data.InsertRange(0, timeBytes);
            }
            OnReading(data.ToArray());
            return ret;
        }
    }
}
