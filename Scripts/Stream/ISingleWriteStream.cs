namespace ArmyAnt.Stream {
    public interface ISingleWriteStream : IStream {
        int Write(byte[] buffer);
        System.Threading.Tasks.Task<int> WriteAsync(byte[] buffer);
    }
}
