using Xunit;

namespace Domain.SmokeTests
{
    // Collection definition that disables parallelization for tests that need global/static isolation.
    [CollectionDefinition("NonParallel", DisableParallelization = true)]
    public class NonParallelCollection
    {
    }
}
