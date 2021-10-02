namespace EasyEvs.Internal
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Microsoft.Extensions.Logging;

    internal class BasicConnectionRetry : IConnectionRetry
    {
        private readonly EventStoreSettings _settings;
        private readonly ILogger<BasicConnectionRetry> _logger;

        public BasicConnectionRetry(EventStoreSettings settings, ILogger<BasicConnectionRetry> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task Subscribe(Func<Task> func, Func<Exception, Task> onException)
        {
            var forever = _settings.SubscriptionReconnectionAttempts < 0;
            int attempted = 0;
            Exception lastException;
            do
            {
                try
                {
                    if (attempted > 0)
                    {
                        await Task.Delay(_settings.SubscriptionReconnectionInterval);
                        _logger.LogDebug($"Retrying to subscribe attempt {attempted}");
                    }

                    await func();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error subscribing");
                    lastException = ex;
                    await onException(ex);
                }

                attempted++;
            } while (forever || attempted < _settings.SubscriptionReconnectionAttempts);

            throw lastException;
        }

        public async Task Write(Func<Task> func, Func<Exception, Task> onException)
        {
            var forever = _settings.WriteReconnectionAttempts < 0;
            int attempted = 0;
            Exception lastException;
            do
            {
                try
                {
                    if (attempted > 0)
                    {
                        await Task.Delay(_settings.WriteReconnectionInterval);
                        _logger.LogDebug($"Retrying to write attempt {attempted}");
                    }

                    await func();
                    return;
                }
                catch (StreamAlreadyExists)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error writing to Event Store");
                    lastException = ex;
                    await onException(ex);
                }

                attempted++;
            } while (forever || attempted < _settings.WriteReconnectionAttempts);

            throw lastException;
        }

        public async Task Read(Func<Task> func, Func<Exception, Task> onException)
        {
            var forever = _settings.ReadReconnectionAttempts < 0;
            int attempted = 0;
            Exception lastException;
            do
            {
                try
                {
                    if (attempted > 0)
                    {
                        await Task.Delay(_settings.ReadReconnectionInterval);
                        _logger.LogDebug($"Retrying to read attempt {attempted}");
                    }

                    await func();
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading to Event Store");
                    lastException = ex;
                    await onException(ex);
                }

                attempted++;
            } while (forever || attempted < _settings.ReadReconnectionAttempts);

            throw lastException;
        }
    }
}
