using System;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Core.Models.Entities;
using Firebend.AutoCrud.Web.Sample.Extensions;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace Firebend.AutoCrud.Web.Sample.ValidationServices
{
    public class PersonValidationService : IEntityValidationService<Guid, EfPerson, V1>
    {
        public Task<ModelStateResult<EfPerson>> ValidateAsync(EfPerson original, EfPerson entity, JsonPatchDocument<EfPerson> patch, CancellationToken cancellationToken)
        {
            if (entity.LastName?.Equals("Fail") ?? false)
            {
                var error = new ModelStateResult<EfPerson>();
                error.AddError(nameof(entity.LastName), "your last name cannot be fail.");
                return Task.FromResult(error);
            }

            return Task.FromResult(ModelStateResult.Success(entity));
        }
    }
}
