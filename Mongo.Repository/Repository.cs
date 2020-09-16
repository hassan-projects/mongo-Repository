using System;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Repository.Mongo;


namespace Mongo.Repository
{
    public class Repository<T> : global::Repository.Mongo.Repository<T> where T : IEntity
    {
        #region constructors

        public Repository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
        }

        public Repository(IMongoDatabase mongoDatabase, string collectionName) : base(mongoDatabase, collectionName)
        {
        }

        public Repository(IMongoClient mongoClient, string databaseName) : base(mongoClient, databaseName)
        {
        }

        public Repository(IMongoClient mongoClient, string databaseName, string collectionName) : base(mongoClient, databaseName, collectionName)
        {
        }

        public Repository(IConfiguration config) : base(config)
        {
        }

        public Repository(string connectionString) : base(connectionString)
        {
        }

        public Repository(string connectionString, string collectionName) : base(connectionString, collectionName)
        {
        }

        #endregion

    }
}
