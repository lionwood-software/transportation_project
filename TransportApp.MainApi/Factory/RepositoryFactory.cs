using Configuration.Core;
using Microsoft.Extensions.Options;
using Repository.Core;
using Repository.MongoDb;
using System.Collections.Generic;
using System.Linq;

namespace TransportApp.MainApi.Factory
{
    public class RepositoryFactory
    {
        private readonly List<DbConfig> _dbConfig;
        public RepositoryFactory(IOptions<ApiConfigurationOptions> options)
        {
            _dbConfig = options.Value?.DB.ToList() ?? new List<DbConfig>();
        }
        public IRepository GetRepository(string cityCode)
        {
            var repositoryConfig = _dbConfig.Where(x => x.Name == cityCode)
                .Select(item => new DbConfiguration
                {
                    ConnectionString = item.ConnectionString,
                    Database = item.Database,
                    Name = item.Name,
                    Type = item.Type
                })
                .First();

            return new MongoDbRepository(repositoryConfig);
        }

        public List<IRepository> GetAllRepository()
        {
            List<IRepository> repositories = new List<IRepository>();

            var repositoryConfig = _dbConfig.Where(x => !x.Name.Contains("-Logs"))
                .Select(item => new DbConfiguration
                {
                    ConnectionString = item.ConnectionString,
                    Database = item.Database,
                    Name = item.Name,
                    Type = item.Type
                })
                .ToList();

            repositories.AddRange(repositoryConfig.Select(x => new MongoDbRepository(x)));

            return repositories;
        }

        private class DbConfiguration : DbConfig, IRepositoryConfiguration
        {
        }
    }
}
