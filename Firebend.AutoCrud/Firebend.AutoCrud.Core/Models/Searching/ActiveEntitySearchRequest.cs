namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class ActiveEntitySearchRequest : EntitySearchRequest
    {
        public bool? IsDeleted { get; set; }
    }
}