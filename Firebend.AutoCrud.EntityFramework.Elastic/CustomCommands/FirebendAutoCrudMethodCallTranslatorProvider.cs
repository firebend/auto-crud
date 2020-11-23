using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;

namespace Firebend.AutoCrud.EntityFramework.Elastic.CustomCommands
{
    public class FirebendAutoCrudMethodCallTranslatorProvider : RelationalMethodCallTranslatorProvider
    {
        public FirebendAutoCrudMethodCallTranslatorProvider(RelationalMethodCallTranslatorProviderDependencies dependencies) : base(dependencies)
        {
            var expressionFactory = dependencies.SqlExpressionFactory;

            this.AddTranslators(new List<IMethodCallTranslator>
            {
                new FullTextContainsAnyMethodCallTranslator(expressionFactory)
            });
        }
    }
}
