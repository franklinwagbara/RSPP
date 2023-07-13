using RSPP.Helpers;
using RSPP.Models.DTOs;
using RSPP.Services.Interfaces;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using RSPP.UnitOfWorks.Interfaces;
using System.Linq;
using RSPP.Models.DTOs.Remita;
using RSPP.Configurations;
using RSPP.Helper;
using RSPP.Models.DB;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace RSPP.Services
{
    public class PaymentService : IPaymentService
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRemitaPaymentService _remitaPaymentService;

        //refactor these soon
        private readonly WorkFlowHelper _workFlowHelper;
        private readonly RSPPdbContext _rsppDbContext;
        private readonly HelperController _helperController;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IRemitaPaymentService remitaPaymentService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _remitaPaymentService = remitaPaymentService;

            //refactor these soon
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _rsppDbContext = new RSPPdbContext();
            _workFlowHelper = new WorkFlowHelper(_rsppDbContext);
            _helperController = new HelperController(_rsppDbContext, _configuration, _httpContextAccessor);
        }
        /// <summary>
        /// Checks the payment status of an application
        /// </summary>
        /// <param name="applicationId">application id</param>
        /// <returns>An object representing the payment status of the application</returns>
        public Task<RemitaTransactionResponse> CheckPaymentStatusAsync(string applicationId)
        {

            var response = new RemitaTransactionResponse(false, $"{AppMessages.APPLICATION} {AppMessages.NOT_EXIST}");
            if (string.IsNullOrWhiteSpace(applicationId) || !Regex.IsMatch(applicationId, AppMessages.APPLICATION_ID_PATTERN))
                return Task.FromResult(response);

            try
            {

                var paymentDetails = _unitOfWork.PaymentLogRepository
                    .Get(pay => pay.ApplicationId == applicationId, null, "", null, null).FirstOrDefault();
                if (paymentDetails == null)
                {
                    response.ResultMessage = $"{AppMessages.PAYMENT} {AppMessages.NOT_EXIST}";
                    return Task.FromResult(response);
                }

                response = _remitaPaymentService.CheckTransactionStatusAsync(paymentDetails.Rrreference).Result;
                if (response.Success)
                {
                    var applicationDetails = _unitOfWork.ApplicationRequestFormRepository
                        .Get(app => app.ApplicationId == applicationId, null, "", null, null).FirstOrDefault();

                    if (response.ResultMessage == $"{AppMessages.TRANSACTION} {AppMessages.PAID}")
                    {
                        paymentDetails.Status = AppMessages.AUTH;
                        paymentDetails.TxnMessage = response.RemitaResponse.message;
                        paymentDetails.TransactionId = response.RemitaResponse.status;
                        paymentDetails.TransactionDate = Convert.ToDateTime(response.RemitaResponse.transactiontime);
                        var updateWorkFlow = _workFlowHelper.processAction(applicationId, "GenerateRRR", applicationDetails.CompanyEmail, "Remita Retrieval Reference Generated");
                    }
                    else
                    {
                        paymentDetails.Status = AppMessages.INIT;
                    }
                    _unitOfWork.Complete();
                }
                response.TransactionAmount = paymentDetails.TxnAmount.ToString();
                response.CompanyName = _unitOfWork.ApplicationRequestFormRepository.GetApplicationCompanyName(applicationId);
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                response.ResultMessage = $"{AppMessages.PAYMENT} {AppMessages.STATUS_CHECK} - {AppMessages.UNABLE_TO_COMPLETE}";
                return Task.FromResult(response);
            }

        }
    }
}
