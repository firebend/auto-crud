using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Mongo.Abstractions.Entities;
using Firebend.AutoCrud.Mongo.Interfaces;
using Firebend.AutoCrud.Mongo.Sample.Models;

namespace Firebend.AutoCrud.Mongo.Sample
{
    public interface IPersonReadRepository : IEntityReadService<Guid, Person>
    {
    }

    public class PersonReadRepository : MongoEntityReadService<Guid, Person>, IPersonReadRepository
    {
        public PersonReadRepository(IMongoReadClient<Guid, Person> readClient) : base(readClient)
        {
        }
    }
}
