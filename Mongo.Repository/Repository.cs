using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using MongoDB.Driver;
using Repository.Mongo;

namespace Mongo.Repository
{
    public class Repository<TEntity, TPopulate> : Repository<TEntity>
        where TEntity : IEntity
    {
        private IMongoDatabase MongoDatabase { get; }

        #region constructors

        /// <summary>
        /// if you already have mongo database and where collection name will be name of the repository
        /// </summary>
        /// <param name="mongoDatabase"></param>
        public Repository(IMongoDatabase mongoDatabase) : base(mongoDatabase)
        {
            MongoDatabase = mongoDatabase;
        }

        /// <summary>if you already have mongo database</summary>
        /// <param name="mongoDatabase"></param>
        /// <param name="collectionName"></param>
        public Repository(IMongoDatabase mongoDatabase, string collectionName) : base(mongoDatabase, collectionName)
        {
            MongoDatabase = mongoDatabase;
        }

        /// <summary>
        /// if you already have mongo client and where collection name will be name of the repository
        /// </summary>
        /// <param name="mongoClient">mongo client object</param>
        /// <param name="databaseName">database name</param>
        public Repository(IMongoClient mongoClient, string databaseName) : base(mongoClient, databaseName)
        {
            MongoDatabase = mongoClient.GetDatabase(databaseName);
        }

        /// <summary>if you already have mongo client</summary>
        /// <param name="mongoClient">mongo client object</param>
        /// <param name="databaseName">database name</param>
        /// <param name="collectionName">collection name</param>
        public Repository(IMongoClient mongoClient, string databaseName, string collectionName) : base(mongoClient,
            databaseName, collectionName)
        {
            MongoDatabase = mongoClient.GetDatabase(databaseName);
        }

        /// <summary>where collection name will be name of the repository</summary>
        /// <param name="connectionString">connection string</param>
        public Repository(string connectionString) : base(connectionString)
        {
            MongoDatabase = new MongoClient(connectionString).GetDatabase(connectionString.Split('/').Last());
        }

        /// <summary>with custom settings</summary>
        /// <param name="connectionString">connection string</param>
        /// <param name="collectionName">collection name</param>
        public Repository(string connectionString, string collectionName) : base(connectionString, collectionName)
        {
            MongoDatabase = new MongoClient(connectionString).GetDatabase(connectionString.Split('/').Last());
        }

        #endregion

        /// <summary>
        /// map populateKeyInfo items except the Path property and generate new <see cref="TPopulate"/> object
        /// </summary>
        /// <param name="entity">the document object</param>
        /// <param name="options">Populate Options</param>
        /// <returns>new <see cref="TPopulate"/> object</returns>
        private TPopulate MapEntityToPopulate(TEntity entity, PopulateOptions options)
        {
            var mapper = new Mapper(new MapperConfiguration(
                expression =>
                    expression.CreateMap<TEntity, TPopulate>().ForMember(options.Key,
                        configurationExpression => configurationExpression.Ignore())));
            return mapper.Map<TPopulate>(entity);
        }

        public TPopulate Populate(TEntity entity, PopulateOptions options)
        {
            var ops = options;
            var populate = MapEntityToPopulate(entity, options);
            do
            {
                var depth = options.Key.Split('.').Length - 1;

                var (parent, childInfo) = GetPopulateKey(populate, options, depth);
                if (childInfo.GetValue(parent) is List<object> populateKeys)
                {
                    PopulateList(childInfo, parent, populateKeys, options);
                }
                else
                {
                    PopulateItem(childInfo, parent, childInfo.GetValue(parent), options);
                }

                ops = ops.ChildPopulateOptions;
            } while (ops != null);

            return populate;
        }

        /// <summary>
        /// return the <see cref="PropertyInfo"/> and the parent for the key will be populated 
        /// </summary>
        /// <param name="populated">the root and it is of type <see cref="TPopulate"/></param>
        /// <param name="options">the parent options</param>
        /// <param name="depth">the depth to go in the path</param>
        /// <returns></returns>
        private (object parent, PropertyInfo childInfo) GetPopulateKey(object populated, PopulateOptions options,
            int depth)
        {
            var pathSplit = options.Key.Split('.');
            PropertyInfo childInfo = null;
            var parent = populated;
            for (var index = 0; index <= depth; index++)
            {
                if (index == depth)
                    childInfo = parent?.GetType().GetProperty(pathSplit.ElementAt(index));
                else
                    parent = parent?.GetType().GetProperty(pathSplit.ElementAt(index))?
                        .GetValue(parent);
            }

            return (parent, childInfo);
        }


        private void PopulateList(PropertyInfo populateKeyInfo, object parent, IEnumerable<object> populateKeys,
            PopulateOptions options)
        {
            if (populateKeyInfo == null) throw new ArgumentNullException(nameof(populateKeyInfo));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (!(populateKeyInfo.GetValue(parent) is List<object> itemsList)) return;

            foreach (var populateKey in populateKeys)
            {
                itemsList.AddRange(FindItems(populateKey, options));
            }
        }

        private void PopulateItem(PropertyInfo populateKeyInfo, object parent, object populateKey,
            PopulateOptions options)
        {
            if (populateKeyInfo == null) throw new ArgumentNullException(nameof(populateKeyInfo));
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (options == null) throw new ArgumentNullException(nameof(options));

            populateKeyInfo.SetValue(parent, FindItems(populateKey, options));
        }

        private IEnumerable<object> FindItems(object item, PopulateOptions options)
        {
            var filter = Builders<object>.Filter.Eq(options.ReferenceKey, item);
            return MongoDatabase.GetCollection<object>(options.Reference).Find(filter).ToList();
        }
    }
}