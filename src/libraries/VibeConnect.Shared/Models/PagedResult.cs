namespace VibeConnect.Shared.Models
{
    public class ApiPagedResult<T> : PagedResultBase where T : class
    {
        /// <summary>
        ///
        /// </summary>
        public IList<T> Results { get; set; }

        /// <summary>
        ///
        /// </summary>
        public ApiPagedResult()
        {
            Results = new List<T>();
        }
    }
}