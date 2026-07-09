namespace DMS_Backend.Common
{
    /// <summary>
    /// Standard shape for paginated list responses. Reused by every list endpoint
    /// so clients always get the same pagination metadata (DRY).
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
