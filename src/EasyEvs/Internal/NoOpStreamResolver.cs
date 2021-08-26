namespace EasyEvs.Internal
{
    public class NoOpStreamResolver : IStreamResolver
    {
        public string StreamNameFor<T>(T @event) where T : IEvent
        {
            throw new System.NotImplementedException();
        }
    }
}
