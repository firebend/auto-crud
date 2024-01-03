using System;
using System.Threading.Tasks;
using Firebend.AutoCrud.Mongo.Interfaces;
using MongoDB.Driver;

namespace Firebend.AutoCrud.Mongo.Abstractions.Client
{
    public class MongoRetryService : IMongoRetryService
    {
        public async Task<TReturn> RetryErrorAsync<TReturn>(Func<Task<TReturn>> method, int maxTries)
        {
            var tries = 0;
            double delay = 100;

            
            while (true)
            {
                try
                {
                    return await method();
                }
                catch (Exception ex)
                {
                    tries++;

                    if (!ShouldRetry(ex) || tries >= maxTries)
                    {
                        throw;
                    }

                    if (tries != 1)
                    {
                        delay = Math.Ceiling(Math.Pow(delay, 1.1));
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(delay));
                }
            }
        }

        // don't retry MongoBulkWriteException or duplicate key exceptions
        private bool ShouldRetry(Exception exception) =>
            exception is MongoException
            && exception is not MongoBulkWriteException
            && !exception.Message.Contains("duplicate key");
    }
}
