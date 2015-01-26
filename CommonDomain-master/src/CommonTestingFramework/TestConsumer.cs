using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonDomainLibrary;
using CommonDomainLibrary.Common;
using NLog;
using NodaTime;

namespace CommonTestingFramework
{
    public class TestConsumer<T>: IDisposable, IHandle<T> where T : class, IMessage
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<Guid, List<T>> _receivedMessages;
        private ConcurrentDictionary<Guid, ConcurrentDictionary<Type,TaskCompletionSource<bool>>> _correlatedLocks;
        private ConcurrentDictionary<Guid, Action<IMessage>> _correlatedCallbacks;
        private Instant _startTime;

        public TestConsumer()
        {
            _correlatedLocks = new ConcurrentDictionary<Guid, ConcurrentDictionary<Type, TaskCompletionSource<bool>>>();
            _correlatedCallbacks = new ConcurrentDictionary<Guid, Action<IMessage>>();
            _receivedMessages = new ConcurrentDictionary<Guid, List<T>>();
        }

        public async Task Handle(T e, bool lastTry)
        {
            _logger.Debug("BEGIN: DELAYINGCONSUMER<{0}>: Got message", e.GetType().Name);
            
            if (!_receivedMessages.ContainsKey(e.CorrelationId))
            {
                _receivedMessages[e.CorrelationId] = new List<T>();
            }

            if (!_receivedMessages[e.CorrelationId].Contains(e))
            {
                _receivedMessages[e.CorrelationId].Add(e);
                
                if (_correlatedCallbacks.ContainsKey(e.CorrelationId))
                {
                    _correlatedCallbacks[e.CorrelationId](e);
                }

                _logger.Debug("DELAYINGCONSUMER<{0}>: Checking if correlation id {1} matches any I am waiting for", e.GetType().Name, e.CorrelationId);
                if (!_correlatedLocks.ContainsKey(e.CorrelationId)) _logger.Debug("DELAYINGCONSUMER<{0}>: No matching key found in the dictionary", e.GetType().Name);
                else
                {
                    _logger.Debug("DELAYINGCONSUMER<{0}>: Matching key found in the dictionary. Checking for the message type key", e.GetType().Name);
                    if (!_correlatedLocks[e.CorrelationId].ContainsKey(typeof (T)))
                        _logger.Debug(
                            "DELAYINGCONSUMER<{0}>: No matching message type key found in the dictionary", e.GetType().Name);
                    else
                    {
                        _logger.Debug("DELAYINGCONSUMER<{0}>: Matching key for the message type found in the dictionary. Checking for the task status", e.GetType().Name);
                        if (!_correlatedLocks[e.CorrelationId][typeof(T)].Task.IsCompleted)
                        {
                            _logger.Debug("DELAYINGCONSUMER<{0}>: Running task found ! Unlocking !", e.GetType().Name);
                            _correlatedLocks[e.CorrelationId][typeof(T)].SetResult(true);
                        }
                        else _logger.Debug("DELAYINGCONSUMER<{0}>: No running task found for this message!", e.GetType().Name);
                    }
                }

                //if (_correlatedLocks.ContainsKey(e.CorrelationId) && _correlatedLocks[e.CorrelationId].ContainsKey(typeof(T)) && 
                //    !_correlatedLocks[e.CorrelationId][typeof(T)].Task.IsCompleted)
                //{
                //    Logger.DebugFormat("DELAYINGCONSUMER<{0}>: Match for Correlation id {1} found", e.GetType().Name, e.CorrelationId);
                //    _correlatedLocks[e.CorrelationId][typeof(T)].SetResult(true);
                //}
                //else Logger.DebugFormat("DELAYINGCONSUMER<{0}>: Match for correlation id {1} not found", e.GetType().Name, e.CorrelationId);
            }
            _logger.Debug("END: DELAYINGCONSUMER<{0}>: Got message", e.GetType().Name);
        }

        public T WaitForMessageSync(Guid correlationId, Action<T> handleCallback = null)
        {
            var task = WaitForMessage(correlationId, handleCallback);
            Task.WaitAll(task);
            return task.Result;
        }

        public async Task<T> WaitForMessage(Guid correlationId, Action<T> handleCallback = null)
        {
            if (handleCallback != null)
            {
                _correlatedCallbacks[correlationId] = message => handleCallback((T)message);
            }

            return await WaitForMessage(Duration.FromMinutes(5), correlationId);
        }

        public async Task<T> WaitForMessage(Duration t, Guid correlationId)
        {
            _startTime = Instant.FromDateTimeUtc(DateTime.UtcNow);            

            if(!_correlatedLocks.ContainsKey(correlationId)) _correlatedLocks[correlationId] = new ConcurrentDictionary<Type, TaskCompletionSource<bool>>();
            _correlatedLocks[correlationId][typeof(T)] = new TaskCompletionSource<bool>();

            try
            {
                _logger.Debug("DELAYINGCONSUMER<" + typeof(T).Name + ">({0}): Starting waiting", correlationId);
                await _correlatedLocks[correlationId][typeof(T)].Task.TimeoutAfter((int)t.ToTimeSpan().TotalMilliseconds);
            }
            catch (Exception)
            {
                _logger.Debug("DELAYINGCONSUMER<" + typeof(T).Name + ">({1}): Quitting. Started waiting at {0}", _startTime, correlationId);
                throw new SystemException("DELAYINGCONSUMER<" + typeof(T).Name + ">(" + correlationId + "): No '" + typeof(T).Name + "' message received");
            }

            return _receivedMessages[correlationId].FirstOrDefault();
        }

        public async Task<T> WaitForMessage(Duration minWait, Duration t, Guid correlationId)
        {
            _startTime = Instant.FromDateTimeUtc(DateTime.UtcNow);            

            if (!_correlatedLocks.ContainsKey(correlationId)) _correlatedLocks[correlationId] = new ConcurrentDictionary<Type, TaskCompletionSource<bool>>();
            _correlatedLocks[correlationId][typeof(T)] = new TaskCompletionSource<bool>();

            try
            {
                _logger.Debug("DELAYINGCONSUMER<" + typeof(T).Name + ">({0}): Starting waiting", correlationId);
                await _correlatedLocks[correlationId][typeof(T)].Task.TimeoutAfter((int)t.ToTimeSpan().TotalMilliseconds);                
            }
            catch (Exception)
            {
                _logger.Debug("DELAYINGCONSUMER<" + typeof(T).Name + ">({1}): Quitting. Started waiting at {0}", _startTime, correlationId);
                throw new SystemException("DELAYINGCONSUMER<" + typeof(T).Name + ">(" + correlationId + "): No '" + typeof(T).Name + "' message received");
            }

            if (Instant.FromDateTimeUtc(DateTime.UtcNow) < _startTime.Plus(minWait)) throw new SystemException("DELAYINGCONSUMER<" + typeof(T).Name + ">(" + correlationId + "): Message of type '" + typeof(T).Name + "' arrived too soon");
            return _receivedMessages[correlationId].FirstOrDefault();
        }

        public async Task<List<T>> WaitForMessage(Duration t, int numberOfDistinctMessages, Guid correlationId)
        {
            _startTime = Instant.FromDateTimeUtc(DateTime.UtcNow);            

            if (!_correlatedLocks.ContainsKey(correlationId)) _correlatedLocks[correlationId] = new ConcurrentDictionary<Type, TaskCompletionSource<bool>>();
            _correlatedLocks[correlationId][typeof(T)] = new TaskCompletionSource<bool>();
            _receivedMessages[correlationId] = new List<T>();

            while (_receivedMessages[correlationId].Count < numberOfDistinctMessages)
            {
                try
                {
                    _logger.Debug("DELAYINGCONSUMER<" + typeof(T).Name + ">({0}): Starting waiting", correlationId);
                    await _correlatedLocks[correlationId][typeof(T)].Task.TimeoutAfter((int)t.ToTimeSpan().TotalMilliseconds);
                }
                catch (Exception)
                {
                    _logger.Debug("DELAYINGCONSUMER<" + typeof(T).Name + ">({1}): Quitting. Started waiting at {0}", _startTime, correlationId);
                    throw new SystemException("DELAYINGCONSUMER<" + typeof(T).Name + ">(" + correlationId + "): Only " + _receivedMessages[correlationId].Count + "/" + numberOfDistinctMessages + " '" + typeof(T).Name + "' messages received");
                }

                lock (_receivedMessages)
                {
                    if (_receivedMessages[correlationId].Count < numberOfDistinctMessages) _correlatedLocks[correlationId][typeof(T)] = new TaskCompletionSource<bool>();
                    else break;
                }
            }

            return _receivedMessages[correlationId];
        }

        public void Dispose()
        {
            foreach (var key in _correlatedLocks.Keys)
            {
                foreach (var key2 in _correlatedLocks[key].Keys)
                {
                    _correlatedLocks[key][key2].TrySetCanceled();
                }
            }
        }
    }
}
