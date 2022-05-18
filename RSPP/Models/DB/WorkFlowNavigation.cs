using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class WorkFlowNavigation
    {
        public int WorkFlowId { get; set; }
        public string Action { get; set; }
        public string ActionRole { get; set; }
        public short CurrentStageId { get; set; }
        public short NextStateId { get; set; }
        public string TargetRole { get; set; }
    }
}
