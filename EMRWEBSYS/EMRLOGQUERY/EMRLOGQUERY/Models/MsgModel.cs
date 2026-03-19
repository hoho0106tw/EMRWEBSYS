using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EMRLOGQUERY.Models
{
    public class MsgModel
    {        public string MSG { get; set; }
    }

    public class PrintEMRRequest
    {
        public List<string> selectedItems { get; set; }
        public string Reason { get; set; }
        public string remarksContent { get; set; }
    }
}