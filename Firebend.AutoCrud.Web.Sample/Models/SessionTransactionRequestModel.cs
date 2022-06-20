using System;

namespace Firebend.AutoCrud.Web.Sample.Models;

public class SessionTransactionRequestModel
{
    public Guid EfPersonId { get; set; }
    public Guid MongoPersonId { get; set; }
}
