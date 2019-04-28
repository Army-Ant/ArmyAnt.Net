using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArmyAnt.IO {
    public class Logger : WriteOnlyStream {
        public enum LogLevel {
            Verbose,
            Debug,
            Info,
            Import,
            Warning,
            Error,
            Fatal,
        }
        public static LogLevel LevelFromString(string str) {
            foreach(var i in Enum.GetValues(typeof(LogLevel))) {
                if(i.ToString().ToLower() == str) {
                    return (LogLevel)i;
                }
            }
            return default;
        }

        /// <summary> 能否写, 恒返回true </summary>
        public override bool CanWrite => true;
        public Encoding DefaultWritingEncoding { get; set; }

        public Logger(bool withConsole = true, LogLevel consoleLevel = LogLevel.Info, bool consoleTimestamp = false) {
            DefaultWritingEncoding = Encoding.UTF8;
            if(withConsole) {
                var console = Console.OpenStandardOutput();
                if(console != null && console.CanWrite) {
                    AddStream(console, consoleLevel, consoleTimestamp);
                }
            }
        }

        /// <summary>
        /// 写入自定义不加前缀的二进制数据
        /// </summary>
        /// <param name="buffer"> 要写入的数据 </param>
        /// <param name="offset"> 可选参数, 要从 buffer 的哪个起始位置开始写入 </param>
        /// <param name="count"> 可选参数,写入多长的数据 </param>
        public override void Write(byte[] buffer, int offset = 0, int count = 0) {
            Write(LogLevel.Verbose, buffer, offset, count);
        }
        public void Write(LogLevel level, byte[] buffer, int offset = 0, int count = 0) {
            if(count == 0) {
                count = buffer.Length;
            }
            var timeBytes = Encoding.Default.GetBytes("[ " + DateTime.Now.ToString() + " ] ");
            var levelBytes = Encoding.Default.GetBytes("[ " + level.ToString() + " ] ");
            lock(streams) {
                foreach(var i in streams) {
                    if(i.stream != null && i.stream.CanWrite && i.level <= level) {
                        if(i.stream.CanSeek) {
                            i.stream.Seek(0, SeekOrigin.End);
                        }
                        if(i.timeStamp) {
                            i.stream.Write(timeBytes, 0, timeBytes.Length);
                        }
                        i.stream.Write(levelBytes, 0, levelBytes.Length);
                        i.stream.Write(buffer, offset, count);
                        i.stream.Flush();
                    } else {
                        streams.Remove(i);
                    }
                }
            }
        }

        /// <summary>
        /// 写入一个指定编码的字符串
        /// </summary>
        /// <param name="content"> 要写入的内容 </param>
        /// <param name="encodeType"> 要写入的字符串编码 </param>
        public void Write(LogLevel level, string content, Encoding encodeType) {
            var bts = encodeType.GetBytes(content);
            Write(level, bts);
        }

        /// <summary>
        /// 写入一个 默认编码的 字符串
        /// </summary>
        /// <param name="content"> 要写入的内容 </param>
        public void Write(LogLevel level, string content) {
            Write(level, content, DefaultWritingEncoding);
        }

        /// <summary>
        /// 写入一组对象拼接成的字符串
        /// </summary>
        /// <param name="strings"> 要写入的对象, 所有这些对象会被转化为字符串写入 </param>
        public void Write(LogLevel level, string content, params object[] strings) {
            if(strings == null || strings.Length == 0) {
                return;
            }
            var str = new StringBuilder(content);
            foreach(var i in strings) {
                str.Append(strings);
            }
            Write(level, str.ToString());
        }

        /// <summary>
        /// 写入一个字符串并自动换行
        /// </summary>
        /// <param name="content"></param>
        public void WriteLine(LogLevel level, string content) {
            Write(level, content + Environment.NewLine);
        }

        /// <summary>
        /// 写入一组对象拼接成的字符串并自动换行
        /// </summary>
        /// <param name="content"> 要写入的第一个对象, 必须至少有一个 </param>
        /// <param name="strings"> 要写入的后续对象, 所有这些对象会被转化为字符串写入 </param>
        public void WriteLine(LogLevel level, string content, params object[] strings) {
            Write(level, content, strings, Environment.NewLine);
        }

        /// <summary>
        /// 添加一个输出流, 写操作会写入到这些流
        /// </summary>
        /// <param name="stream"> 要添加的输出流, 必须是可写的 </param>
        public void AddStream(Stream stream, LogLevel level, bool timeStamp) {
            if(stream == null) {
                throw new ArgumentNullException();
            }
            if(!stream.CanWrite) {
                throw new ArgumentException("The adding stream cannot write");
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
        /// static 辅助函数, 获得一个可用于加入到 Logger 的文件的 Stream, 以共享方式打开, 如不存在自动创建(但不能创建上级目录),
        /// 该流需要手动关闭
        /// </summary>
        /// <param name="filepath"> 要打开的文件完整路径, 可每层目录作为一个字符串传入 </param>
        /// <returns> 返回成功打开的文件流 </returns>
        public static FileStream CreateLoggerFileStream(params string[] filepath) {
            var str = new StringBuilder();
            for(var i = 0; i < filepath.Length; ++i) {
                str.Append(i);
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
