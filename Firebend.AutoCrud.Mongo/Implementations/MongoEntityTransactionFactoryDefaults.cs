using System;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Implementations;

public static class MongoEntityTransactionFactoryDefaults
{
#pragma warning disable CA2211, IDE1006
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static TransactionOptions TransactionOptions;
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public static ClientSessionOptions SessionOptions;
#pragma warning restore CA2211, IDE1006

    static MongoEntityTransactionFactoryDefaults()
    {
        TransactionOptions = new TransactionOptions(
            ReadConcern.Local,
            readPreference: ReadPreference.Primary,
            writeConcern: WriteConcern.WMajority,
            maxCommitTime: TimeSpan.FromMinutes(5));

        SessionOptions = new ClientSessionOptions
        {
            DefaultTransactionOptions = TransactionOptions,
        };
    }
}
