public interface ISerializer<TState>
{
    string Serialize(TState state);
    TState Deserialize(string payload);
}