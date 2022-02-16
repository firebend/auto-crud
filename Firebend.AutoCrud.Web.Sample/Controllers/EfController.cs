using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Models.Searching;
using Firebend.AutoCrud.Web.Sample.DbContexts;
using Firebend.AutoCrud.Web.Sample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firebend.AutoCrud.Web.Sample.Controllers;

[ApiController]
[Route("/api/v1/ef/people-regular")]
public class EfController : ControllerBase
{
    private readonly PersonDbContext _context;

    public EfController(PersonDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> PeopleAsync(CancellationToken cancellationToken)
    {
        await using (_context)
        {

            var count = await _context.Set<EfPerson>().LongCountAsync(cancellationToken);
            var records = await _context.Set<EfPerson>().AsNoTracking().Take(10).ToListAsync(cancellationToken);

            var response = new EntityPagedResponse<EfPerson> { Data = records, CurrentPage = 1, TotalRecords = count, CurrentPageSize = 10 };
            return Ok(response);
        }
    }
}
