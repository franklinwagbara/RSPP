using RSPP.Models.DTOs.Remita;
using RSPP.Models;
using System.Threading.Tasks;

namespace RSPP.Services.Interfaces
{
    /// <summary>
    /// Provides interface for remita payment operations
    /// </summary>
    public interface IRemitaPaymentService
    {
        /// <summary>
        /// Checks the status of a payment transaction
        /// </summary>
        /// <param name="rrr">Remita Retrieval Reference</param>
        /// <returns>An object representing the status of the transaction</returns>
        Task<RemitaTransactionResponse> CheckTransactionStatusAsync(string rrr);

        /// <summary>
        /// Generate RRR
        /// </summary>
        /// <param name="rrrRequest">request model to retrieve Remita Retrieval Reference</param>
        /// <returns>An object that determines if RRR was generated</returns>
        Task<RemitaInitiatePaymentResponse> InitiatePaymentAsync(RRRRequestModel rrrRequest);
    }
}
