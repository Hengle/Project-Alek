namespace ScriptableObjectArchitecture
{
    public interface IGameEventListener<T, TU>
    {
        void OnEventRaised(T value1, TU value2);
    }
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T value);
    }
    public interface IGameEventListener
    {
        void OnEventRaised();
    } 
}