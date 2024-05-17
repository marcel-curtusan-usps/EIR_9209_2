using EIR_9209_2.Models;
using System.Collections.Concurrent;

namespace EIR_9209_2.InMemory
{
    public class InMemoryTagsRepository : IInMemoryTagsRepository
    {
        public readonly static ConcurrentDictionary<string, GeoMarker> _tagList = new();
        public void Add(GeoMarker tag)
        {
            _tagList.TryAdd(tag.Properties.Id, tag);
        }

        public void Remove(string connectionId)
        {
            _tagList.TryRemove(connectionId, out _);
        }

        public GeoMarker Get(string id)
        {
            _tagList.TryGetValue(id, out GeoMarker tag);
            return tag;
        }

        public IEnumerable<GeoMarker> GetAll()
        {
            return _tagList.Values;
        }

        public void Update(GeoMarker tag)
        {
            if (_tagList.TryGetValue(tag.Properties.Id, out GeoMarker currentTag))
            {
                _tagList.TryUpdate(tag.Properties.Id, tag, currentTag);
            }
        }
    }
}
