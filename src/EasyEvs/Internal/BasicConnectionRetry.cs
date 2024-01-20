namespace EasyEvs.Internal;

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
        if (_settings.SubscriptionReconnectionAttempts == 0)
        {
            await func();
            return;
        }

        bool forever = _settings.SubscriptionReconnectionAttempts < 0;
        int attempt = 0;
        Exception lastException;
        do
        {
            try
            {
                if (attempt > 0)
                {
                    await Task.Delay(_settings.SubscriptionReconnectionInterval);
                    _logger.LogDebug("Retrying to subscribe attempt {Attempt}", attempt);
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

            attempt++;
        } while (forever || attempt < _settings.SubscriptionReconnectionAttempts);

        throw lastException;
    }

    public async Task Write(Func<Task> func, Func<Exception, Task> onException)
    {
        int reconnectionAttempts = _settings.WriteReconnectionAttempts;
        if (reconnectionAttempts == 0)
        {
            await func();
            return;
        }

        bool forever = reconnectionAttempts < 0;
        int attempt = 0;
        Exception lastException;
        do
        {
            try
            {
                if (attempt > 0)
                {
                    await Task.Delay(_settings.WriteReconnectionInterval);
                    _logger.LogDebug("Retrying to write attempt {WriteAttempt}", attempt);
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

            attempt++;
        } while (forever || attempt < reconnectionAttempts);

        throw lastException;
    }

    public async Task Read(Func<Task> func, Func<Exception, Task> onException)
    {
        if (_settings.ReadReconnectionAttempts == 0)
        {
            await func();
            return;
        }

        bool forever = _settings.ReadReconnectionAttempts < 0;
        int attempt = 0;
        Exception lastException;
        do
        {
            try
            {
                if (attempt > 0)
                {
                    await Task.Delay(_settings.ReadReconnectionInterval);
                    _logger.LogDebug("Retrying to read attempt {RetryAttempt}", attempt);
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

            attempt++;
        } while (forever || attempt < _settings.ReadReconnectionAttempts);

        throw lastException;
    }
}
