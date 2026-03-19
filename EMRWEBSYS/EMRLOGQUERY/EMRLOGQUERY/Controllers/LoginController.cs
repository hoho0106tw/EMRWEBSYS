using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using EMRLOGQUERY.ViewModels;
using System.Net;
using EMRLOGQUERY.Controllers.FunCode;
using System.Data;
using System.Security.Policy;
using MRLOGQUERY.Controllers.FunCode;


namespace EMRLOGQUERY.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            Session.Clear();
            Session.Abandon();
            
            Response.Headers.Set("X-Frame-Options", "allow-from https://intra5.femh.org.tw https://intra5.femh.org.tw:65534/");
            ModelLogin data = new ModelLogin();
            return View("Login", data);     
        }



        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult LoginCheck(ModelLogin data)
        {
            var contentType = Request.ContentType;

            if (contentType != "application/x-www-form-urlencoded; charset=UTF-8")
            {
                // 如果不是合法的表單請求，可以返回錯誤或重定向
                return new HttpStatusCodeResult(403, "Forbidden");
            }


            String message = "";
            if (ModelState.IsValid)
            {
                getSQL getSQL = new getSQL();
                ConnectDB ConnectDB = new ConnectDB();

                String url = "";
                FormsAuthentication.SignOut();
                String userid = data.userid.Trim();
                String userpw = data.password.Trim();

                try
                {


                    Boolean isPass = false;
                    if (userid != "femhadmin")
                    {
                        String strHostName = string.Empty;
                        IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
                        IPAddress[] addr = ipEntry.AddressList;
                        string IP = "";
                        for (int i = 0; i < addr.Length; i++)
                        {
                            string[] temp = addr[i].ToString().Split('.');

                            if (temp.Length >= 4)
                            {
                                IP = addr[i].ToString();
                                break;
                            }
                        }


                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        LoginService.LoginWebServiceSoapClient client = new LoginService.LoginWebServiceSoapClient();
                        string login_ID = userid.ToString().TrimEnd().Replace("'", "").Replace("-", "").Replace("=", "").Replace(" ", "");
                        //　帶入以下引數　１．"FEMHEMRSYS"  寫死登入代號　　2.使用者帳號　3.使用者密碼　4.與ｃｌｉｅｎｔ　ｉｐ　
                        var result = client.VerifyPassword("FEMHEMRSYS", new LoginService.LoginVerifyPassword { Type = LoginService.LoginType.His, UserId = login_ID, Password = userpw.ToString().Trim().Replace("'", "''"), Source = IP });

                        isPass = result.Success;
                    }
                    else
                    {
                        DataTable isVerifyPwdDT = new DataTable();
                        isVerifyPwdDT = ConnectDB.SQL_SERVER_GETDT3(getSQL.VerifyPwd(), userpw);
                        if (isVerifyPwdDT.Rows.Count > 0)
                        {
                            isPass = true;
                        }
                    }

                    if (isPass == true)
                    {
                        DataTable DT = new DataTable();
                        DT = ConnectDB.SQL_SERVER_GETDT2(getSQL.GETEMRROLE(userid), userid);

                        if (DT.Rows.Count > 0)
                        {
                            url = Url.Action("Index", "Home");
                        }
                        else
                        {
                            message = "登入失敗，\n請檢查你的帳號密碼是否正確。";                           
                        }
                    }
                    else
                    {
                        message = "登入失敗，\n請檢查你的帳號密碼是否正確。";                      
                    }

                }
                catch (Exception ex)
                {
                    message = "登入失敗，\n目前登入系統有問題:" + ex.Message;
                  
                }


             

                if (url != "")
                {
                    Session["UserRole"] = "Admin";
                    FormsAuthentication.SetAuthCookie(userid, false);
                    return Json(new { url = url });
                }
                else
                {
                    return Json(new { message = message });
                }

            }

            else
            {
                message= "登入失敗，\n你的帳號密碼不合法。";
                return Json(new { message = message });
            }
        }







        [HttpPost]
        [ValidateAntiForgeryToken]
   
        public ActionResult TokenCheck(ModelLogin data, string token)
        {
            string message = "";
            bool isPass = false;

            try
            {
                // 1. 驗證 Content-Type
                if (!Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
                {
                    return new HttpStatusCodeResult(403, "Forbidden");
                }

                // 2. 驗證 Token
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { message = "登入失敗，\n不合法登入。" });
                }

                if (!ModelState.IsValid)
                {
                    return Json(new { message = "登入失敗，\n你的帳號密碼不合法。" });
                }

                string userid = data.userid?.Trim();
                string userpw = data.password?.Trim();

                if (string.IsNullOrWhiteSpace(userid) || string.IsNullOrWhiteSpace(userpw))
                {
                    return Json(new { message = "登入失敗，\n請輸入有效的帳號密碼。" });
                }

                // 3. 取得用戶 IP
                string userIP = Dns.GetHostEntry(Dns.GetHostName())
                                   .AddressList
                                   .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
                                   .ToString() ?? "0.0.0.0";

                // 4. 呼叫登入 WebService
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var client = new LoginService.LoginWebServiceSoapClient();
                var result = client.VerifyPassword("FEMHEMRSYS", new LoginService.LoginVerifyPassword
                {
                    Type = LoginService.LoginType.His,
                    UserId = userid.Replace("'", "").Replace("-", "").Replace("=", "").Replace(" ", ""),
                    Password = userpw.Replace("'", "''"),
                    Source = userIP
                });

                isPass = result.Success;

                // 5. 解密 Token
                Token decrypted = TokenCrypt.Decrypt(token);
                string tokenUserId = decrypted.UserId;
                string submissionNum = decrypted.SubmissionNum;
                string REASION = decrypted.REASION;

                // 6. 驗證 Token 與帳號是否一致
                if (!isPass || tokenUserId != userid)
                {
                    return Json(new { message = "登入失敗，\n不合法登入。" });
                }

                // 7. 查詢資料庫
                var getSQL = new getSQL();
                var ConnectDB = new ConnectDB();
                string sql = getSQL.GETBorrowEMRTblSQL(submissionNum, userid);
                DataTable dt = ConnectDB.GETDATABASEDATA(sql);

                if (dt.Rows.Count == 0)
                {
                    return Json(new { message = "登入失敗，\n目前可能開放時效已到期，\n或者你沒有任何申請的病歷資料。" });
                }
                else
                {
                    Session["isHile"] = dt.Rows[0]["ISHIDEPROFILE"].ToString();
                }



                    // 8. 成功登入
                TempData["userid"] = userid;
                TempData["SubmissionNum"] = submissionNum;
                Session["BrowerReason"] = "[借閱系統] 簽呈編號:" + submissionNum + ",借閱理由:" + REASION; // 這裡設置 BrowerReason
                Session["UserRole"] = "User";
                FormsAuthentication.SetAuthCookie(userid, false);


                string redirectUrl = Url.Action("BorrowEmr", "Home");
                return Json(new { url = redirectUrl });
            }
            catch (Exception ex)
            {
                message = "登入失敗，\n目前登入系統有問題: " + ex.Message;
                return Json(new { message });
            }
        }





    }
}