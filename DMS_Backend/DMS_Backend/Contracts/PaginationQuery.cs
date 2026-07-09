namespace DMS_Backend.Contracts
{
    /// <summary>
    /// Common query-string parameters for paged list endpoints. PageSize is clamped
    /// so a client can never request an unbounded page (protects the DB/API).
    /// </summary>
    public class PaginationQuery
    {
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 20;

        private int _page = 1;
        private int _pageSize = DefaultPageSize;

        /// <summary>1-based page number.</summary>
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
        }

        /// <summary>Optional free-text search term.</summary>
        public string? Search { get; set; }
    }
}
