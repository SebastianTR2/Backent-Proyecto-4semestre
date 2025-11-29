using Machly.Api.Config;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Machly.Api.Repositories
{
    public class MongoDbContext
    {
        public IMongoDatabase Database { get; }

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            Database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<T> GetCollection<T>(string name) =>
            Database.GetCollection<T>(name);
    }
}
