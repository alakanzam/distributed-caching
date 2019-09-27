using DistributedCacheExercise.Constants;
using DistributedCacheExercise.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace DistributedCacheExercise.Extensions
{
    public static class MongoDatabaseExtensions
    {
        #region Methods

        public static void AddGlobalMongoCacheDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            // Make mongodb save object with camel-cased naming convention.
            var conventionPack = new ConventionPack();
            conventionPack.Add(new CamelCaseElementNameConvention());
            conventionPack.Add(new IgnoreExtraElementsConvention(true));
            conventionPack.AddClassMapConvention("LenientDiscriminator", m => m.SetDiscriminatorIsRequired(false));
            ConventionRegistry.Register("camelCase", conventionPack, t => true);

            // Resolve order database.
            var mongoDbConnectionString =
                configuration.GetConnectionString(ConnectionStringConstants.GlobalCache);

            // Main mongo database url.
            var mainMongoDatabaseUrl = new MongoUrl(mongoDbConnectionString);
            services.AddSingleton<IMongoClient>(options => new MongoClient(mainMongoDatabaseUrl));

            services.AddSingleton(options =>
            {
                var mongoClient = options.GetService<IMongoClient>();
                return mongoClient.GetDatabase(DatabaseNameConstants.GlobalCache);
            });

            AddKeyValueItemCollection(services);
        }

        /// <summary>
        /// Add key value item collection into global cache.
        /// </summary>
        /// <param name="services"></param>
        private static void AddKeyValueItemCollection(IServiceCollection services)
        {
            // Add users collection.
            services.AddSingleton(options =>
            {
                // Get mongo client.
                var mongoDatabase = options.GetService<IMongoDatabase>();
                return mongoDatabase.GetCollection<KeyValueItem>(CollectionNameConstants.KeyValueItem);
            });

            BsonClassMap.RegisterClassMap<KeyValueItem>(options =>
            {
                options.AutoMap();
                options.MapCreator(x => new KeyValueItem(x.Key));

                options.SetIgnoreExtraElements(true);
            });
        }

        #endregion
    }
}