using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Core.Abstractions.Services;

public abstract class AbstractEntitySearchService<TEntity, TSearch> : BaseDisposable
    where TSearch : IEntitySearchRequest
{
    private Type _myType;
    private MethodInfo _yieldDateFiltersMethodInfo;
    private MethodInfo _yieldActiveFiltersMethodInfo;

    private Type MyType => _myType ??= typeof(AbstractEntitySearchService<TEntity, TSearch>);

    private MethodInfo YieldDateFiltersMethodInfo => _yieldDateFiltersMethodInfo ??= MyType
        .GetMethod(nameof(YieldDateFilters), BindingFlags.Static | BindingFlags.NonPublic)
        ?.MakeGenericMethod(typeof(TEntity));

    private MethodInfo YieldActiveFiltersMethodInfo => _yieldActiveFiltersMethodInfo ??= MyType
        .GetMethod(nameof(YieldActiveFilters), BindingFlags.Static | BindingFlags.NonPublic)
        ?.MakeGenericMethod(typeof(TEntity));


    protected IEnumerable<Expression<Func<TEntity, bool>>> GetSearchExpressions(TSearch search)
    {
        if (search is IActiveEntitySearchRequest { IsDeleted: not null })
        {
            foreach (var func in InvokeFiltering(YieldActiveFiltersMethodInfo, search))
            {
                yield return func;
            }
        }

        if (search is IModifiedEntitySearchRequest)
        {
            foreach (var func in InvokeFiltering(YieldDateFiltersMethodInfo, search))
            {
                yield return func;
            }
        }
    }

    private IEnumerable<Expression<Func<TEntity, bool>>> InvokeFiltering(MethodInfo method, TSearch search)
    {
        var invoked = method.Invoke(this, [search]);
        var enumerable = invoked as IEnumerable<Expression<Func<TEntity, bool>>>;
        return enumerable;
    }

    private static IEnumerable<Expression<Func<T, bool>>> YieldDateFilters<T>(
        IModifiedEntitySearchRequest modifiedEntitySearchRequest)
        where T : TEntity, IModifiedEntity
    {
        if (modifiedEntitySearchRequest.CreatedStartDate.HasValue)
        {
            yield return x => x.CreatedDate >= modifiedEntitySearchRequest.CreatedStartDate;
        }

        if (modifiedEntitySearchRequest.CreatedEndDate.HasValue)
        {
            yield return x => x.CreatedDate <= modifiedEntitySearchRequest.CreatedEndDate;
        }

        if (modifiedEntitySearchRequest.ModifiedStartDate.HasValue)
        {
            yield return x => x.ModifiedDate >= modifiedEntitySearchRequest.ModifiedStartDate;
        }

        if (modifiedEntitySearchRequest.ModifiedEndDate.HasValue)
        {
            yield return x => x.ModifiedDate <= modifiedEntitySearchRequest.ModifiedEndDate;
        }
    }

    private static IEnumerable<Expression<Func<T, bool>>> YieldActiveFilters<T>(
        IActiveEntitySearchRequest activeEntitySearchRequest)
        where T : IActiveEntity, TEntity
    {
        var isDeleted = activeEntitySearchRequest.IsDeleted.GetValueOrDefault();

        yield return isDeleted switch
        {
            true => x => x.IsDeleted,
            false => x => !x.IsDeleted
        };
    }
}
