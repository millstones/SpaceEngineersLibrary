using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IngameScript
{
    class Repository<TId, TItem>
    {
        public TItem[] Values => _dictionary.Values.ToArray();
        public TId[] Ids => _dictionary.Keys.ToArray();
        readonly Dictionary<TId, TItem> _dictionary = new Dictionary<TId, TItem>();

        public void Add(TId id, TItem item)
        {
            if (!_dictionary.ContainsKey(id))
                _dictionary.Add(id, item);
        }

        public void Remove(TItem item)
        {
            if (_dictionary.ContainsValue(item))
                _dictionary.Remove(GetKeyFor(item));
        }

        public void Remove(TId id)
        {
            if (_dictionary.ContainsKey(id))
                _dictionary.Remove(id);
        }

        public void Replace(TItem item)
        {
            if (_dictionary.ContainsValue(item))
                _dictionary[GetKeyFor(item)] = item;
        }

        public void AddOrReplace(TId id, TItem item)
        {
            if (_dictionary.ContainsKey(id))
                _dictionary[id] = item;
            else
                _dictionary.Add(id, item);
        }

        public TItem GetOrDefault(TId id) => _dictionary.GetValueOrDefault(id);

        public TItem GetOrFirst(TId id)
        {
            TItem item;
            return _dictionary.TryGetValue(id, out item) ? item : _dictionary.Values.First();
        }

        public bool Contains(TId id) => _dictionary.ContainsKey(id);
        public bool Contains(TItem item) => _dictionary.ContainsValue(item);

        public bool Contains(Func<TItem, bool> predicate) => _dictionary.Values.Any(predicate);

        public TId GetKeyFor(TItem item) => _dictionary.FirstOrDefault(x => x.Value.Equals(item)).Key;

        public TId GetKeyFor(Func<TItem, bool> predicate, TId @default)
        {
            var retVal = _dictionary.FirstOrDefault(x => predicate(x.Value));

            return (retVal.Key == null) ? @default : retVal.Key;
        }
    }
}