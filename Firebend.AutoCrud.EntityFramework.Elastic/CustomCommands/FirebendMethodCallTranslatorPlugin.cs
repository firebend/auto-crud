using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;

namespace Firebend.AutoCrud.EntityFramework.Elastic.CustomCommands
{
    public sealed class FirebendMethodCallTranslatorPlugin : IMethodCallTranslatorPlugin
    {
        public FirebendMethodCallTranslatorPlugin(ISqlExpressionFactory expressionFactory)
        {
            Translators = new[] { new FullTextContainsAnyMethodCallTranslator(expressionFactory), };
        }

        public IEnumerable<IMethodCallTranslator> Translators { get; }
    }
}
