using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApplicationCore.Domain.Repositories;

namespace ApplicationCore.Infrastructure.Memory
{
    // Generic in-memory repository using reflection to find Id property.
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
    private static readonly ConcurrentDictionary<long, T> _store = new();
    private static long _seq = 0;
        private readonly PropertyInfo? _idProp;

        public InMemoryRepository()
        {
            // find property that starts with "Id" (case-insensitive)
            _idProp = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.Name.StartsWith("Id", StringComparison.OrdinalIgnoreCase) && (p.PropertyType == typeof(long) || p.PropertyType == typeof(long?)));
        }

        protected long NextId() => System.Threading.Interlocked.Increment(ref _seq);

        public virtual T? ReadById(long id)
        {
            _store.TryGetValue(id, out var v);
            return v;
        }

    public virtual IEnumerable<T> ReadAll() => _store.Values.ToList();

    public virtual IEnumerable<T> ReadFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return ReadAll();
            filter = filter.ToLowerInvariant();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string)).ToArray();
            return _store.Values.Where(item => props.Any(p => (p.GetValue(item) as string)?.ToLowerInvariant().Contains(filter) == true)).ToList();
        }

    public virtual void New(T entity)
        {
            if (_idProp != null)
            {
                var id = NextId();
                if (_idProp.PropertyType == typeof(long)) _idProp.SetValue(entity, id);
                else _idProp.SetValue(entity, (long?)id);
                _store.TryAdd(id, entity);
            }
            else
            {
                throw new InvalidOperationException($"Entity {typeof(T).Name} has no long Id property starting with 'Id'.");
            }
        }

    public virtual void Modify(T entity)
        {
            if (_idProp == null) throw new InvalidOperationException("No id property");
            var val = _idProp.GetValue(entity);
            if (val == null) throw new InvalidOperationException("Entity id is null");
            var id = Convert.ToInt64(val);
            _store[id] = entity;
        }

        public virtual void Destroy(long id)
        {
            _store.TryRemove(id, out _);
        }
    }
}
