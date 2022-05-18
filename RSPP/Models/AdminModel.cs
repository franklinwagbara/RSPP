using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSPP.Models
{
    public class AdminModel
    {
    }


    public class ResponseWrapper
    {
        public ResponseWrapper() { }
        public bool status { get; set; }
        public string value { get; set; }
        public string nextStageId { get; set; }
        public string nextStateType { get; set; }
        public string receivedBy { get; set; }
        public string receivedByRole { get; set; }
        public string receivedLocation { get; set; }
    }


    public class Terminal
    {
        public string TerminalName { get; set; }
        public string TerminalLocation { get; set; }
    }



    public class StaffDeskModel
    {
        public List<StaffDesk> StaffDeskList { get; set; }
    }

    public class StaffDesk
    {
        public string StaffEmail { get; set; }
        public string StaffName { get; set; }
        public string BranchName { get; set; }
        public string Department { get; set; }
        public string Role { get; set; }
        public int OnDesk { get; set; }
        public string status { get; set; }
    }


    public class StaffJson
    {
        public string userid { get; set; }
        public string name { get; set; }
    }

    public class Staff
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string userId { get; set; }
        public string email { get; set; }
    }



    public class FacilityInfo
    {
        public string CompanyEmail { get; set; }
        public string AgencyName { get; set; }
        public string ApplicationID { get; set; }
        public string FacilityName { get; set; }
        public string CompanyAddress { get; set; }
        public string ApplicationTypeId { get; set; }
        public string AppliedDate { get; set; }
        public string AplicationCodeType { get; set; }
        public int CurrentStageId { get; set; }
        public string LastAssignedUser { get; set; }
        public string GPS { get; set; }

    }




    public class Permitmodel
    {
        public string TelePhoneNumber { get; set; }
        public string Fieldofficeaddress { get; set; }
        public string Signature { get; set; }
        public string PMB { get; set; }
        public string Address { get; set; }
        public string LGA { get; set; }
        public string ApprefNo { get; set; }

        public string CompanyName { get; set; }
        public string State { get; set; }
        public string RegisteredAddress { get; set; }
        public string LocationAddress { get; set; }
        public string IssuedDate { get; set; }
        public string PermitNumber { get; set; }
        public decimal Amountpaid { get; set; }
        public string CompanyIdentity { get; set; }
        public string LicenseNumber { get; set; }
        public decimal Capacity { get; set; }
        public string CapacityToWord { get; set; }
        public string AmountToWord { get; set; }
        public string IssuedDay { get; set; }
        public DateTime DateIssued { get; set; }
        public string IssuedMonth { get; set; }
        public string ExpiryDay { get; set; }
        public string AgencyName { get; set; }
        public string ExpiryMonth { get; set; }
        public string IssuedYear { get; set; }
        public string ExpiryYear { get; set; }
        public DateTime Expiry { get; set; }
        public byte[] QrCode { get; internal set; }



    }



    public class PermitModels
    {
        public List<Permitmodel> permitmodels { get; set; }
    }

    //statuscode":"025","RRR":"320008236824","status":"Payment Reference generated"

    public class RemitaModel
    {
        public string statuscode { get; set; }
        public string RRR { get; set; }
        public string status { get; set; }


    }
    public class ApplicationRatio
    {
        public int Initiated { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Processing { get; set; }
        public int Legacy { get; set; }

    }

    public class PaymentModel
    {

        public string RRReference { get; set; }
        public string Status { get; set; }
        public Nullable<DateTime> TransactionDate { get; set; }
        public string TransactionID { get; set; }
        public Nullable<decimal> TxnAmount { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicationID { get; set; }
        public string ExtraPaymentBy { get; set; }
        public string CompanyUserId { get; set; }
        public decimal Arrears { get; set; }

    }

    public class ApplicationRequestFormModel
    {
        public string ApplicationId { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string LastAssignedUser { get; set; }
        public string ModifyDate { get; set; }
    }



    public class LicenseRatio
    {
        public int totalLicense { get; set; }
        public int Online { get; set; }
        public int Legacy { get; set; }
    }



    public class RatioDash
    {
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int OnDesk { get; set; }
        public int LegacyInProgress { get; set; }
        public int OnlineInProgress { get; set; }
        public int Processed { get; set; }
    }


    public partial class MyApplicationRequestForm
    {
        

        public string ApplicationId { get; set; }
        public string AgencyName { get; set; }
        public DateTime? DateofEstablishment { get; set; }
        public string CompanyAddress { get; set; }
        public string PostalAddress { get; set; }
        public string PhoneNum { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyWebsite { get; set; }
        public int? AgencyId { get; set; }
        public string LicenseReference { get; set; }
        public DateTime? LicenseIssuedDate { get; set; }
        public DateTime? LicenseExpiryDate { get; set; }
        public int? CurrentStageId { get; set; }
        public string LastAssignedUser { get; set; }
        public DateTime? AddedDate { get; set; }
        public string Status { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ApplicationTypeId { get; set; }
        public string PrintedStatus { get; set; }
        public int? LineOfBusinessId { get; set; }
        public string CacregNum { get; set; }
        public string NameOfAssociation { get; set; }
        public DateTime? OtherLicenseExpiryDate { get; set; }
        public string OtherLicense { get; set; }
        public string ServicesProvidedInPort { get; set; }
        public string AnyOtherRelevantInfo { get; set; }
        public string LineOfBusiness { get; set; }
        public string CustomLicenseNum { get; set; }
        public string CrffnRegistrationNum { get; set; }
        public string AnyOtherInfo { get; set; }
        public DateTime? CustomLicenseExpiryDate { get; set; }
        public DateTime? CrffnRegistratonExpiryDate { get; set; }
        public string NameOfTerminal { get; set; }
        public string LocationOfTerminal { get; set; }
        public string StatusOfTerminal { get; set; }
        public string CargoType { get; set; }
        public string VesselLinesRepresentedInNigeria { get; set; }
        public string Category { get; set; }
        public string NepcRegNo { get; set; }
        public string Nparegnum { get; set; }
        public string Nimasaregnum { get; set; }
    }


    public class OutofOfficeDto
    {
        public string Password { get; set; }
        public string Reliever { get; set; }
        public string Relieved { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
    }

    public class PaymentChart
    {
        public Nullable<long> Bargo_Operators { get; set; }
        public Nullable<long> CargoConsolidators_DeConsolidators { get; set; }
        public Nullable<long> Chandling { get; set; }
        public Nullable<long> DryPortOperator { get; set; }
        public Nullable<long> FreightForwarders_ClearingAgents { get; set; }
        public Nullable<long> Haulers_Truckers { get; set; }
        public Nullable<long> ICD { get; set; }
        public Nullable<long> Logististics_Service_Provider { get; set; }
        public Nullable<long> Stevedoring_Warehousing { get; set; }
        public Nullable<long> SeaportTerminalOperator { get; set; }
        public Nullable<long> OffDockTerminalOperator { get; set; }
        public Nullable<long> ShippingAgency { get; set; }
        public Nullable<long> ShippingCompany_Line { get; set; }
        public Nullable<long> ShippersAssociation { get; set; }
        public Nullable<long> CargoSurveyor { get; set; }
        public Nullable<long> IndividualCategory { get; set; }
        public Nullable<long> CorperateCategory { get; set; }
        public Nullable<long> OtherPortServiceProviders { get; set; }

    }

    public class ApplicationReportViewModel
    {
        public string ApplicationID { get; set; }
        public string Status { get; set; }
        public string CompanyEmail { get; set; }
        public string CompanyName { get; set; }
        public DateTime AddedDate { get; set; }
        public string issueddate { get; set; }
        public string expirydate { get; set; }
        public DateTime? expiryDATE { get; set; }
        public DateTime? issuedDATE { get; set; }
        public string ApplicationTypeId { get; set; }
        public string AgencyName { get; set; }
        public string IsLegacy { get; set; }
        public string LineOfBusinessId { get; set; }
    }

    public class UserMasterModel
    {
        public int UserMasterId { get; set; }
        public string UserEmail { get; set; }
        public string CompanyAddress { get; set; }
        public string Password { get; set; }
        public string PhoneNum { get; set; }
        public string CompanyName { get; set; }
        public string UserType { get; set; }
        public string UserRole { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string Status { get; set; }
        public DateTime? LastLogin { get; set; }
        public int? LoginCount { get; set; }
        public string LastComment { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SignatureImage { get; set; }
        public IFormFile myfiles { get; set; }
    }

}
