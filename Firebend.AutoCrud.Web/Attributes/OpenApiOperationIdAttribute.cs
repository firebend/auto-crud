using System;

namespace Firebend.AutoCrud.Web.Attributes;

public class OpenApiOperationIdAttribute : Attribute
{
    public OpenApiOperationIdAttribute(string operationId)
    {
        OperationId = operationId;
    }

    public string OperationId { get; }
}
