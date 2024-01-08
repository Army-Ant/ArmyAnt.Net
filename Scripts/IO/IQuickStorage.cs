namespace ArmyAnt.IO {
    /// <summary>
    /// 简洁快速的小数据存储接口
    /// </summary>
    public interface IQuickStorage {
        void SetItem<T>(string key, T value);
        T GetItem<T>(string key);
        void Clear();
    }
}
