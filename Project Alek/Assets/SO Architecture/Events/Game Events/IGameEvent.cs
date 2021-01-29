namespace ScriptableObjectArchitecture
{
    public interface IGameEvent<T,TU>
    {
        void Raise(T value1, TU value2);
        void AddListener(IGameEventListener<T,TU> listener);
        void RemoveListener(IGameEventListener<T,TU> listener);
        void RemoveAll();
    }
    public interface IGameEvent<T>
    {
        void Raise(T value);
        void AddListener(IGameEventListener<T> listener);
        void RemoveListener(IGameEventListener<T> listener);
        void RemoveAll();
    }
    public interface IGameEvent
    {
        void Raise();
        void AddListener(IGameEventListener listener);
        void RemoveListener(IGameEventListener listener);
        void RemoveAll();
    } 
}