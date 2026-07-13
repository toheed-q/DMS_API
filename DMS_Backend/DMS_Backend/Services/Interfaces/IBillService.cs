using DMS_Backend.Common;
using DMS_Backend.Contracts;

namespace DMS_Backend.Services.Interfaces
{
    public interface IBillService
    {
        /// <summary>Creates a bill atomically: items, stock deduction, ledger entry
        /// and (if paid upfront) a customer receiving.</summary>
        Task<Result<BillDto>> CreateAsync(CreateBillRequest request);

        Task<Result<BillDto>> GetByIdAsync(int id);
    }
}
