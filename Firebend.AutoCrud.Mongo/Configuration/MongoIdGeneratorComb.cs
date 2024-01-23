using System;
using Firebend.AutoCrud.Core.Ids;
using MongoDB.Bson.Serialization;

namespace Firebend.AutoCrud.Mongo.Configuration;

public class MongoIdGeneratorComb : IIdGenerator
{
    public static readonly MongoIdGeneratorComb Instance = new();

    public object GenerateId(object container, object document)
        => CombGuid.New();

    public bool IsEmpty(object id) => id == default || (Guid)id == Guid.Empty;
}
