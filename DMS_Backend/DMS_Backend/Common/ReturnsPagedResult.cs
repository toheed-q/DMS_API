using DMS_Backend.Contracts;

namespace DMS_Backend.Common
{
    /// <summary>
    /// A page of returns plus the aggregate total for the whole filtered set —
    /// so a salesman's "Total Returned" is one call, not two.
    /// </summary>
    public class ReturnsPagedResult : PagedResult<ReturnDto>
    {
        public decimal TotalReturned { get; init; }
    }
}
