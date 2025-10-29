using System;
using System.IO;
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
            // Look for NHibernate.cfg.xml in the application's base directory
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var cfgPath = Path.Combine(baseDir, "NHibernate.cfg.xml");
            if (File.Exists(cfgPath)) cfg.Configure(cfgPath);
            else cfg.Configure();

            // Add mappings from the "Mappings" folder if present in output
            var mappingsDir = Path.Combine(baseDir, "Mappings");
            if (Directory.Exists(mappingsDir)) cfg.AddDirectory(new DirectoryInfo(mappingsDir));

            return cfg.BuildSessionFactory();
        }

        public static ISession OpenSession() => SessionFactory.OpenSession();
    }
}
