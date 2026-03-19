using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace EMRLOGQUERY.Controllers.FunCode
{
    public class getSQL
    {

        public string GETsystemSQL()
        {
            string SQL = string.Format(@"select 'ALL㊣' AS scode , 'ALL' AS sContent,'' as sRemark ,'selected' as selected   union 
           select   scode + '㊣' + sRemark as scode,sContent,sRemark ,'' as selected  from [jemrdb].[dbo].[va_reference] 
                        where stype= 'System' and scode not in ( 'ALL','' ) 
                        and sSystem = 'EMR' 
              order by sRemark");

            return SQL;
        }


        public string GETsystemDocType(string SelectedValue)
        {
      

            string SQL = string.Format(@"select scode,sContent,'' as selected from [jemrdb].[dbo].[va_reference]
                        where stype='DocType' ");

            if (SelectedValue.Contains(",") == true)
            {
                string SelectedValue2 = "";
                string[] arrSelected = SelectedValue.Split(',');
                for (int i = 0; i <= arrSelected.Length - 1; i++)
                {
                    SelectedValue2 = SelectedValue2 + "'" + arrSelected[i] + "'";

                    if (i < arrSelected.Length - 1)
                    {
                        SelectedValue2 = SelectedValue2 + ",";
                    }
                }

                SQL = SQL + string.Format(@"and  sCode in ({0}) ", SelectedValue2);

            }
            else if (SelectedValue != string.Empty && SelectedValue.Contains(",") == false)
            { 
         

                SQL = SQL + string.Format(@"and  sCode like '{0}%' ",  SelectedValue);
            }   

            SQL = SQL + string.Format(@"union select 'ALL' as  scode,'ALL' as sContent,'selected' as selected 
                       order by sCode");


            return SQL;
        }



        public string GETDATASQL(string system,string sysCODE, string DocType, string SOrderDate, string EOrderDate,
            string PatNo,string sRequestDocParent,string TabSelect,string  isSing,string EMR_SYSKEYIN_SQL="")
        {
            string SQL = "";
            string GroupOrdeyBy = "";

            if (TabSelect == "收發作業")
            {
                GroupOrdeyBy = "ROW_NUMBER() OVER(PARTITION BY sRequestDocParent ORDER BY SRequestOrderDate+sRequestOrderTime Desc) AS GroupNO";
            }
            else
            {
                GroupOrdeyBy = "ROW_NUMBER() OVER(PARTITION BY sRequestDocParent ORDER BY SRequestOrderDate+sRequestOrderTime Asc) AS GroupNO";
            }


            SQL = string.Format(@"Select * from  ( 
              select {0}
              ,sRequestDocType, sRequestDocParent
              ,sRequestPatientID,sRequestPatinetName,SRequestOrderDate+sRequestOrderTime as ddate 
             , EMR_Syskey,sRequestNo,sRequestUser,sRequestDate,sRequestTime,sRequestPrint
             ,sSystem
             ,CASE WHEN  sStatus='Y' THEN 'VSSign'  
             WHEN  sStatus='N' AND sRequestOrgSign='Y' THEN 'OrgSign'
             ELSE  'NotSign' END AS sStatus ,sRequestDivision
             ,  cast(sRequestPatinetName  AS varbinary(50))   as sRequestPatinetName2
             ,sRequesttimestampDate,sRequesttimestampTime,sRequestDocRoot
             ,sRequestServerFileName,sRequestDocDate
             from [jemrdb].dbo.va_request 
             where 1=1 AND  sRequestType='S' " + "\r\n", GroupOrdeyBy);


            if (system != "ALL")
            {
                if (system.Contains(","))
                {
                    SQL = SQL + string.Format(@" and sSystem in  {0} " + "\r\n", system);
                }
                else
                {
                    SQL = SQL + string.Format(@" and sSystem= '{0}'" + "\r\n", system);
                }
            }

            if (DocType != "ALL")
            {
                SQL = SQL + string.Format(@" and sRequestDocType= '{0}'" + "\r\n", DocType);
            }
            else {
             if (sysCODE.Contains(",") == true)
                {
                    string SelectedValue2 = "";
                    string[] arrSelected = sysCODE.Split(',');
                    for (int i = 0; i <= arrSelected.Length - 1; i++)
                    {
                        SelectedValue2 = SelectedValue2 + "'" + arrSelected[i] + "'";

                        if (i < arrSelected.Length - 1)
                        {
                            SelectedValue2 = SelectedValue2 + ",";
                        }
                    }

                    SQL = SQL + string.Format(@"and  sRequestDocType IN  ({0}) ", SelectedValue2);

                }
                else if (sysCODE != "")
                {
                    SQL = SQL + string.Format(@"and  sRequestDocType like '{0}%' ", sysCODE);
                }
            }
    

            if (SOrderDate != string.Empty && EOrderDate != string.Empty)
            {   
                SQL = SQL + string.Format(@" and sRequestDocDate between  '{0}' AND '{1}'" + "\r\n" + "\r\n", SOrderDate, EOrderDate);
            }

            if (PatNo.TrimEnd() != string.Empty)
            { 
                SQL = SQL + string.Format(@" and sRequestPatientID= '{0}'" + "\r\n", PatNo.TrimEnd());
            }



            if (sRequestDocParent.TrimEnd() != string.Empty)
            {
                SQL = SQL + string.Format(@" and sRequestDocParent= '{0}'" + "\r\n", sRequestDocParent.TrimEnd());
            }


            if (TabSelect == "收發作業")
            {
                SQL = SQL + string.Format(@" and( sStatus='Y' or  sRequestOrgSign='Y' )");
            }
            else
            {
                if (isSing != "ALL")
                {
                    if (isSing == "FemhSign")
                    {
                        SQL = SQL + string.Format(@" and sRequestOrgSign='Y'   AND sStatus='N' " + "\r\n");
                    }
                    else if (isSing == "N")
                    {
                        SQL = SQL + string.Format(@" and sStatus= '{0}' and  sRequestOrgSign is null" + "\r\n", isSing);
                    }
                    else if (isSing == "Y")
                    {
                        SQL = SQL + string.Format(@" and sStatus= '{0}'" + "\r\n", isSing);
                    }
                }

            }

            if (EMR_SYSKEYIN_SQL != string.Empty) {

                SQL = SQL + string.Format(@"and {0}" + "\r\n", EMR_SYSKEYIN_SQL);
            }


            SQL = SQL + string.Format(@")a
                           left join
                          (select sCode,sContent,sRemark from [dbo].[va_reference]
                           where stype='DocType' and sSystem='EMR' )b
                           on a.sRequestDocType=b.sCode
                           left join
                           (select sCode,sContent as sSystem2 ,sRemark from [dbo].[va_reference]
                           where stype='system' and sSystem='EMR' )C
                           on a.sSystem=c.sCode
                           LEFT JOIN
                          (SELECT H_DptID,H_DptName FROM [dbo].[EMR_H_DEPT]) D
                          on a.sRequestDivision=D.H_DptID
                          LEFT JOIN
                          (SELECT DISTINCT H_EmpNO,H_EmpNane FROM [dbo].[EMR_H_EMPY])U
                          on a.sRequestUser=U.H_EmpNO ");


            if (TabSelect == "收發作業")
            {
                SQL = SQL + "\r\n"+ "Where GroupNO='1'";
            }


            SQL = SQL + "\r\n" + "order by ddate desc, sRequestDocParent";

            return SQL;
        }



        public string GETEMRROLE(string sadminid)
        {

            string todaydate = DateTime.Now.ToString("yyyyMMdd");
            string SQL = "select  sPassWord,sName, sRole from [jemrdb].[dbo].[va_Admin]" + "\r\n";
            SQL = SQL + "where sadminid= @sadminid " + "\r\n";
            SQL = SQL + "AND sRole   IN('R001', 'R005', 'R006', 'R015','R021','R022') AND sType = 'U'" + "\r\n";
            SQL = SQL + "and cast('" + todaydate + "' as int)  <= cast(sDateUntil as int)" + "\r\n";
            return SQL;

        }

        public string VerifyPwd()
        {
           return string.Format(@"SELECT sAdminID FROM [dbo].[va_Admin]
WHERE sAdminID = 'femhadmin' and sPassWord = @sPassWord");
            
        }


        public string PrintLogQuery(string  system, string sysCODE, string DocType, string SOrderDate, string EOrderDate, string PatNo) {

            string WhereSQL = "Where  1=1  " + "\r\n";

            if (SOrderDate != string.Empty && EOrderDate != string.Empty)
            {
                WhereSQL = WhereSQL + string.Format(@" and sRequestDocDate between  '{0}' AND '{1}'" + "\r\n" + "\r\n", SOrderDate, EOrderDate);
            }


            if (PatNo.TrimEnd() != string.Empty)
            {
                WhereSQL = WhereSQL + string.Format(@" and sRequestPatientID= '{0}'" + "\r\n", PatNo.TrimEnd());
            }



            if (system != "ALL")
            {
                if (system.Contains(","))
                {
                    WhereSQL = WhereSQL + string.Format(@" and sSystem in  {0} " + "\r\n", system);
                }
                else
                {
                    WhereSQL = WhereSQL + string.Format(@" and sSystem= '{0}'" + "\r\n", system);
                }
            }

            if (DocType != "ALL")
            {
                WhereSQL = WhereSQL + string.Format(@" and sRequestDocType= '{0}'" + "\r\n", DocType);
            }
            else
            {
                if (sysCODE.Contains(",") == true)
                {
                    string SelectedValue2 = "";
                    string[] arrSelected = sysCODE.Split(',');
                    for (int i = 0; i <= arrSelected.Length - 1; i++)
                    {
                        SelectedValue2 = SelectedValue2 + "'" + arrSelected[i] + "'";

                        if (i < arrSelected.Length - 1)
                        {
                            SelectedValue2 = SelectedValue2 + ",";
                        }
                    }

                    WhereSQL = WhereSQL + string.Format(@"and  sRequestDocType IN  ({0}) ", SelectedValue2);

                }
                else if (sysCODE != "")
                {
                    WhereSQL = WhereSQL + string.Format(@"and  sRequestDocType like '{0}%' ", sysCODE);
                }
            }


            string SQL =   string.Format(@" select EMR_SysKey, sRequestOrderDate, sRequestOrderTime, sRequestPatientID, sRequestPatinetName, sRequestUser
  , sRequestDocDate, SYST.sContent AS sSystem,DocType.sContent AS  sRequestDocType, sRequestPrint
  , Print_Date, Print_Time, Print_Reason,H_EmpNane
   from(
  SELECT a.EMR_SysKey, sRequestOrderDate, sRequestOrderTime, sRequestPatientID, sRequestPatinetName, sRequestUser
   , sRequestDocDate , sSystem, sRequestDocType, sRequestPrint
   , Print_Date, Print_Time,
    CASE WHEN Print_Reason= '1' THEN '轉診' WHEN Print_Reason = '2' THEN '本人申請' WHEN Print_Reason = '3' THEN '委託申請'
   WHEN Print_Reason = '5' THEN '院內(研究)' WHEN Print_Reason = '6' THEN '院內(其它)' WHEN Print_Reason = '7' THEN '公文回覆'
   WHEN Print_Reason = '8' THEN '保險申請'  WHEN Print_Reason = '9' THEN '健保作業'  WHEN Print_Reason = '10' THEN '疾病分類'
   WHEN Print_Reason = '11' THEN '其它'   WHEN Print_Reason = '12' THEN '系統管理'  WHEN Print_Reason = '13' THEN '檢調單位'
   WHEN Print_Reason = '14' THEN '衛生主管' END AS  Print_Reason
    FROM (select EMR_SysKey, sRequestOrderDate, sRequestOrderTime, sRequestPatientID, sRequestPatinetName, sRequestUser
   , sRequestDocDate, sSystem, sRequestDocType, sRequestPrint, sRequestserverFilePath, sRequestServerFileName
    from [dbo].[va_request]
     {0}
  )  A 
    JOIN
   (select EMR_Syskey, Print_Date, Print_Time, Print_Reason From [dbo].[va_EMR_Print_Log])B
     ON A.EMR_SysKey=B.EMR_Syskey
  )M
  left join
   (select H_EmpNO, H_EmpNane from[dbo].[EMR_H_EMPY])emp
    on m.sRequestUser = emp.H_EmpNO
       left join
   (select sCode, sContent from [dbo].[va_reference]
  where sSystem='EMR'
  AND sType = 'DocType')DocType
      on m.sRequestDocType = DocType.sCode
    LEFT JOIN
   (select sCode, sContent from [dbo].[va_reference]
  where sSystem='EMR'
  AND sType = 'System')SYST
    on m.sSystem = SYST.sCode", WhereSQL);


            return SQL;
        }



        public string PrintDetail(string sDate,string eDate ,string sTime, string patno) {
      return       string.Format(@"    SELECT sDate, sTime, A.sAdminID,sName ,SCode,sRemark
  FROM(select sDate, sTime, sAdminID,
  CASE WHEN sCode= 'E001' THEN '列印'   WHEN sCode = 'E002' THEN '匯出PDF'  END AS  sCode
   , sRemark
   from dbo.va_EMRlog
   where sDate between {0} and {1}
and sTime like '{3}%'
 and sRemark like '%病歷號碼:{2}%'
AND SCODE IN ('E001','E002'))A
LEFT JOIN
(select sAdminID, sName from dbo.va_Admin)B
ON A.sAdminID=B.sAdminID", sDate, eDate, patno, sTime);
        
        
        }

        public string GETBorrowEMRTblSQL(string SUBMISSIONNUM) {
            return string.Format(@"SELECT  EMR_SYSKEY,RECEIVEROPTIONS
            ,BORROWERID,BORROWINGSDATE,BORROWINGEDATE
FROM  BorrowEMRTbl
  WHERE SUBMISSIONNUM='{0}' 
 AND TRUNC(SYSDATE) BETWEEN TRUNC(BORROWINGSDATE) AND TRUNC(BORROWINGEDATE)", SUBMISSIONNUM);
        }

        public string GETBorrowEMRTblSQL(string SUBMISSIONNUM,string BORROWERID)
        {
            return string.Format(@"SELECT   EMR_SYSKEY,RECEIVEROPTIONS
            ,BORROWERID,BORROWINGSDATE,BORROWINGEDATE,ISHIDEPROFILE
FROM  BorrowEMRTbl
  WHERE SUBMISSIONNUM='{0}'
and BORROWERID='{1}'
AND TRUNC(SYSDATE) BETWEEN TRUNC(BORROWINGSDATE) AND TRUNC(BORROWINGEDATE)", SUBMISSIONNUM, BORROWERID);
        }
     




    }
}