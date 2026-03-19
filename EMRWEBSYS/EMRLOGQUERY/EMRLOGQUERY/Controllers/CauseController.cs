using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMRLOGQUERY.Controllers
{ 
  // //   [System.Web.Mvc.Authorize]
    public class CauseController : Controller
    {     
        // GET: Cause
        public ActionResult CauseAnalysis()
        {
            return View();
        }
    }
}