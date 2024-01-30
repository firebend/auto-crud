using System;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.EntityFramework.Interfaces;
using Firebend.AutoCrud.EntityFramework.Sample.Models;
using Firebend.AutoCrud.EntityFramework.Services;

namespace Firebend.AutoCrud.EntityFramework.Sample;

public class PersonReadRepository : EntityFrameworkEntityReadService<Guid, Person>, IPersonReadRepository
{
    public PersonReadRepository(IEntityFrameworkQueryClient<Guid, Person> readClient,
        ISessionTransactionManager transactionManager) : base(readClient, transactionManager)
    {
    }
}
