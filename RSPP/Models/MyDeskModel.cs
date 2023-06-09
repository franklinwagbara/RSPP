using System;

namespace RSPP.Models
{
    public class MyDeskModel
    {
        public string ApplicationId { get; set; } = default;
        public string ApplicationType { get; set; } = default;
        public string CompanyName { get; set; } = default;
        public string CompanyEmail { get; set; } = default;
        public string CompanyAddress { get; set; } = default;
        public DateTime? AddedDate { get; set; } = default;
    }
}
