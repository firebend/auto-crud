using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;

/// <summary>
/// Memoizes the return of a given factory function.
/// </summary>
public interface IMemoizer<T>
{
    /// <summary>
    /// Given a function, if its already been ran, use its result; otherwise, run the function and cache the result.
    /// </summary>
    /// <param name="key">
    ///     A friendly unique key to cache the function by.
    /// </param>
    /// <param name="factory">
    ///     The factory to create the result.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T">
    /// The result type.
    /// </typeparam>
    /// <returns>
    /// A result of a function that should have only ran once.
    /// </returns>
    Task<T> MemoizeAsync(string key, Func<Task<T>> factory, CancellationToken cancellationToken);
}
