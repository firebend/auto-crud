using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Web.Interfaces;
using Firebend.AutoCrud.Web.Sample.Models;

namespace Firebend.AutoCrud.Web.Sample.Extensions;

public class PersonViewModelMapper : IReadViewModelMapper<Guid, EfPerson, V1, GetPersonViewModel>
{
    public Task<EfPerson> FromAsync(GetPersonViewModel model, CancellationToken cancellationToken) =>
        null;

    public Task<IEnumerable<EfPerson>> FromAsync(IEnumerable<GetPersonViewModel> model,
        CancellationToken cancellationToken) => null;

    public Task<GetPersonViewModel> ToAsync(EfPerson entity, CancellationToken cancellationToken) =>
        Task.FromResult(new GetPersonViewModel(entity));

    public Task<IEnumerable<GetPersonViewModel>> ToAsync(IEnumerable<EfPerson> entity,
        CancellationToken cancellationToken) =>
        Task.FromResult(entity.Select(x => new GetPersonViewModel(x)));
}
