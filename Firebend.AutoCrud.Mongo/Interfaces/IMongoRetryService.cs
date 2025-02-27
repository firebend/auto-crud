using System;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Mongo.Interfaces;

public interface IMongoRetryService
{
    public Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries);
}
