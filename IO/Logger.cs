using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArmyAnt.IO {
    public class Logger : WriteOnlyStream {
        /// <summary> 能否写, 恒返回true </summary>
        public override bool CanWrite => true;
        public Encoding DefaultWritingEncoding { get; set; }

        public Logger(bool withConsole = true) {
            DefaultWritingEncoding = Encoding.UTF8;
            if(withConsole) {
                var console = Console.OpenStandardOutput();
                if(console != null && console.CanWrite) {
                    AddStream(console);
                }
            }
        }

        /// <summary>
        /// 写入二进制数据
        /// </summary>
        /// <param name="buffer"> 要写入的数据 </param>
        /// <param name="offset"> 可选参数, 要从 buffer 的哪个起始位置开始写入 </param>
        /// <param name="count"> 可选参数,写入多长的数据 </param>
        public override void Write(byte[] buffer, int offset = 0, int count = 0) {
            if(count == 0) {
                count = buffer.Length;
            }
            lock(streams) {
                foreach(var i in streams) {
                    if(i != null && i.CanWrite) {
                        if(i.CanSeek) {
                            i.Seek(0, SeekOrigin.End);
                        }
                        i.Write(buffer, offset, count);
                        i.Flush();
                    } else {
                        streams.Remove(i);
                    }
                }
            }
        }

        /// <summary>
        /// 写入一个 默认编码的 字符串
        /// </summary>
        /// <param name="content"> 要写入的内容 </param>
        public void Write(string content) {
            Write(content, DefaultWritingEncoding);
        }

        /// <summary>
        /// 写入一个指定编码的字符串
        /// </summary>
        /// <param name="content"> 要写入的内容 </param>
        /// <param name="encodeType"> 要写入的字符串编码 </param>
        public void Write(string content, Encoding encodeType) {
            var bts = encodeType.GetBytes(content);
            Write(bts, 0, bts.Length);
        }

        /// <summary>
        /// 写入一组对象拼接成的字符串
        /// </summary>
        /// <param name="content"> 要写入的第一个对象, 必须至少有一个 </param>
        /// <param name="strings"> 要写入的后续对象, 所有这些对象会被转化为字符串写入 </param>
        public void Write(object content, params object[] strings) {
            if(strings == null || strings.Length == 0) {
                Write(content.ToString());
            }
            var str = new StringBuilder(content.ToString());
            foreach(var i in strings) {
                str.Append(strings);
            }
            Write(str.ToString());
        }

        /// <summary>
        /// 写入一个字符串并自动换行
        /// </summary>
        /// <param name="content"></param>
        public void WriteLine(string content) {
            Write(content + Environment.NewLine);
        }

        /// <summary>
        /// 写入一组对象拼接成的字符串并自动换行
        /// </summary>
        /// <param name="content"> 要写入的第一个对象, 必须至少有一个 </param>
        /// <param name="strings"> 要写入的后续对象, 所有这些对象会被转化为字符串写入 </param>
        public void WriteLine(object content, params object[] strings) {
            Write(content, strings);
            Write(Environment.NewLine);
        }

        /// <summary>
        /// 添加一个输出流, 写操作会写入到这些流
        /// </summary>
        /// <param name="stream"> 要添加的输出流, 必须是可写的 </param>
        public void AddStream(Stream stream) {
            if(stream == null) {
                throw new ArgumentNullException();
            }
            if(!stream.CanWrite) {
                throw new ArgumentException("The adding stream cannot write");
            }
            lock(streams) {
                streams.Add(stream);
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
                return streams.Remove(stream);
            }
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

        private IList<Stream> streams = new List<Stream>();
    }
}
