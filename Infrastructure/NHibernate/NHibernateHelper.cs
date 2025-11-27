using System;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using Microsoft.Data.SqlClient;
using System.Data.Common;
using Environment = System.Environment;

namespace Infrastructure.NHibernate
{
    public static class NHibernateHelper
    {
        private static readonly Lazy<ISessionFactory> _sessionFactory = new(() => BuildSessionFactory());

        public static ISessionFactory SessionFactory => _sessionFactory.Value;
        // ENTREGA DE LA PRACTICA
        // Build a reusable NHibernate Configuration. This can be used to build a SessionFactory
        // or to perform SchemaExport operations programmatically.
        public static Configuration BuildConfiguration()
        {
            var cfg = new Configuration();
            // Look for NHibernate.cfg.xml in a few likely locations: app base, assembly location
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var cfgPath = Path.Combine(baseDir, "NHibernate.cfg.xml");
            if (!File.Exists(cfgPath))
            {
                // Try assembly location (Infrastructure output)
                var asmPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var asmDir = Path.GetDirectoryName(asmPath) ?? baseDir;
                var alt = Path.Combine(asmDir, "NHibernate.cfg.xml");
                if (File.Exists(alt)) cfgPath = alt;
            }

            if (File.Exists(cfgPath)) cfg.Configure(cfgPath);
            else
            {
                // Try to load embedded NHibernate.cfg.xml resource from this assembly
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("NHibernate.cfg.xml", StringComparison.OrdinalIgnoreCase));
                if (resourceName != null)
                {
                    using var stream = asm.GetManifestResourceStream(resourceName);
                    if (stream != null)
                    {
                        using var reader = System.Xml.XmlReader.Create(stream);
                        cfg.Configure(reader);
                    }
                }
                else
                {
                    // Last resort, attempt default configure (looks for hibernate.cfg.xml in working dir)
                    cfg.Configure();
                }
            }

            // Add mappings from the "Mappings" folder if present in either output dir
            var mappingsDir = Path.Combine(Path.GetDirectoryName(cfgPath) ?? baseDir, "Mappings");
            if (!Directory.Exists(mappingsDir)) mappingsDir = Path.Combine(baseDir, "Mappings");
            if (Directory.Exists(mappingsDir))
            {
                cfg.AddDirectory(new DirectoryInfo(mappingsDir));
            }
            else
            {
                // If mappings were embedded or copied into the assembly, try loading them from resources
                var asm = System.Reflection.Assembly.GetExecutingAssembly();
                var resources = asm.GetManifestResourceNames().Where(n => n.EndsWith(".hbm.xml", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var res in resources)
                {
                    // Load the embedded mapping, strip DOCTYPE (prevents XmlReader DTD restriction), and add as stream
                    using var rs = asm.GetManifestResourceStream(res);
                    if (rs == null) continue;
                    using var sr = new StreamReader(rs);
                    var xml = sr.ReadToEnd();
                    // Remove DOCTYPE declaration if present
                    var idx = xml.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        var end = xml.IndexOf('>', idx);
                        if (end > idx)
                        {
                            xml = xml.Remove(idx, end - idx + 1);
                        }
                    }
                    var bytes = System.Text.Encoding.UTF8.GetBytes(xml);
                    using var ms = new MemoryStream(bytes);
                    cfg.AddInputStream(ms);
                }
            }

            // If InitializeDb previously fell back to SQLite, prefer that local file only when requested
            try
            {
                // Solo usar SQLite fallback si NP_USE_SQLITE=true/1
                var useSqlite = Environment.GetEnvironmentVariable("NP_USE_SQLITE");
                var useSqliteFallback = !string.IsNullOrEmpty(useSqlite) &&
                                        (useSqlite == "1" || useSqlite.Equals("true", StringComparison.OrdinalIgnoreCase));

                if (useSqliteFallback)
                {
                    var candidates = new[]
                    {
                        Path.Combine(baseDir, "..", "..", "..", "InitializeDb", "Data", "project.db"),
                        Path.Combine(baseDir, "..", "..", "..", "..", "InitializeDb", "Data", "project.db"),
                        Path.Combine(baseDir, "..", "..", "..", "..", "..", "InitializeDb", "Data", "project.db")
                    };

                    foreach (var cand in candidates)
                    {
                        var full = Path.GetFullPath(cand);
                        if (File.Exists(full))
                        {
                            cfg.SetProperty("connection.driver_class", "NHibernate.Driver.SQLite20Driver");
                            cfg.SetProperty("dialect", "NHibernate.Dialect.SQLiteDialect");
                            cfg.SetProperty("connection.connection_string", $"Data Source={full}");
                            break;
                        }
                    }
                }
            }
            catch { }

            return cfg;
        }

        private static ISessionFactory BuildSessionFactory()
        {
            var cfg = BuildConfiguration();
            // Ensure the target database exists (helpful for local dev). If the DB specified in the
            // connection string does not exist, attempt to create it using a connection to the server's
            // master database. This avoids "Cannot open database ... requested by the login" errors
            // on fresh environments.
            try
            {
                if (cfg.Properties != null && cfg.Properties.TryGetValue("connection.connection_string", out var connStr))
                {
                    var builder = new DbConnectionStringBuilder();
                    builder.ConnectionString = connStr;

                    // Determine database name (Initial Catalog or Database)
                    string? database = null;
                    if (builder.ContainsKey("Initial Catalog")) database = builder["Initial Catalog"]?.ToString();
                    if (string.IsNullOrEmpty(database) && builder.ContainsKey("Database")) database = builder["Database"]?.ToString();

                    if (!string.IsNullOrEmpty(database))
                    {
                        // Try to connect to the specified DB briefly to see if it exists
                        try
                        {
                            using var testConn = new SqlConnection(connStr);
                            testConn.Open();
                            testConn.Close();
                        }
                        catch (SqlException ex) when (ex.Number == 4060 || ex.Message.Contains("Cannot open database"))
                        {
                            // Database does not exist or cannot be opened. Attempt to create it by connecting to master.
                            var masterBuilder = new DbConnectionStringBuilder();
                            masterBuilder.ConnectionString = connStr;
                            if (masterBuilder.ContainsKey("Initial Catalog")) masterBuilder["Initial Catalog"] = "master";
                            else if (masterBuilder.ContainsKey("Database")) masterBuilder["Database"] = "master";
                            else masterBuilder.Add("Initial Catalog", "master");

                            using var masterConn = new SqlConnection(masterBuilder.ConnectionString);
                            masterConn.Open();
                            using var cmd = masterConn.CreateCommand();
                            cmd.CommandText = $"IF DB_ID(N'{database.Replace("'", "''")}') IS NULL CREATE DATABASE [{database}]";
                            cmd.ExecuteNonQuery();
                            masterConn.Close();
                        }
                    }
                }
            }
            catch (Exception)
            {
                // If anything fails here, do not block session factory creation; let NHibernate surface the error.
            }

            return cfg.BuildSessionFactory();
        }

        public static ISession OpenSession() => SessionFactory.OpenSession();

        // Export schema to the provided connection string. Optionally override dialect and driver class.
        public static void ExportSchema(string connectionString, string? dialect = null, string? driverClass = null)
        {
            var cfg = BuildConfiguration();
            if (!string.IsNullOrEmpty(dialect)) cfg.SetProperty("dialect", dialect);
            if (!string.IsNullOrEmpty(driverClass)) cfg.SetProperty("connection.driver_class", driverClass);
            cfg.SetProperty("connection.connection_string", connectionString);

            // Use NHibernate's SchemaExport to create the schema
            var export = new SchemaExport(cfg);
            // Create the schema (do not write SQL to console, execute against DB)
            export.Create(false, true);
        }
    }
}