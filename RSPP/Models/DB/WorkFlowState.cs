using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class WorkFlowState
    {
        public short StateId { get; set; }
        public string StateName { get; set; }
        public string StateType { get; set; }
        public string Progress { get; set; }
    }
}
