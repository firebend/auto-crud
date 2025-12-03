using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.Web.Sample.ValidationServices
{
    public class PersonDeleteValidationService : IEntityDeleteValidationService<Guid, EfPerson, V1>
    {
        public Task<ModelStateResult<EfPerson>> ValidateAsync(EfPerson entity, CancellationToken cancellationToken)
        {
            if (entity.FirstName == "Block")
            {
                return Task.FromResult(ModelStateResult<EfPerson>.Error(nameof(EfPerson.FirstName),
                    "Cannot delete a person with the first name 'Block'"));
            }

            return Task.FromResult(ModelStateResult.Success(entity));
        }
    }
}
