using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArmyAnt.IO {
    /// <summary>
    /// 日志管理器， 日志流的管理，日志的输入和输出；
    /// 日志流可以是文件，控制台，或者其他任意可写的 Stream
    /// </summary>
    public class Logger : WriteOnlyStream {
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
        public static LogLevel LevelFromString(string str) {
            foreach(var i in Enum.GetValues(typeof(LogLevel))) {
                if(i.ToString().ToLower() == str.ToLower()) {
                    return (LogLevel)i;
                }
            }
            if(str == null)
            {
                throw new ArgumentNullException("str");
            }
            else
            {
                throw new ArgumentTextNotAllowException("str", str);
            }
        }

        /// <summary> 
        /// 能否写, 恒返回true 
        /// </summary>
        public sealed override bool CanWrite => true;

        /// <summary>
        /// 默认的日志信息字符串编码，一般会跟随系统 （<seealso cref="Encoding.UTF8"/>）
        /// </summary>
        public Encoding DefaultWritingEncoding { get; set; }

        /// <summary>
        /// 日志管理器构造函数, 参数可选指定控制台设置
        /// </summary>
        /// <param name="withConsole"> 是否带有控制台. 如不带控制台, 则日志不会输出到控制台, 后续参数无效 </param>
        /// <param name="consoleLevel"> 控制台最低日志等级, 默认为 Import </param>
        /// <param name="consoleTimestamp"> 控制台是否带有时间标识 </param>
        public Logger(bool withConsole = true, LogLevel consoleLevel = LogLevel.Import, bool consoleTimestamp = false) {
            DefaultWritingEncoding = Encoding.UTF8;
            if(withConsole)
            {
                Console.InputEncoding = DefaultWritingEncoding;
                Console.OutputEncoding = DefaultWritingEncoding;
                var console = Console.OpenStandardOutput();
                if(console != null && console.CanWrite) {
                    AddStream(console, consoleLevel, consoleTimestamp);
                }
            }
        }

        /// <summary>
        /// 写入自定义不加前缀的二进制数据, 日志等级默认为 verbose
        /// </summary>
        /// <param name="buffer"> 要写入的数据 </param>
        /// <param name="offset"> 可选参数, 要从 buffer 的哪个起始位置开始写入 </param>
        /// <param name="count"> 可选参数,写入多长的数据 </param>
        /// <exception cref="ArgumentNullException"> 当 buffer 传入 null 时引发 </exception>
        /// <exception cref="ArgumentException"> 当 count + offset 超过了 buffer 的总长度时引发 </exception>
        public override void Write(byte[] buffer, int offset = 0, int count = 0) {
            Write(LogLevel.Verbose, buffer, offset, count);
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
        public void Write(LogLevel level, byte[] buffer, int offset = 0, int count = 0)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (count == 0) {
                count = buffer.Length - offset;
            }
            if(count == 0 || count + offset > buffer.Length)
            {
                throw new ArgumentException("The argument \"count + offset\" is larger than length of the argument \"buffer\"", "count");
            }
            var timeBytes = Encoding.Default.GetBytes("[ " + DateTime.Now.ToString() + " ] ");
            var levelBytes = Encoding.Default.GetBytes("[ " + level.ToString() + " ] ");
            lock(streams) {
                foreach(var i in streams) {
                    if(i.stream == null || !i.stream.CanWrite) {
                        streams.Remove(i);
                    }else if(i.level <= level) {
                        if(i.stream.CanSeek) {
                            i.stream.Seek(0, SeekOrigin.End);
                        }
                        if(i.timeStamp) {
                            i.stream.Write(timeBytes, 0, timeBytes.Length);
                        }
                        i.stream.Write(levelBytes, 0, levelBytes.Length);
                        i.stream.Write(buffer, offset, count);
                        i.stream.Flush();
                    }
                }
            }
        }

        /// <summary>
        /// 写入一个的字符串, 可指定编码
        /// </summary>
        /// <param name="content"> 要写入的内容 </param>
        /// <param name="encodeType"> 指定要写入的字符串编码, 默认等于 <seealso cref="DefaultWritingEncoding"/> </param>
        /// <exception cref="ArgumentNullException"> 当 content 传入 null 或空值时引发 </exception>
        public void Write(LogLevel level, string content, Encoding encodeType = null)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("content");
            }
            if(encodeType == null)
            {
                encodeType = DefaultWritingEncoding;
            }
            var bts = encodeType.GetBytes(content);
            Write(level, bts);
        }

        /// <summary>
        /// 写入一组对象拼接成的字符串
        /// </summary>
        /// <param name="strings"> 要写入的对象, 所有这些对象会被转化为字符串写入 </param>
        /// <exception cref="ArgumentNullException"> 当 content 传入 null 或空值时引发 </exception>
        public void Write(LogLevel level, string content, params object[] strings)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException("content");
            }
            var str = new StringBuilder(content);
            foreach(var i in strings) {
                str.Append(i);
            }
            Write(level, str.ToString());
        }

        /// <summary>
        /// 写入一个字符串并自动换行
        /// </summary>
        /// <param name="content"></param>
        /// <exception cref="ArgumentNullException"> 当 content 传入 null 或空值时引发 </exception>
        public void WriteLine(LogLevel level, string content, params object[] strings) {
            WriteLine(DefaultWritingEncoding, level, content, strings);
        }

        /// <summary>
        /// 写入一组对象拼接成的字符串并自动换行
        /// </summary>
        /// <param name="content"> 要写入的第一个对象, 必须至少有一个 </param>
        /// <param name="strings"> 要写入的后续对象, 所有这些对象会被转化为字符串写入 </param>
        /// <exception cref="ArgumentNullException"> 当 content 传入 null 或空值时引发 </exception>
        public void WriteLine(Encoding encodeType, LogLevel level, string content, params object[] strings) {
            if(strings == null || strings.Length == 0) {
                return;
            }
            var str = new StringBuilder(content);
            foreach(var i in strings) {
                if(i is Array arr) {
                    foreach(var arr_i in arr) {
                        str.Append(arr_i.ToString());
                    }
                } else {
                    str.Append(i.ToString());
                }
            }
            str.Append(Environment.NewLine);
            Write(level, str.ToString(), encodeType);
        }

        /// <summary>
        /// 添加一个输出流, 写操作会写入到这些流
        /// </summary>
        /// <param name="stream"> 要添加的输出流, 必须是可写的 </param>
        /// <exception cref="ArgumentNullException"> 当 stream 传入 null 或空值时引发 </exception>
        /// <exception cref="ArgumentException"> 当 stream 不可写入时引发 </exception>
        public void AddStream(Stream stream, LogLevel level, bool timeStamp) {
            if(stream == null) {
                throw new ArgumentNullException("stream");
            }
            if(!stream.CanWrite) {
                throw new ArgumentException("The adding stream cannot write", "stream");
            }
            lock(streams) {
                streams.Add(new StreamInfo() { stream = stream, level = level, timeStamp = timeStamp});
            }
        }

        /// <summary>
        /// 移除一个输出流
        /// </summary>
        /// <param name="stream"> 要移除的输出流 </param>
        /// <returns> 移除成功返回true, 未找到返回false, 参见<seealso cref="ICollection{T}.Remove(T)"/> </returns>
        /// <exception cref="ArgumentNullException"> 当 stream 传入 null 或空值时引发 </exception>
        public bool RemoveStream(Stream stream) {
            if(stream == null) {
                throw new ArgumentNullException();
            }
            lock(streams) {
                StreamInfo ret = default;
                foreach(var i in streams) {
                    if(i.stream == stream) {
                        ret = i;
                    }
                }
                if(ret.stream == stream)
                    return streams.Remove(ret);
            }
            return false;
        }

        /// <summary>
        /// static 辅助函数, 获得一个可用于加入到 Logger 的文件的 Stream, 以共享方式打开, 如不存在自动创建(但不能创建上级目录); <br/>
        /// 该流需要手动关闭. <br/>
        /// 异常参见 <seealso cref="File.Open(string, FileMode, FileAccess, FileShare)"/>
        /// </summary>
        /// <param name="filepath"> 要打开的文件完整路径, 可每层目录作为一个字符串传入 </param>
        /// <returns> 返回成功打开的文件流 </returns>
        /// <exception cref="OverflowException"> From <see cref="Array.Length"/> </exception>
        /// <exception cref="ArgumentException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="ArgumentNullException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="PathTooLongException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="DirectoryNotFoundException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="IOException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="UnauthorizedAccessException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="ArgumentOutOfRangeException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="FileNotFoundException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        /// <exception cref="NotSupportedException"> From <see cref="File.Open(string, FileMode, FileAccess, FileShare)"/> </exception>
        public static FileStream CreateLoggerFileStream(params string[] filepath) {
            var str = new StringBuilder();
            for(var i = 0; i < filepath.Length; ++i) {
                str.Append(filepath[i]);
                if(i != filepath.Length - 1) {
                    str.Append(Path.DirectorySeparatorChar);
                }
            }
            return File.Open(str.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        private struct StreamInfo {
            public Stream stream;
            public LogLevel level;
            public bool timeStamp;
        }

        private IList<StreamInfo> streams = new List<StreamInfo>();
    }
}
