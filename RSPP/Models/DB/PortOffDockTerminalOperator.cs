using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class PortOffDockTerminalOperator
    {
        public int PortOffTerminalOperatorId { get; set; }
        public string LineOfBusiness { get; set; }
        public string NameOfTerminal { get; set; }
        public string LocationOfTerminal { get; set; }
        public string StatusOfTerminal { get; set; }
        public string CargoType { get; set; }
        public string AnyOtherInfo { get; set; }
        public string ApplicationId { get; set; }
    }
}
