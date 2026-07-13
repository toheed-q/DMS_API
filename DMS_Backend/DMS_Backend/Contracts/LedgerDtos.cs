namespace DMS_Backend.Contracts
{
    /// <summary>
    /// The customer's ledger KPIs. Computed with SQL aggregation (never by loading
    /// rows), so it stays fast with tens of thousands of entries.
    /// </summary>
    public class LedgerSummaryDto
    {
        public int ShopId { get; set; }
        public string? ShopName { get; set; }

        public decimal TotalBills { get; set; }
        public decimal TotalReturns { get; set; }
        /// <summary>TotalBills - TotalReturns.</summary>
        public decimal NetBills { get; set; }
        public decimal TotalReceived { get; set; }

        /// <summary>What the customer still owes: max(0, NetBills - TotalReceived).</summary>
        public decimal Outstanding { get; set; }

        /// <summary>Overpayment: max(0, TotalReceived - NetBills). Informational only —
        /// it is never auto-allocated to bills.</summary>
        public decimal Excess { get; set; }
    }

    public class LedgerEntryDto
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public int? BillId { get; set; }
        /// <summary>Bill items, or the manual description for manual entries.</summary>
        public string? Items { get; set; }
        public decimal BillTotal { get; set; }
        public decimal ReturnAmount { get; set; }
        public bool IsReturn { get; set; }
        public bool IsManualEntry { get; set; }
        public string? ReturnProducts { get; set; }
        public string? SalesmanName { get; set; }
    }

    /// <summary>Filters for the ledger entry list (inherits page/pageSize/search).</summary>
    public class LedgerEntryQuery : PaginationQuery
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
