using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EMRLOGQUERY.Controllers.FunCode;
using System.Data;
using Newtonsoft.Json;
using EMRLOGQUERY.Models;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web.Http;

using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Drawing;
using System.Web.UI.WebControls;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using Microsoft.Ajax.Utilities;
using PdfSharp.Pdf.Content;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.ServiceModel.MsmqIntegration;
using System.Web.Security;
using Antlr.Runtime.Misc;
using System.Web.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Web.Services;
using FemhSecurityLibrary;
using System.Net.Sockets;




namespace EMRLOGQUERY.Controllers
{
  //   [System.Web.Mvc.Authorize]
    public class componentController : Controller
    {
        // ===== Mock 開關 =====
        private const bool USE_MOCK = true;

        [System.Web.Mvc.AllowAnonymous]
        public async Task<ActionResult> HIS(string V)
        {
            try
            {
                if (USE_MOCK)
                {
                    // 模擬 HIS 帶入資料
                    string TextPatNO = "1234567";
                    string sRequestDocParent = "DOC-001";
                    string USERID = "TEST001";

                    DataTable dt = BuildMockHisData(TextPatNO, sRequestDocParent);

                    List<object> TreeModeM = new List<object>();
                    List<object> subTreeModel = new List<object>();
                    int count = 0;

                    foreach (DataRow dr in dt.Rows)
                    {
                        string EMR_SysKey = dr["EMR_SysKey"].ToString();
                        string GroupNO = dr["GroupNO"].ToString();
                        string sStatus = dr["sStatus"].ToString();
                        string ddate = dr["ddate"].ToString();
                        string sContent = dr["sContent"].ToString();
                        string sRequestServerFileName = dr["sRequestServerFileName"].ToString();
                        string sRequestPrint = dr["sRequestPrint"].ToString();

                        string FileType = "";
                        if (sRequestServerFileName.ToUpper().Contains("XML"))
                            FileType = "XML";
                        else if (sRequestServerFileName.ToUpper().Contains("PDF"))
                            FileType = "PDF";

                        int iGroupNO = Convert.ToInt32(GroupNO);

                        if (iGroupNO != 1)
                        {
                            subTreeModel.Add(TreeAddRecursive_subnode(
                                EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate, sContent,
                                subTreeModel, 26, FileType, sRequestPrint));
                        }
                        else
                        {
                            subTreeModel.Add(TreeAddRecursive_subnode(
                                EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate, sContent,
                                subTreeModel, 26, FileType, sRequestPrint));

                            TreeModeM.Add(TreeAddRecursive_parentsnode(
                                EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate, count,
                                subTreeModel, 26, FileType, sRequestPrint));

                            subTreeModel = new List<object>();
                            count++;
                        }
                    }

                    TreeModelList treeModelList = new TreeModelList()
                    {
                        node = TreeModeM
                    };

                    FormsAuthentication.SetAuthCookie(USERID, false);
                    return PartialView("~/Views/Home/component/HIS.cshtml", treeModelList);
                }

                return View("~/Views/Shared/Error.cshtml");
            }
            catch (Exception ex)
            {
                ViewData["Message"] = ex.Message;
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        public string combSystem()
        {
            if (USE_MOCK)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("TEXT");
                dt.Columns.Add("VALUE");

                dt.Rows.Add("全部", "ALL㊣");
                dt.Rows.Add("病房系統", "WARD㊣WARD");
                dt.Rows.Add("放射系統", "RAD㊣RAD");
                dt.Rows.Add("檢驗系統", "LAB㊣LAB");

                return JsonConvert.SerializeObject(dt);
            }

            return "[]";
        }

        public string combDoctype(string SYSCODE)
        {
            if (USE_MOCK)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("TEXT");
                dt.Columns.Add("VALUE");

                dt.Rows.Add("全部", "ALL");
                dt.Rows.Add("住院病程紀錄", "DOC001");
                dt.Rows.Add("出院病摘", "DOC002");
                dt.Rows.Add("放射報告", "RAD001");
                dt.Rows.Add("檢驗報告", "LAB001");

                return JsonConvert.SerializeObject(dt);
            }

            return "[]";
        }

        [System.Web.Mvc.NonAction]
        string FormatDate(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return "未指定日期";

            string raw = dateObj.ToString()?.Trim();

            if (raw.Length == 8 && long.TryParse(raw, out _))
            {
                try
                {
                    var dt = DateTime.ParseExact(raw, "yyyyMMdd", null);
                    return dt.ToString("yyyy-MM-dd");
                }
                catch
                {
                    return "格式錯誤";
                }
            }

            if (DateTime.TryParse(raw, out DateTime normalDate))
                return normalDate.ToString("yyyy-MM-dd");

            return "格式錯誤";
        }




        [System.Web.Mvc.NonAction]
        private TreeModel BuildDocTree(SystemInfo systemInfo, DataTable dt)
        {
            var docGroups = dt.AsEnumerable()
                .Where(row => row.Field<string>("sSystem") == systemInfo.sysCode)
                .GroupBy(row => row.Field<string>("sCode"))
                .Select(group => new docInfo
                {
                    docCode = group.Key,
                    docConent = group.First().Field<string>("sContent")
                })
                .ToList();

            List<TreeModel> subDocTreeModels = new List<TreeModel>();

            foreach (var docInfo in docGroups)
            {
                var docRows = dt.AsEnumerable()
                    .Where(row => row.Field<string>("sSystem") == systemInfo.sysCode &&
                                  row.Field<string>("sCode") == docInfo.docCode);

                var dateGroups = docRows
                    .GroupBy(row => row.Field<string>("sRequestDocDate"))
                    .OrderBy(g => g.Key)
                    .ToList();

                var dateNodes = new List<TreeModel>();

                foreach (var dateGroup in dateGroups)
                {
                    var date = dateGroup.Key;
                    var nodesForDate = BuildNodeTree(systemInfo, docInfo, dateGroup.CopyToDataTable());

                    dateNodes.Add(new TreeModel
                    {
                        id = $"{docInfo.docCode}_{date}",
                        icon = "https://intra5.femh.org.tw/FemhEMRSYS/images/folderH.png",
                        iconsize = 20,
                        label = "醫囑日期:" + FormatDate(date),
                        expanded = false,
                        selected = false,
                        items = nodesForDate.Cast<object>().ToList(),
                        sStatus = "",
                        ddate = date,
                        sRequestDocParent = docInfo.docCode,
                        sContent = "",
                        FileType = ""
                    });
                }

                subDocTreeModels.Add(new TreeModel
                {
                    id = docInfo.docCode,
                    icon = "https://intra5.femh.org.tw/FemhEMRSYS/images/sys.png",
                    iconsize = 25,
                    label = docInfo.docConent,
                    expanded = false,
                    selected = false,
                    items = dateNodes.Cast<object>().ToList(),
                    sStatus = "",
                    ddate = "",
                    sRequestDocParent = "",
                    sContent = docInfo.docConent,
                    FileType = ""
                });
            }

            return new TreeModel
            {
                id = systemInfo.sysCode,
                icon = "https://intra5.femh.org.tw/FemhEMRSYS/images/database2.png",
                iconsize = 25,
                label = systemInfo.SysConent,
                expanded = false,
                selected = false,
                items = subDocTreeModels.Cast<object>().ToList(),
                sStatus = "",
                ddate = "",
                sRequestDocParent = "",
                sContent = systemInfo.SysConent,
                FileType = ""
            };
        }

        [System.Web.Mvc.NonAction]
        private List<TreeModel> BuildNodeTree(SystemInfo systemInfo, docInfo docInfo, DataTable dt)
        {
            var nodeGroups = dt.AsEnumerable()
                .Where(row => row.Field<string>("sSystem") == systemInfo.sysCode &&
                              row.Field<string>("sRequestDocType") == docInfo.docCode)
                .Select(row => new node
                {
                    EMR_Syskey = row.Field<long>("EMR_Syskey").ToString(),
                    ddate = row.Field<string>("ddate"),
                    sStatus = row.Field<string>("sStatus"),
                    sRequestServerFileName = row.Field<string>("sRequestServerFileName"),
                    sContent = row.Field<string>("sContent"),
                    sRequestDocParent = row.Field<string>("sRequestDocParent"),
                    sRequestPrint = row.Field<string>("sRequestPrint"),
                })
                .ToList();

            return nodeGroups.Select(node =>
            {
                string Sicon = "";
                string SignState = "";

                if (node.sStatus == "VSSign")
                {
                    Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/contract.png";
                    SignState = " (已簽章) ";
                }
                else if (node.sStatus == "OrgSign")
                {
                    Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/stamp.png";
                    SignState = " (醫院簽) ";
                }
                else if (node.sStatus == "NotSign")
                {
                    Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/vote.png";
                    SignState = " (未簽章) ";
                }

                string FileType = "";
                if ((node.sRequestServerFileName ?? "").ToUpper().Contains("XML"))
                    FileType = "XML";
                else if ((node.sRequestServerFileName ?? "").ToUpper().Contains("PDF"))
                    FileType = "PDF";

                string sRequestPrint = "";
                if (node.sRequestPrint == "Y")
                    sRequestPrint = "<span style='color:red;'> 列印 </span>";

                string slabel = node.sContent + " (" + TimeFormat(node.ddate) + ") " + SignState + sRequestPrint + FileType;

                return new TreeModel
                {
                    id = node.EMR_Syskey, // mock 不加密
                    icon = Sicon,
                    iconsize = 25,
                    label = slabel,
                    expanded = false,
                    selected = false,
                    items = null,
                    sStatus = SignState,
                    ddate = node.ddate,
                    sRequestDocParent = node.sRequestDocParent,
                    sContent = node.sContent,
                    FileType = FileType
                };
            }).ToList();
        }

        public ActionResult AuditDataGridView()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult btnSearch_Audit(string[] info)
        {
            MsgModel msgModel = new MsgModel();

            if (USE_MOCK)
            {
                List<AuditModels> auditModelsList = new List<AuditModels>
                {
                    new AuditModels
                    {
                        documentID = "10001",
                        patno = "P1234567",
                        patname = "王小明",
                        OrderDate = "20250901",
                        SignerEmployeeID = "D0001",
                        SignerName = "陳醫師",
                        SubmissionDate = "20250901",
                        SubmissionTime = "091500",
                        SystemType = "WARD",
                        DocumentType = "DOC001",
                        Status = "Y",
                        PrintDate = "20250902",
                        PrintTime = "101500",
                        PrintReason = "保險申請",
                        ID = "MOCK-AUDIT-1"
                    },
                    new AuditModels
                    {
                        documentID = "10002",
                        patno = "P1234567",
                        patname = "王小明",
                        OrderDate = "20250902",
                        SignerEmployeeID = "D0002",
                        SignerName = "林醫師",
                        SubmissionDate = "20250902",
                        SubmissionTime = "103000",
                        SystemType = "RAD",
                        DocumentType = "RAD001",
                        Status = "N",
                        PrintDate = "20250903",
                        PrintTime = "111500",
                        PrintReason = "轉診",
                        ID = "MOCK-AUDIT-2"
                    }
                };

                return Json(auditModelsList);
            }

            msgModel.MSG = "未啟用 Mock";
            return Json(msgModel);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult btnSearch(string[] info)
        {
            List<object> TreeModeM = new List<object>();
            MsgModel MsgModel = new MsgModel();

            //if (info == null || info.Length <= 1)
            //{
            //    MsgModel.MSG = "參數有問題。";
            //    return Json(MsgModel);
            //}

            //if (string.IsNullOrWhiteSpace(info[0]) || string.IsNullOrWhiteSpace(info[1]))
            //{
            //    MsgModel.MSG = "醫囑日期不能為空白。";
            //    return Json(MsgModel);
            //}

            //if (Convert.ToDateTime(info[1]) < Convert.ToDateTime(info[0]))
            //{
            //    MsgModel.MSG = "醫囑終止日期不能小於開始日期。";
            //    return Json(MsgModel);
            //}

            //if (string.IsNullOrWhiteSpace(info[4]))
            //{
            //    MsgModel.MSG = "病歷號必須要填寫。";
            //    return Json(MsgModel);
            //}

            if (USE_MOCK)
            {
                string tabSelect = info.Length > 5 ? info[5].Trim() : "";
                DataTable dt = BuildMockSearchData2();

                if (dt.Rows.Count == 0)
                {
                    MsgModel.MSG = "你查詢的資料，筆數0筆。";
                    return Json(MsgModel);
                }

                if (tabSelect == "收發作業")
                {
                    var systemGroups = dt.AsEnumerable()
                        .GroupBy(row => row.Field<string>("sSystem"))
                        .Select(group => new SystemInfo
                        {
                            sysCode = group.Key,
                            SysConent = group.First().Field<string>("sSystem2")
                        })
                        .ToList();

                    List<TreeModel> treeModels = new List<TreeModel>();

                    foreach (var systemInfo in systemGroups)
                    {
                        var docTree = BuildDocTree(systemInfo, dt);
                        treeModels.Add(docTree);
                    }

                    return Json(treeModels);
                }
                else
                {
                    List<object> subTreeModel = new List<object>();
                    int count = 0;

                    foreach (DataRow dr in dt.Rows)
                    {
                        string EMR_SysKey = dr["EMR_SysKey"].ToString();
                        string GroupNO = dr["GroupNO"].ToString();
                        string sStatus = dr["sStatus"].ToString();
                        string ddate = dr["ddate"].ToString();
                        string sContent = dr["sContent"].ToString();
                        string sRequestDocParent = dr["sRequestDocParent"].ToString();
                        string sRequestServerFileName = dr["sRequestServerFileName"].ToString();
                        string sRequestPrint = dr["sRequestPrint"].ToString();

                        string FileType = "";
                        if (sRequestServerFileName.ToUpper().Contains("XML"))
                            FileType = "XML";
                        else if (sRequestServerFileName.ToUpper().Contains("PDF"))
                            FileType = "PDF";

                        int iGroupNO = Convert.ToInt32(GroupNO);

                        if (iGroupNO != 1)
                        {
                            subTreeModel.Add(TreeAddRecursive_subnode(
                                EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate,
                                sContent, subTreeModel, 25, FileType, sRequestPrint));
                        }
                        else
                        {
                            subTreeModel.Add(TreeAddRecursive_subnode(
                                EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate,
                                sContent, subTreeModel, 25, FileType, sRequestPrint));

                            TreeModeM.Add(TreeAddRecursive_parentsnode(
                                EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate, count,
                                subTreeModel, 25, FileType, sRequestPrint));

                            subTreeModel = new List<object>();
                            count++;
                        }
                    }
                }

                return Json(TreeModeM);
            }

            MsgModel.MSG = "未啟用 Mock";
            return Json(MsgModel);
        }





        [System.Web.Mvc.NonAction]
        private DataTable BuildMockSearchData2()
        {
            DataTable dt = new DataTable();

            // 給 foreach 舊樹狀邏輯用
            dt.Columns.Add("EMR_SysKey", typeof(string));
            dt.Columns.Add("GroupNO", typeof(string));
            dt.Columns.Add("sStatus", typeof(string));
            dt.Columns.Add("ddate", typeof(string));
            dt.Columns.Add("sContent", typeof(string));
            dt.Columns.Add("sRequestDocParent", typeof(string));
            dt.Columns.Add("sRequestServerFileName", typeof(string));
            dt.Columns.Add("sRequestPrint", typeof(string));

            // 給 BuildDocTree / BuildNodeTree 用
            dt.Columns.Add("EMR_Syskey", typeof(long));   // 注意這個欄位名稱和上面不同
            dt.Columns.Add("sSystem", typeof(string));
            dt.Columns.Add("sSystem2", typeof(string));
            dt.Columns.Add("sCode", typeof(string));
            dt.Columns.Add("sRequestDocType", typeof(string));
            dt.Columns.Add("sRequestDocDate", typeof(string));

            // ===== 第一組：心電圖 =====
            // 子節點先
            dt.Rows.Add(
                "10002",                  // EMR_SysKey
                "2",                      // GroupNO
                "NotSign",                // sStatus
                "20260319084500",         // ddate
                "心電圖原始檔",            // sContent
                "10001",                  // sRequestDocParent
                "ECG_20260319_02.xml",    // sRequestServerFileName
                "N",                      // sRequestPrint

                10002L,                   // EMR_Syskey
                "SYS01",                  // sSystem
                "收發系統",                // sSystem2
                "ECG",                    // sCode
                "ECG",                    // sRequestDocType
                "20260319"                // sRequestDocDate
            );

            // 父節點最後
            dt.Rows.Add(
                "10001",
                "1",
                "VSSign",
                "20260319083000",
                "心電圖報告",
                "ROOT",
                "ECG_20260319_01.pdf",
                "Y",

                10001L,
                "SYS01",
                "收發系統",
                "ECG",
                "ECG",
                "20260319"
            );

            // ===== 第二組：心臟超音波 =====
            dt.Rows.Add(
                "10004",
                "2",
                "VSSign",
                "20260318101500",
                "心臟超音波影像",
                "10003",
                "ECHO_20260318_02.xml",
                "N",

                10004L,
                "SYS01",
                "收發系統",
                "ECHO",
                "ECHO",
                "20260318"
            );

            dt.Rows.Add(
                "10003",
                "1",
                "OrgSign",
                "20260318100000",
                "心臟超音波報告",
                "ROOT",
                "ECHO_20260318_01.pdf",
                "Y",

                10003L,
                "SYS01",
                "收發系統",
                "ECHO",
                "ECHO",
                "20260318"
            );

            // ===== 第三組：檢驗報告 =====
            dt.Rows.Add(
                "10006",
                "2",
                "OrgSign",
                "20260317114500",
                "檢驗明細",
                "10005",
                "LAB_20260317_02.xml",
                "N",

                10006L,
                "SYS01",
                "收發系統",
                "LAB",
                "LAB",
                "20260317"
            );

            dt.Rows.Add(
                "10005",
                "1",
                "NotSign",
                "20260317113000",
                "檢驗報告",
                "ROOT",
                "LAB_20260317_01.pdf",
                "Y",

                10005L,
                "SYS01",
                "收發系統",
                "LAB",
                "LAB",
                "20260317"
            );

            // ===== 第四組：影像系統 / X光 =====
            dt.Rows.Add(
                "20002",
                "2",
                "NotSign",
                "20260316093000",
                "胸部X光原始檔",
                "20001",
                "XRAY_20260316_02.xml",
                "N",

                20002L,
                "SYS02",
                "影像系統",
                "XRAY",
                "XRAY",
                "20260316"
            );

            dt.Rows.Add(
                "20001",
                "1",
                "VSSign",
                "20260316090000",
                "胸部X光報告",
                "ROOT",
                "XRAY_20260316_01.pdf",
                "Y",

                20001L,
                "SYS02",
                "影像系統",
                "XRAY",
                "XRAY",
                "20260316"
            );

            // ===== 第五組：影像系統 / CT =====
            dt.Rows.Add(
                "20004",
                "2",
                "VSSign",
                "20260315082000",
                "電腦斷層影像",
                "20003",
                "CT_20260315_02.xml",
                "N",

                20004L,
                "SYS02",
                "影像系統",
                "CT",
                "CT",
                "20260315"
            );

            dt.Rows.Add(
                "20003",
                "1",
                "OrgSign",
                "20260315080000",
                "電腦斷層報告",
                "ROOT",
                "CT_20260315_01.pdf",
                "Y",

                20003L,
                "SYS02",
                "影像系統",
                "CT",
                "CT",
                "20260315"
            );

            return dt;
        }









        private TreeModel TreeAddRecursive_subnode(
            string EMR_SysKey,
            string sRequestDocParent,
            int iGroupNO,
            string sStatus,
            string ddate,
            string sContent,
            List<object> TreeModel,
            int iconsize,
            string FileType,
            string sRequestPrint)
        {
            TreeModel TreeModelD = new TreeModel();

            string SignState = "";
            string Sicon = "";

            if (sStatus == "VSSign")
            {
                Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/contract.png";
                SignState = " (已簽章) ";
            }
            else if (sStatus == "OrgSign")
            {
                Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/stamp.png";
                SignState = " (醫院簽) ";
            }
            else if (sStatus == "NotSign")
            {
                Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/vote.png";
                SignState = " (未簽章) ";
            }

            if (sRequestPrint == "Y")
                sRequestPrint = "<span style='color:red;'> 列印 </span>";
            else
                sRequestPrint = "";

            string slabel = sContent + " (" + TimeFormat(ddate) + ") " + SignState + sRequestPrint + FileType;

            TreeModelD = new TreeModel
            {
                id = EMR_SysKey, // mock 不加密
                icon = Sicon,
                iconsize = iconsize,
                label = slabel,
                expanded = false,
                selected = false,
                items = null,
                sStatus = sStatus,
                ddate = ddate,
                sRequestDocParent = sRequestDocParent,
                sContent = sContent,
                FileType = FileType
            };

            return TreeModelD;
        }

        private TreeModel TreeAddRecursive_parentsnode(
            string EMR_SysKey,
            string sRequestDocParent,
            int iGroupNO,
            string sStatus,
            string ddate,
            int count,
            List<object> subTreeModel,
            int iconsize,
            string FileType,
            string sRequestPrint)
        {
            TreeModel TreeModel = new TreeModel();
            TreeModel TreeModel_Node0 = (TreeModel)subTreeModel[0];

            string SignState = "";
            string Sicon = "";

            if (TreeModel_Node0.sStatus == "VSSign")
            {
                Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/medicalrecord.png";
                SignState = " (已簽章) ";
            }
            else if (TreeModel_Node0.sStatus == "OrgSign")
            {
                Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/folderH.png";
                SignState = " (醫院簽) ";
            }
            else if (TreeModel_Node0.sStatus == "NotSign")
            {
                Sicon = "https://intra5.femh.org.tw/FemhEMRSYS/images/2278843.png";
                SignState = " (未簽章) ";
            }

            bool isexpanded = false;
            string isCOUNTDisplay = "";

            if (subTreeModel.Count > 1)
                isCOUNTDisplay = "<span style='color: Blue;'>(" + subTreeModel.Count.ToString() + ")</span>";

            if (sRequestPrint == "Y")
                sRequestPrint = "<span style='color:red;'> 列印 </span>";
            else
                sRequestPrint = "";

            string slabel = TreeModel_Node0.sContent + " (" + TimeFormat(TreeModel_Node0.ddate) + ") " +
                            SignState + isCOUNTDisplay + sRequestPrint + FileType;

            bool selected = false;

            if (subTreeModel.Count == 1)
            {
                TreeModel = new TreeModel
                {
                    id = TreeModel_Node0.id,
                    icon = Sicon,
                    iconsize = iconsize,
                    label = slabel,
                    expanded = isexpanded,
                    selected = selected,
                    items = null,
                    FileType = FileType
                };
            }
            else
            {
                TreeModel = new TreeModel
                {
                    id = TreeModel_Node0.id,
                    icon = Sicon,
                    iconsize = iconsize,
                    label = slabel,
                    expanded = isexpanded,
                    selected = selected,
                    items = subTreeModel,
                    FileType = FileType
                };
            }

            return TreeModel;
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> EMRView(string[] info)
        {
            EMRViewModel model = new EMRViewModel();

            if (USE_MOCK)
            {
                string emrKey = (info != null && info.Length > 0) ? info[0] : "10001";
                string fileType = (info != null && info.Length > 4) ? info[4] : "XML";

                model.HTML = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Mock EMR View</title>
    <style>
        body {{
            font-family: Arial, Microsoft JhengHei, sans-serif;
            margin: 20px;
            background: #f8f9fa;
        }}
        .card {{
            background: white;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0,0,0,.08);
        }}
        .title {{
            font-size: 24px;
            font-weight: bold;
            color: #1f4e79;
            margin-bottom: 10px;
        }}
        .row {{
            margin: 8px 0;
            font-size: 16px;
        }}
        .badge {{
            display: inline-block;
            padding: 4px 10px;
            background: #e8f0fe;
            color: #174ea6;
            border-radius: 16px;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <div class='title'>病歷內容預覽（Mock）</div>
        <div class='row'><strong>EMR_SysKey：</strong>{emrKey}</div>
        <div class='row'><strong>檔案類型：</strong><span class='badge'>{fileType}</span></div>
        <div class='row'><strong>病人姓名：</strong>王小明</div>
        <div class='row'><strong>病歷號：</strong>P1234567</div>
        <div class='row'><strong>醫囑日期：</strong>2025-09-01</div>
        <div class='row'><strong>內容摘要：</strong>這是一份用來測試前端畫面與樹狀選單的虛擬病歷內容。</div>
        <hr />
        <div class='row'>主訴：發燒、咳嗽 3 天。</div>
        <div class='row'>評估：上呼吸道感染。</div>
        <div class='row'>處置：給予退燒藥與衛教後返家休養。</div>
    </div>
</body>
</html>";
            }

            return new JsonResult
            {
                Data = model,
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [System.Web.Mvc.HttpPost]
        public async Task<ActionResult> PrintEMR(PrintEMRRequest request)
        {
            if (USE_MOCK)
            {
                // 先不真的合併 PDF，直接回成功訊息即可
                return Json(new
                {
                    success = true,
                    message = "Mock 模式：已模擬列印 " + (request?.selectedItems?.Count ?? 0) + " 筆"
                });
            }

            return Json(new { success = false, message = "未啟用 Mock" });
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult OrgSignAction(List<string> selectedItems)
        {
            if (USE_MOCK)
            {
                return Json(new
                {
                    success = true,
                    message = "Mock 模式：備簽成功",
                    count = selectedItems?.Count ?? 0
                });
            }

            return Json(new { success = false, message = "未啟用 Mock" });
        }

        [System.Web.Mvc.HttpPost]
        public JsonResult cancelPrint(List<string> selectedItems)
        {
            if (USE_MOCK)
            {
                return Json(new
                {
                    success = true,
                    message = "Mock 模式：取消列印成功",
                    count = selectedItems?.Count ?? 0
                });
            }

            return Json(new { success = false, message = "未啟用 Mock" });
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult BorrowEmrSearch()
        {
            if (USE_MOCK)
            {
                DataTable dt = BuildMockSearchData();
                List<object> TreeModeM = new List<object>();
                MsgModel MsgModel = new MsgModel();
                string isPrint = "Y";

                List<object> subTreeModel = new List<object>();
                int count = 0;

                foreach (DataRow dr in dt.Rows)
                {
                    string EMR_SysKey = dr["EMR_SysKey"].ToString();
                    string GroupNO = dr["GroupNO"].ToString();
                    string sStatus = dr["sStatus"].ToString();
                    string ddate = dr["ddate"].ToString();
                    string sContent = dr["sContent"].ToString();
                    string sRequestDocParent = dr["sRequestDocParent"].ToString();
                    string sRequestServerFileName = dr["sRequestServerFileName"].ToString();
                    string sRequestPrint = dr["sRequestPrint"].ToString();

                    string FileType = "";
                    if (sRequestServerFileName.ToUpper().Contains("XML"))
                        FileType = "XML";
                    else if (sRequestServerFileName.ToUpper().Contains("PDF"))
                        FileType = "PDF";

                    int iGroupNO = Convert.ToInt32(GroupNO);
                    if (iGroupNO != 1)
                    {
                        subTreeModel.Add(TreeAddRecursive_subnode(
                            EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate,
                            sContent, subTreeModel, 25, FileType, sRequestPrint));
                    }
                    else
                    {
                        subTreeModel.Add(TreeAddRecursive_subnode(
                            EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate,
                            sContent, subTreeModel, 25, FileType, sRequestPrint));
                        TreeModeM.Add(TreeAddRecursive_parentsnode(
                            EMR_SysKey, sRequestDocParent, iGroupNO, sStatus, ddate,
                            count, subTreeModel, 25, FileType, sRequestPrint));

                        subTreeModel = new List<object>();
                        count++;
                    }
                }

                return Json(new
                {
                    data = TreeModeM,
                    msg = MsgModel,
                    isPrint = isPrint
                });
            }

            return Json(new
            {
                data = new List<object>(),
                msg = new MsgModel { MSG = "未啟用 Mock" },
                isPrint = "N"
            });
        }

        // =========================
        // Mock Data
        // =========================

        private DataTable BuildMockHisData(string patNo, string docParent)
        {
            DataTable dt = CreateBaseTreeTable();

            dt.Rows.Add("10001", "1", "VSSign", "20250901083000", "住院病程紀錄", "note1.xml", docParent, "N",
                "WARD", "病房系統", "DOC001", "DOC001", "住院病程紀錄", "20250901");

            dt.Rows.Add("10002", "2", "OrgSign", "20250901093000", "住院病程紀錄-附件", "note2.pdf", docParent, "Y",
                "WARD", "病房系統", "DOC001", "DOC001", "住院病程紀錄", "20250901");

            dt.Rows.Add("10003", "1", "NotSign", "20250902103000", "放射報告", "rad1.pdf", "RAD001", "N",
                "RAD", "放射系統", "RAD001", "RAD001", "放射報告", "20250902");

            return dt;
        }

        private DataTable BuildMockSearchData()
        {
            DataTable dt = CreateBaseTreeTable();

            dt.Rows.Add("10001", "1", "VSSign", "20250901083000", "住院病程紀錄", "note1.xml", "DOC001", "N",
                "WARD", "病房系統", "DOC001", "DOC001", "住院病程紀錄", "20250901");

            dt.Rows.Add("10002", "2", "OrgSign", "20250901093000", "住院病程紀錄-附件", "note2.pdf", "DOC001", "Y",
                "WARD", "病房系統", "DOC001", "DOC001", "住院病程紀錄", "20250901");

            dt.Rows.Add("10003", "1", "NotSign", "20250902103000", "出院病摘", "discharge.xml", "DOC002", "N",
                "WARD", "病房系統", "DOC002", "DOC002", "出院病摘", "20250902");

            dt.Rows.Add("10004", "1", "OrgSign", "20250903110000", "放射報告", "rad1.pdf", "RAD001", "N",
                "RAD", "放射系統", "RAD001", "RAD001", "放射報告", "20250903");

            dt.Rows.Add("10005", "1", "VSSign", "20250904120000", "檢驗報告", "lab1.xml", "LAB001", "Y",
                "LAB", "檢驗系統", "LAB001", "LAB001", "檢驗報告", "20250904");

            return dt;
        }

        private DataTable CreateBaseTreeTable()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("EMR_SysKey", typeof(string));
            dt.Columns.Add("EMR_Syskey", typeof(long)); // BuildNodeTree 用
            dt.Columns.Add("GroupNO", typeof(string));
            dt.Columns.Add("sStatus", typeof(string));
            dt.Columns.Add("ddate", typeof(string));
            dt.Columns.Add("sContent", typeof(string));
            dt.Columns.Add("sRequestServerFileName", typeof(string));
            dt.Columns.Add("sRequestDocParent", typeof(string));
            dt.Columns.Add("sRequestPrint", typeof(string));

            dt.Columns.Add("sSystem", typeof(string));
            dt.Columns.Add("sSystem2", typeof(string));
            dt.Columns.Add("sRequestDocType", typeof(string));
            dt.Columns.Add("sCode", typeof(string));
            dt.Columns.Add("sRequestDocDate", typeof(string));

            return dt;
        }

        private string TimeFormat(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "";

            if (raw.Length >= 14 &&
                DateTime.TryParseExact(raw.Substring(0, 14), "yyyyMMddHHmmss", null,
                System.Globalization.DateTimeStyles.None, out DateTime dt))
            {
                return dt.ToString("yyyy-MM-dd HH:mm");
            }

            if (raw.Length >= 8 &&
                DateTime.TryParseExact(raw.Substring(0, 8), "yyyyMMdd", null,
                System.Globalization.DateTimeStyles.None, out DateTime dt2))
            {
                return dt2.ToString("yyyy-MM-dd");
            }

            return raw;
        }
    }
}