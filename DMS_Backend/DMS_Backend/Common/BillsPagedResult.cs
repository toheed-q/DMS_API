using DMS_Backend.Contracts;

namespace DMS_Backend.Common
{
    /// <summary>
    /// A page of bills plus aggregates for the whole filtered set — so the bills
    /// history screen gets its totals in the same call.
    /// </summary>
    public class BillsPagedResult : PagedResult<BillSummaryDto>
    {
        public decimal TotalBilled { get; init; }
        public decimal TotalPaid { get; init; }
    }
}
