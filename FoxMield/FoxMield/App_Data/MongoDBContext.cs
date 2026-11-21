
using MongoDB.Driver;
using FoxMield.Components.Models;

namespace FoxMield.App_Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public IMongoCollection<Deck> Decks =>
            _database.GetCollection<Deck>("Decks");

        public IMongoCollection<ApplicationUser> Users =>
            _database.GetCollection<ApplicationUser>("Users");

        public MongoDbContext(IConfiguration configuration)
        {
            string connectionString =
                configuration["MongoDB:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "MongoDB connection string is missing.");

            string databaseName =
                configuration["MongoDB:DatabaseName"]
                ?? "FoxMieldDB";

            var client = new MongoClient(connectionString);

            _database = client.GetDatabase(databaseName);
        }

        public void TestConnection()
        {
            _database.RunCommand<dynamic>(
                new MongoDB.Bson.BsonDocument("ping", 1)
            );
        }
    }
}
