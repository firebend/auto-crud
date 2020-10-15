using System;
using System.Reflection;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Firebend.AutoCrud.Mongo.Configuration
{
    public class MongoDbConfigurator : IMongoDbConfigurator
    {
        private static bool _configured;
        private static readonly object Key = new object();

        public void Configure()
        {
            if (_configured) return;

            lock (Key)
            {
                if (_configured) return;

                _configured = true;

                BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));
                BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
                BsonSerializer.RegisterSerializer(typeof(decimal?), new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

                BsonTypeMapper.RegisterCustomTypeMapper(typeof(DateTimeOffset), new MongoDateTimeOffsetBsonTypeMapper());

                var pack = new ConventionPack
                {
                    new CamelCaseElementNameConvention(),
                    new EnumRepresentationConvention(BsonType.String),
                    new IgnoreExtraElementsConvention(true)
                };

                var mongoEntityType = typeof(IEntity<>);
                const string mongoEntityIdName = "Id";

                pack.AddClassMapConvention("Mongo ID Guid Generator", map =>
                {
                    if (map.ClassType.BaseType == null ||
                        map.ClassType.BaseType.IsInterface ||
                        map.ClassType.BaseType.GetProperty(mongoEntityIdName) == null)
                        if (mongoEntityType.IsAssignableFrom(map.ClassType))
                        {
                            map.MapIdProperty(mongoEntityIdName)
                                .SetIdGenerator(MongoIdGeneratorComb.Instance)
                                .SetSerializer(new GuidSerializer(BsonType.String));

                            map.SetIgnoreExtraElements(true);
                        }
                });

                pack.AddMemberMapConvention("Ignore Default Values", m => m.SetIgnoreIfDefault(!m.MemberType.GetTypeInfo().IsEnum));

                ConventionRegistry.Register("Custom Conventions", pack, _ => true);
            }
        }
    }
}