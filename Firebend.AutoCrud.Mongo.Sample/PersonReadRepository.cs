using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Sample.Models;
using Firebend.AutoCrud.Mongo.Services;

namespace Firebend.AutoCrud.Mongo.Sample;

public class PersonReadRepository : MongoEntityReadService<Guid, Person>, IPersonReadRepository
{
    public PersonReadRepository(IMongoReadClient<Guid, Person> readClient,
        ISessionTransactionManager transactionManager) : base(readClient, transactionManager)
    {
    }
}
