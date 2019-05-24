using NotSupportedException = System.NotSupportedException;

namespace ArmyAnt.IO {
    /// <summary>
    /// 用于只能写入数据的 Stream 的父类, 将一些与读相关的 API 实现为恒抛出 NotSupportedException
    /// </summary>
    public abstract class WriteOnlyStream : System.IO.Stream {
        public sealed override bool CanRead => false;

        public sealed override bool CanSeek => false;

        public sealed override long Length => throw new NotSupportedException("This stream have no length");

        public sealed override long Position {
            get => throw new NotSupportedException("This stream have no position");
            set => throw new NotSupportedException("This stream cannot set position");
        }

        public override void Flush() {
            // Nothing to do
        }

        public sealed override int Read(byte[] buffer, int offset, int count) {
            throw new NotSupportedException("This stream cannot read");
        }

        public sealed override long Seek(long offset, System.IO.SeekOrigin origin) {
            throw new NotSupportedException("This stream cannot seek");
        }

        public sealed override void SetLength(long value) {
            throw new NotSupportedException("This stream cannot set length");
        }
    }
}
