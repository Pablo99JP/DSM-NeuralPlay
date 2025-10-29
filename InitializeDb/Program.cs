using System;
using System.Linq;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApplicationCore.Domain.EN;
using ApplicationCore.Domain.CEN;
using ApplicationCore.Domain.CP;
using ApplicationCore.Domain.Repositories;
using ApplicationCore.Infrastructure.Memory;
using Infrastructure.NHibernate;
using Microsoft.Data.SqlClient;

// Modes: --mode=inmemory (default) | --mode=schemaexport
// Flags: --force-drop (allow destructive recreate), --confirm (required with --force-drop), --db-name=<name>
string mode = "inmemory";
bool forceDrop = false;
bool confirm = false;
string dbName = "ProjectDatabase";
bool doSeed = false;
string? dataDirArg = null;
bool verbose = false;
string? logFile = null;
foreach (var a in args)
{
	if (a.StartsWith("--mode=", StringComparison.OrdinalIgnoreCase))
	{
		mode = a.Substring("--mode=".Length).ToLowerInvariant();
	}
	else if (a.Equals("--force-drop", StringComparison.OrdinalIgnoreCase))
	{
		forceDrop = true;
	}
	else if (a.Equals("--confirm", StringComparison.OrdinalIgnoreCase))
	{
		confirm = true;
	}
	else if (a.StartsWith("--db-name=", StringComparison.OrdinalIgnoreCase))
	{
		dbName = a.Substring("--db-name=".Length);
		if (string.IsNullOrWhiteSpace(dbName)) dbName = "ProjectDatabase";
	}
	else if (a.Equals("--seed", StringComparison.OrdinalIgnoreCase))
	{
		using System;
		using System.Threading.Tasks;

		// Delegate to programmatic entrypoint
		await InitializeDbService.RunAsync(args);
		if (string.IsNullOrWhiteSpace(dataDirArg)) dataDirArg = null;

	}
