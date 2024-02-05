using System.Threading;
using Firebend.AutoCrud.EntityFramework.Elastic.Interfaces;
using Firebend.AutoCrud.EntityFramework.Elastic.Models;

namespace Firebend.AutoCrud.EntityFramework.Elastic.Implementations;

internal record ConnectionStringContext(
    string Key,
    IShardManager ShardManager,
    ShardMapMangerConfiguration ShardMapMangerConfiguration,
    IShardNameProvider ShardNameProvider,
    CancellationToken CancellationToken);
