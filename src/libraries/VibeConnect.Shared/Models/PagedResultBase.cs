namespace VibeConnect.Shared.Models
{
    /// <summary>
    ///
    /// </summary>
    public abstract class PagedResultBase
    {
        public int LowerBound { get; set; }
        public int UpperBound { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}