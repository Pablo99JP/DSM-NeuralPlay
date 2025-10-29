using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

public class InitializeDbConcurrencyTests
{
    [Fact]
    public async Task Multiple_InMemory_Runs_DoNotFail()
    {
        var tasks = new Task<int>[6];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = InitializeDbService.RunAsync(new[] { "--mode=inmemory" }, new StringWriter());
        }

        var results = await Task.WhenAll(tasks);
        foreach (var r in results)
        {
            Assert.Equal(0, r);
        }
    }
}
