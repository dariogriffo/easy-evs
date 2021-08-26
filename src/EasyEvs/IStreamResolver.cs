namespace EasyEvs
{
    public interface IStreamResolver
    {
        string StreamNameFor<T>(T @event) where T : IEvent;
    }
}
