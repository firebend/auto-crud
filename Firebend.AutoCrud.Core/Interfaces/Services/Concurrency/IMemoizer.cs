using System;
using System.Threading;
using System.Threading.Tasks;

namespace Firebend.AutoCrud.Core.Interfaces.Services.Concurrency;

/// <summary>
///     Memoizes the return of a given factory function.
/// </summary>
public interface IMemoizer
{
    /// <summary>
    ///     Given a function, if its already been ran, use its result; otherwise, run the function and cache the result.
    /// </summary>
    /// <param name="key">
    ///     A friendly unique key to cache the function by.
    /// </param>
    /// <param name="factory">
    ///     The factory to create the result.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A result of a function that should have only ran once.
    /// </returns>
    Task<T> MemoizeAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken);


    /// <summary>
    ///     Given a function, if its already been ran, use its result; otherwise, run the function and cache the result.
    ///     This overload accepts an argument to pass to the factory to prevent closure memory allocations.
    /// </summary>
    /// <param name="key">
    ///     A friendly unique key to cache the function by.
    /// </param>
    /// <param name="factory">
    ///     The factory to create the result.
    /// </param>
    /// <param name="arg">
    ///     The argument to pass to the factory.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TArg">
    ///     The type of argument to pass to the factory.
    /// </typeparam>
    /// <typeparam name="T"></typeparam>
    ///     The type of the result of the task
    /// <returns>
    ///     A result of a function that should have only ran once.
    /// </returns>
    Task<T> MemoizeAsync<T, TArg>(string key, Func<TArg, Task<T>> factory, TArg arg, CancellationToken cancellationToken);

    T Memoize<T>(string key, Func<T> factory);

    T Memoize<T, TArg>(string key, Func<TArg, T> factory, TArg arg);
}
