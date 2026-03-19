using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EMRLOGQUERY.Models
{
    public  class TreeModel
    {
        public string id {  get; set; }
        public string icon { get; set; }
        public int iconsize { get; set; }
        public string label { get; set; }
        public Boolean expanded { get; set; }
        public Boolean selected { get; set; }
        public List<Object>  items { get; set; }
        public string sStatus { get; set; }
        public string ddate { get; set; }
        public string sRequestDocParent { get; set; }
        public string sContent { get; set; }
        public string FileType { get; set; }
    }


    //public class TreeRootSYSModel
    //{

    //    public string id { get; set; }
    //    public string icon { get; set; }
    //    public int iconsize { get; set; }
    //    public string label { get; set; }
    //    public Boolean expanded { get; set; }
    //    public Boolean selected { get; set; }
    //    public List<Object> items { get; set; }
    //    public string sStatus { get; set; }
    //    public string ddate { get; set; }
    //    public string sRequestDocParent { get; set; }
    //    public string sContent { get; set; }
    //    public string FileType { get; set; }
    //}


    public class TreeModelList
    {
        public List<Object> node = new List<object>();
    }


    public class SystemInfo
    {
        public string sysCode { get; set; }
        public string SysConent { get; set; }  
    }

    public class docInfo
    {
        public string docCode { get; set; }
        public string docConent { get; set; }
    }

    public class node
    {
        public string EMR_Syskey { get; set; }
        public string ddate { get; set; }
        public string sStatus { get; set; }

        public string sContent { get; set; }

        public string sRequestDocParent { get; set; }

        public string sRequestServerFileName { get; set; }


        public string sRequestPrint { get; set; }
    }

}















