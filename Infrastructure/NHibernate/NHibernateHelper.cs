using System;
using System.IO;
using System.Linq;
using NHibernate;
using NHibernate.Cfg;

namespace Infrastructure.NHibernate
{
    public static class NHibernateHelper
    {
        private static readonly Lazy<ISessionFactory> _sessionFactory = new(() => BuildSessionFactory());

        public static ISessionFactory SessionFactory => _sessionFactory.Value;

        private static ISessionFactory BuildSessionFactory()
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

            return cfg.BuildSessionFactory();
        }

        public static ISession OpenSession() => SessionFactory.OpenSession();
    }
}
