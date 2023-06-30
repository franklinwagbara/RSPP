using RSPP.Models.DTOs.Remita;
using System.Threading.Tasks;

namespace RSPP.Services.Interfaces
{
    /// <summary>
    /// Provides interface for payment operations
    /// </summary>
    public interface IPaymentService
    {
        Task<RemitaTransactionResponse> CheckPaymentStatusAsync(string applicationId);
        
    }
}
