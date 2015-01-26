using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edit;
using NLog;

namespace EventStore
{
    public class InMemoryStreamStore : IStreamStore
    {
        private Dictionary<string,List<Chunk>> _chunks = new Dictionary<string, List<Chunk>>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public async Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, IStoredDataVersion expectedVersion = null)
        {
            Logger.Debug("BEGIN: persisting chunks to the store");
            lock (_chunks)
            {
                var enumerable = chunks as Chunk[] ?? chunks.ToArray();
                foreach (var chunk in enumerable)
                {
                    Logger.Debug("Adding message '{0}' to the store", chunk.Instance.GetType());
                }
                Logger.Debug("END: chunks persisted");

                _chunks[streamName] = new List<Chunk>();
                _chunks[streamName].AddRange(enumerable);
            }
        }

        public Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, TimeSpan timeout, IStoredDataVersion expectedVersion = null)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, CancellationToken token, IStoredDataVersion expectedVersion = null)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string streamName, IEnumerable<Chunk> chunks, TimeSpan timeout, CancellationToken token,
                                IStoredDataVersion expectedVersion = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ChunkSet> ReadAsync(string streamName)
        {
            lock (_chunks)
            {
                if (!_chunks.ContainsKey(streamName)) return new ChunkSet(Enumerable.Empty<Chunk>(), null);

                return new ChunkSet(_chunks[streamName], new SomeDataVersion());
            }
        }

        public Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public Task<ChunkSet> ReadAsync(string streamName, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<ChunkSet> ReadAsync(string streamName, TimeSpan timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void ClearStore()
        {
            _chunks = new Dictionary<string, List<Chunk>>();
        }

        private class SomeDataVersion : IStoredDataVersion { }
    }
}
