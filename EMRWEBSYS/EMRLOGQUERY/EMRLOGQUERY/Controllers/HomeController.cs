using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EMRLOGQUERY.Controllers.FunCode;
using System.Net;
using EMRLOGQUERY.Models;
using System.Web.Http;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EMRLOGQUERY.OPENEMRVIEW;
using EMRLOGQUERY.Filters;
using System.Net.Sockets;
using FemhSecurityLibrary;
using MRLOGQUERY.Controllers.FunCode;

namespace EMRLOGQUERY.Controllers
{    
    
 //   [System.Web.Mvc.Authorize]
    public class HomeController : Controller
    {

        // //[RoleAuthorize("Admin")]
        public ActionResult Index()
        {
            try            
            {               
             return View();               
            }
            catch (Exception ex) {
                return View("~/Views/Shared/Error.cshtml");
            }
        }


        // //[RoleAuthorize("Admin")]
        public ActionResult EMR()
        {
            //   return PartialView("_easyuilayout");
            return PartialView();
        }

        // //[RoleAuthorize("Admin")]
        public ActionResult Audit()
        {
           
            return PartialView();
        }



         //[RoleAuthorize("Admin")]
        public ActionResult Summary()
        {
            return PartialView();
        }

         //[RoleAuthorize("Admin")]
        public ActionResult Fake()
        {
            return PartialView();
        }


         //[RoleAuthorize("Admin")]
        public ActionResult ReleaseEMR()
        {
            try
            {
                return View();

            }
            catch (Exception ex)
            {
                return View("~/Views/Shared/Error.cshtml");
            }
        }

        [System.Web.Mvc.AllowAnonymous]
        public ActionResult HIS()
        {
            try
            {
                if (Request.QueryString["V"] != null)
                {
                    return RedirectToAction("HIS", "component", new { V = Request.QueryString["V"].ToString() });
                }
                else if (Request.QueryString["Radtoken"] != null)
                {
                     string Radtoken=  Request.QueryString["Radtoken"].ToString();

               
                    fun fun = new fun();

                    if (Radtoken != string.Empty)
                    {
                        Radtoken = Radtoken.Replace("+", "%2B");
                    }
                    Radtoken = HttpUtility.UrlDecode(Radtoken);
                    string PATNO = FemhSecurityLibrary.SecurityLibrary.VerifyTicket(Radtoken, "MRECORD");

                     string V = PATNO + "&" + ""+"&"+"SYSTEM"+ "&RADMR08002";
                      V = fun.EncryptDES(V, DateTime.Now.ToString("yyyyMMdd"));
                    //住院醫囑WEB版
                    return RedirectToAction("HIS", "component", new { V = V });
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("~/Views/Shared/Error.cshtml");
            }
        }


        [RoleAuthorize("User")]
        public ActionResult BorrowEmr()
        {
            try
            {              
                return View();
            }
            catch (Exception ex)
            {

                ViewData["Message"] = ex.Message;
                return View("~/Views/Shared/Error.cshtml");
            }
        }


        private string ReplaceSignImage(string html, string replaceUrl)
        {
            html = html.Replace("https://emrap.femh.org.tw/JTEMRWebservice/xslt/YV.jpg", replaceUrl);
            html = html.Replace("https://emrap.femh.org.tw/JTEMRWebservice/xslt/NV.jpg", replaceUrl);
            html = html.Replace("https://emrap.femh.org.tw/JTEMRWebservice/xslt/Y.jpg", replaceUrl);
            html = html.Replace("https://emrap.femh.org.tw/JTEMRWebservice/xslt/N.jpg", replaceUrl);
            html = html.Replace("https://emrap.femh.org.tw/JTEMRWebservice/xslt/OrgSign.jpg", replaceUrl);

            return html;
        }
        public async Task<ActionResult> CompareDifference(string[] info)
        {
            String mRet = "";
            fun fun = new fun();

            if (info.Length >= 2)
            {
                string HtmlString1 = "";
                string HtmlString2 = "";

                bool USE_MOCK_COMPARE = true;

                if (USE_MOCK_COMPARE)
                {
                    // ===== 假資料 =====
                    HtmlString1 = @"
                <html>
                    <body>
                        <h3>心電圖報告</h3>
                        <p>病人姓名：王小明</p>
                        <p>檢查時間：2026/03/19 08:30</p>
                        <p>結論：正常</p>
                    </body>
                </html>";

                    HtmlString2 = @"
                <html>
                    <body>
                        <h3>心電圖報告</h3>
                        <p>病人姓名：王小明</p>
                        <p>檢查時間：2026/03/19 08:30</p>
                        <p>結論：疑似心律不整</p>
                    </body>
                </html>";
                }
                else
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    OPENEMRVIEW.Service1SoapClient OPENEMRVIEW = new OPENEMRVIEW.Service1SoapClient();

                    var response_1 = await OPENEMRVIEW.VIEW20230403Async(info[0]);
                    HtmlString1 = response_1.Body.VIEW20230403Result.ToString();
                    HtmlString1 = fun.DecryptDES(HtmlString1, DateTime.Now.ToString("yyyyMMdd"));

                    var response_2 = await OPENEMRVIEW.VIEW20230403Async(info[1]);
                    HtmlString2 = response_2.Body.VIEW20230403Result.ToString();
                    HtmlString2 = fun.DecryptDES(HtmlString2, DateTime.Now.ToString("yyyyMMdd"));
                }

                // ===== 共用處理 =====
                if (info.Length > 2 && info[2] == "26")
                {
                    string replaceUrl = "https://intra5.femh.org.tw/FemhEMRSYS/images/NOPRINT.png";

                    HtmlString1 = ReplaceSignImage(HtmlString1, replaceUrl);
                    HtmlString2 = ReplaceSignImage(HtmlString2, replaceUrl);
                }

                // 移除 script
                HtmlString1 = Regex.Replace(HtmlString1, @"<script.*?>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                HtmlString2 = Regex.Replace(HtmlString2, @"<script.*?>.*?</script>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                // diff
                HtmlDiff diffHelper = new HtmlDiff(HtmlString1, HtmlString2);
                string Difference = diffHelper.Build();

                EMRViewModel model = new EMRViewModel
                {
                    HistoryWEB1 = HtmlString1,
                    HistoryWEB2 = HtmlString2,
                    CompareWEB = Difference
                };

                return Json(model);
            }

            return Content(mRet);
        }


    }


}