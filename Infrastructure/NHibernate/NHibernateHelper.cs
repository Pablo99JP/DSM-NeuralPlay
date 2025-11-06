using System;
using System.IO;
using NHibernate;
using NHibernate.Cfg;

namespace Infrastructure.NHibernate
{
    public static class NHibernateHelper
    {
        private static ISessionFactory _sessionFactory;
        private static readonly object _lock = new object();

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    lock (_lock)
                    {
                        if (_sessionFactory == null)
                        {
                            _sessionFactory = BuildSessionFactory();
                        }
                    }
                }
                return _sessionFactory;
            }
        }

        private static ISessionFactory BuildSessionFactory()
        {
            try
            {
                var configuration = new Configuration();
                
                var cfgPath = Path.Combine(AppContext.BaseDirectory, "NHibernate.cfg.xml");
                if (!File.Exists(cfgPath))
                {
                    cfgPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "NHibernate.cfg.xml");
                }

                if (File.Exists(cfgPath))
                {
                    configuration.Configure(cfgPath);
                }
                else
                {
                    configuration.Configure();
                }

                RewriteMappingPaths(configuration);

                return configuration.BuildSessionFactory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al construir SessionFactory: {ex.Message}");
                throw;
            }
        }

        private static void RewriteMappingPaths(Configuration configuration)
        {
            var baseDir = AppContext.BaseDirectory;
            var mappingsDir = Path.Combine(baseDir, "Infrastructure", "NHibernate", "Mappings");

            if (!Directory.Exists(mappingsDir))
            {
                mappingsDir = Path.Combine(baseDir, "..", "..", "..", "Infrastructure", "NHibernate", "Mappings");
            }

            if (Directory.Exists(mappingsDir))
            {
                foreach (var file in Directory.GetFiles(mappingsDir, "*.hbm.xml"))
                {
                    configuration.AddFile(file);
                }
            }
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
