namespace VibeConnect.Shared.Models
{
    /// <summary>
    ///
    /// </summary>
    public abstract class PagedResultBase
    {
        /// <summary>
        ///
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int FirstRowOnPage => (CurrentPage - 1) * PageSize + 1;

        /// <summary>
        ///
        /// </summary>
        public int LastRowOnPage => Math.Min(CurrentPage * PageSize, RowCount);
    }
}