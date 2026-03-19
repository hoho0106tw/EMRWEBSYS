using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using EMRLOGQUERY.Controllers.FunCode;
using Newtonsoft.Json;
using EMRLOGQUERY.Models;

namespace EMRLOGQUERY.Controllers
{
  //   [System.Web.Mvc.Authorize]
    public class AuditController : Controller
    {
        // GET: Audit

        public ActionResult PrintDetail(string ID)
        {
            fun fun = new fun();
            List<AuditPrintDetailModels> AuditPrintDetailModels = new List<AuditPrintDetailModels>();

            if (ID != null){
                ID = fun.DecryptDES(ID, DateTime.Now.ToString("yyyyMMdd"));

                if (ID.Contains("@")) {
            
                    string[] TempID = ID.Split('@');
                    if (TempID.Length == 3) { 

                    string PrintDate = TempID[0]; string PrintTime = TempID[1]; string patno = TempID[2];


                        if (PrintDate != null && PrintTime != null && patno != null)
                        {
                            ConnectDB ConnectDB = new ConnectDB();
                            getSQL getSQL = new getSQL();
                            DataTable dt = new DataTable();
                            PrintTime = PrintTime.Substring(0, 2);
                            dt = ConnectDB.SQL_SERVER_GETDT(getSQL.PrintDetail(PrintDate, PrintDate, PrintTime, patno));


                            foreach (DataRow row in dt.Rows)
                            {
                                AuditPrintDetailModels model = new AuditPrintDetailModels
                                {
                                    PrintDate = row["sDate"].ToString(),
                                    PrintTime = row["sTime"].ToString(),
                                    adminID = row["sAdminID"].ToString(),
                                    adminName = row["sName"].ToString(),
                                    adminAction = row["SCode"].ToString(),
                                    PrintDetail = row["sRemark"].ToString(),
                                };

                                AuditPrintDetailModels.Add(model);

                            }
                        }
                    }
                }
            }
            return View(AuditPrintDetailModels);

        }


    }
}