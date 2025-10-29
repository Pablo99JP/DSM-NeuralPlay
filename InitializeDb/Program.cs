using System;

// Program entrypoint delegates to InitializeDbService so tests and tooling can call the same logic.
await InitializeDbService.RunAsync(args);
 
