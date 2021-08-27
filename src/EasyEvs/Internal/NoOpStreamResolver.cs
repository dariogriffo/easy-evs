namespace EasyEvs.Internal
{
#pragma warning disable 1591
    public class NoOpStreamResolver : IStreamResolver
#pragma warning restore 1591
    {
#pragma warning disable 1591
        public string StreamNameFor<T>(T @event) where T : IEvent
#pragma warning restore 1591
        {
            throw new System.NotImplementedException();
        }
    }
}
