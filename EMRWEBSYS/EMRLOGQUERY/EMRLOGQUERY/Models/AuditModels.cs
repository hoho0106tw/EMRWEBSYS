using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EMRLOGQUERY.Models
{


 

    public class AuditModels
    {  
        public string documentID { get; set; }
        public string patno { get; set; }
        public string patname { get; set; }
        public string OrderDate { get; set; }
        public string SignerEmployeeID { get; set; }
        public string SignerName { get; set; }
        public string SubmissionDate { get; set; }
        public string SubmissionTime { get; set; }
        public string SystemType { get; set; }
        public string DocumentType { get; set; }
        public string Status { get; set; }
        public string PrintDate { get; set; }
        public string PrintTime { get; set; }
        public string PrintReason { get; set; }
        public string ID { get; set; }
    }



    public class AuditPrintDetailModels
    {
        public string PrintDate { get; set; }
        public string PrintTime { get; set; }
        public string adminID { get; set; }
        public string adminName { get; set; }
        public string adminAction { get; set; }
        public string PrintDetail { get; set; }

    }






  }