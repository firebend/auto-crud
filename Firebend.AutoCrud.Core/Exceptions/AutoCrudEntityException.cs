using System;
using System.Collections.Generic;
using System.Linq;

namespace Firebend.AutoCrud.Core.Exceptions
{
    public class AutoCrudEntityException : Exception
    {
        public object Entity { get; set; }

        public IEnumerable<KeyValuePair<string,  string>> PropertyErrors { get; set; }

        public AutoCrudEntityException(string message,
            Exception ex,
            object entity = null,
            params (string property, string error)[] errors) : base(message, ex)
        {
            Entity = entity;

            if (errors != null)
            {
                PropertyErrors = errors
                    .Select(e => new KeyValuePair<string, string>(e.property, e.error))
                    .ToList();
            }
        }
    }
}
