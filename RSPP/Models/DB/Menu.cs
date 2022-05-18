using System;
using System.Collections.Generic;

namespace RSPP.Models.DB
{
    public partial class Menu
    {
        public string MenuId { get; set; }
        public string Description { get; set; }
        public string IconName { get; set; }
        public byte? SeqNo { get; set; }
        public string Status { get; set; }
    }
}
