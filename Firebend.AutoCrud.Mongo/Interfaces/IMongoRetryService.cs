using System;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Mongo.Interfaces
{
    public interface IMongoRetryService
    {
        Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries);
    }
}
