namespace ArmyAnt.IO {
    /// <summary>
    /// �����ٵ�С���ݴ洢�ӿ�
    /// </summary>
    public interface IQuickStorage {
        void SetItem<T>(string key, T value);
        T GetItem<T>(string key);
        void Clear();
    }
}
