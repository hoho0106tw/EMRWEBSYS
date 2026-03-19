using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EMRLOGQUERY.ViewModels
{

    public class ModelLogin
    {
        [Required(ErrorMessage = "(必須有資料)")]
        public String userid { get; set; }

        [Required(ErrorMessage = "(必須有資料)")]
        [DataType(DataType.Password)]
        public String password { get; set; }
        public String random { get; set; }
    }



}
