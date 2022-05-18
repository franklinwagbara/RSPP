using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class ActionHistory
    {
        public long ActionId { get; set; }
        public string ApplicationId { get; set; }
        public short? CurrentStageId { get; set; }
        public string Action { get; set; }
        public DateTime? ActionDate { get; set; }
        public string TriggeredBy { get; set; }
        public string TriggeredByRole { get; set; }
        public string Message { get; set; }
        public string TargetedTo { get; set; }
        public string TargetedToRole { get; set; }
        public short? NextStateId { get; set; }
    }
}
