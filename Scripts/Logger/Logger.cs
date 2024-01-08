using ArmyAnt.IO;

using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.Logger {
    /// <summary>
    /// 日志管理器， 日志流的管理，日志的输入和输出；
    /// 日志流可以是文件，控制台，或者其他任意可写的 Stream
    /// </summary>
    public class Logger
    {
        public enum LogLevel
        {
            /// <summary> 最低级日志，用于记录每一步细节，因日志量极大，通常不会显示或记录 </summary>
            Verbose,
            /// <summary> 调试级日志，用于记录主要细节，通常不会显示或记录，只有在专门开启调试模式后会显示并记录 </summary>
            Debug,
            /// <summary> 信息级日志，用于记录主要状态步骤信息，通常会记录而不显示 </summary>
            Info,
            /// <summary> 重要级日志，用于记录标志性重要状态步骤信息，通常会记录并且显示 </summary>
            Import,
            /// <summary> 警示级日志，用于发现可能不正常、不完全成功的操作或者有隐患的状态时记录，通常会记录并且黄色重点显示 </summary>
            Warning,
            /// <summary> 错误级日志，用于发现错误操作、故障状态或者步骤失败时记录，通常会记录并且红色重点显示 </summary>
            Error,
            /// <summary> 严重错误级日志，用于发现导致运行中断的错误操作和故障状态时记录，通常会记录并且深红色重点显示 </summary>
            Fatal,
        }

        /// <summary>
        /// 将 <see cref="LogLevel"/> 枚举成员的代表字符串，转化成其成员
        /// </summary>
        /// <param name="str"> <see cref="LogLevel"/> 枚举成员对应的字符串，大小写不敏感 </param>
        /// <returns> 与 <paramref name="str"/> 所对应的 <see cref="LogLevel"/> 枚举值 </returns>
        /// <exception cref="ArgumentNullException"> 当 <paramref name="str"/> 传入 null 时引发 </exception>
        /// <exception cref="ArgumentTextNotAllowException"> 当 <paramref name="str"/> 传入不合法字符串时引发 </exception>
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
        /// 默认的日志信息字符串编码，一般会跟随系统 （<seealso cref="Encoding.UTF8"/>）
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

        /// <summary> 是否带有控制台. 如不带控制台, 则日志不会输出到控制台, 后续参数无效 </summary>
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

        /// <summary> 控制台最低日志等级, 默认为 Import </summary>
        public LogLevel ConsoleLevel { get; set; } = LogLevel.Import;

        /// <summary> 控制台是否带有时间标识 </summary>
        public bool IsConsoleShowTimestamp { get; set; } = true;

        /// <summary>
        /// 写入一个字符串并自动换行
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException"> 当 content 传入 null 或空值时引发 </exception>
        public void WriteLine(LogLevel level, string content)
        {
            var str = new StringBuilder(content);
            str.Append(Environment.NewLine);
            Write(level, str.ToString());
        }

        /// <summary>
        /// 写入一个的字符串, 可指定编码
        /// </summary>
        /// <param name="content"> 要写入的内容 </param>
        /// <exception cref="ArgumentNullException"> 当 content 传入 null 或空值时引发 </exception>
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
        /// 写入自定义不加前缀的二进制数据
        /// 该方法一般不用于写日志，只为了实现接口
        /// </summary>
        /// <param name="buffer"> 要写入的数据 </param>
        /// <exception cref="NotImplementedException"></exception>
        public int Write(byte[] buffer)
        {
            return Write(LogLevel.Verbose, buffer, 0, 0);
        }

        /// <summary>
        /// 写入自定义不加前缀的二进制数据
        /// </summary>
        /// <param name="level"> 日志等级 </param>
        /// <param name="buffer"> 要写入的数据 </param>
        /// <param name="offset"> 可选参数, 要从 buffer 的哪个起始位置开始写入 </param>
        /// <param name="count"> 可选参数,写入多长的数据 </param>
        /// <exception cref="ArgumentNullException"> 当 buffer 传入 null 时引发 </exception>
        /// <exception cref="ArgumentException"> 当 count + offset 超过了 buffer 的总长度时引发 </exception>
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
