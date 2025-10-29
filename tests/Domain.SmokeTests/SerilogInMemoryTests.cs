using System.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Core;
using Xunit;
using Domain.SmokeTests.TestSinks;

namespace Domain.SmokeTests
{
    public class SerilogInMemoryTests
    {
        [Fact]
        public void InMemorySink_CapturesStructuredLog()
        {
            var sink = new InMemorySerilogSink();
            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Sink(sink)
                .CreateLogger();

            Log.Logger = logger;

            Log.ForContext("UserId", 42).Information("User {Name} created", "alice");
            Log.CloseAndFlush();

            var ev = sink.Events.FirstOrDefault();
            Assert.NotNull(ev);
            Assert.Equal(LogEventLevel.Information, ev.Level);
            // Rendered message should contain the name
            var rendered = ev.RenderMessage();
            // Rendering may include quotes around strings, so assert the name appears
            Assert.Contains("alice", rendered);

            // Check structured property
            Assert.True(ev.Properties.ContainsKey("UserId"));
            Assert.Equal("42", ev.Properties["UserId"].ToString());
        }
    }
}
