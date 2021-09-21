
namespace Firebend.AutoCrud.Core.Models.Searching
{
    public class EntitySearchRequest
    {
        /// <summary>
        /// The starting page number for this search request.
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// The total number of records per page
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// A list of strings representing an order by clause. example ?orderBy=lastName:desc&orderBy=firstName:desc.
        /// </summary>
        public string[] OrderBy { get; set; }

        /// <summary>
        /// The text string to search by
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// True if a count of total records should be returned; otherwise, false.
        /// </summary>
        public bool? DoCount { get; set; } = true;
    }
}
