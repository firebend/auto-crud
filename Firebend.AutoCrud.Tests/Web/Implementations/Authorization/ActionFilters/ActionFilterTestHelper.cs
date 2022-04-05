using System;
using Firebend.AutoCrud.Core.Interfaces.Models;

namespace Firebend.AutoCrud.Tests.Web.Implementations.Authorization.ActionFilters;

public static class ActionFilterTestHelper
{
    public class TestEntity : IEntity<Guid>
    {
        public string WhoAreYou { get; set; }
        public bool AreYouHavingFun { get; set; }
        public DateTime LastTimeYouHadFun { get; set; }
        public Guid Id { get; set; }
    }
}
