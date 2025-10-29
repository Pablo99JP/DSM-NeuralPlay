using System.Collections.Concurrent;
using Serilog.Core;
using Serilog.Events;

namespace Domain.SmokeTests.TestSinks
{
    public class InMemorySerilogSink : ILogEventSink
    {
        private readonly ConcurrentBag<LogEvent> _events = new ConcurrentBag<LogEvent>();
        public IReadOnlyCollection<LogEvent> Events => _events.ToArray();
        public void Emit(LogEvent logEvent)
        {
            _events.Add(logEvent);
        }
    }
}
