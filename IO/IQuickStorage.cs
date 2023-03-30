namespace ArmyAnt.IO
{
    public interface IQuickStorage
    {
        void SetItem<T>(string key, T value);
        T GetItem<T>(string key);
        void Clear();
    }
}
