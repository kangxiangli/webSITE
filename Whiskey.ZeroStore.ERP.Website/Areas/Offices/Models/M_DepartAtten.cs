using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models
{
    public class M_DepartAtten
    {
        public string RealName { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

        public DateTime AttenTime { get; set; }

        public string AttenType { get; set; }
    }
}