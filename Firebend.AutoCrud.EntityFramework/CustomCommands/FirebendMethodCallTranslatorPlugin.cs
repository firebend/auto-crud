using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;

namespace Firebend.AutoCrud.EntityFramework.CustomCommands;

public sealed class FirebendMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
{
    public FirebendMethodCallTranslatorPlugin(ISqlExpressionFactory expressionFactory)
    {
        Translators = new IMethodCallTranslator[]
        {
            new FullTextContainsAnyMethodCallTranslator(expressionFactory),
            new JsonObjectContainsMethodCallTranslator(expressionFactory)
        };
    }

    public IEnumerable<IMethodCallTranslator> Translators { get; }
}
