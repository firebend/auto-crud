using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Firebend.AutoCrud.ChangeTracking.Extensions;

public static class OperationExtensions
{
    public static List<Operation> ToNonGenericOperations<T>(this IEnumerable<Operation<T>> operations) where T : class =>
        operations?.Select(x => new Operation(x.op, x.path, x.from, x.value)).ToList();
}
