using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Abstractions.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.EntityFramework.Sample.Models;

namespace Firebend.AutoCrud.EntityFramework.Sample
{
    public interface IPersonReadRepository : IEntityReadService<Guid, Person>;

    public class PersonReadRepository : EntityFrameworkEntityReadService<Guid, Person>, IPersonReadRepository
    {
        public PersonReadRepository(IEntityFrameworkQueryClient<Guid, Person> readClient,
            ISessionTransactionManager transactionManager) : base(readClient, transactionManager)
        {
        }
    }
}
