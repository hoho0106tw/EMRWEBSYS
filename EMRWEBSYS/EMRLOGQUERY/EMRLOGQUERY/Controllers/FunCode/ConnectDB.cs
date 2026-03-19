using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.SqlClient;

namespace EMRLOGQUERY.Controllers.FunCode
{
    public class ConnectDB
    {

   


        public string connectionString()
        {

            // return "Data Source=EMRDB\\EMRDB;Failover Partner=EMRDB-BK\\EMRDB;Initial Catalog=jemrdb;User ID=" + DBUser.User + "; Password=" + DBUser.PWD;

            return "";
        }
        private string conn_dbgen()
        {

            //   return String.Format(@"DATA SOURCE = (DESCRIPTION = (ADDRESS_LIST = (ADDRESS = (PROTOCOL = TCP)(HOST = femhrac-scan.ad.femh.local)(PORT = 1521)) (LOAD_BALANCE = YES))(CONNECT_DATA = (SERVICE_NAME = DBGEN) (FAILOVER_MODE =(TYPE = SELECT) (METHOD = BASIC)))); PERSIST SECURITY INFO = True; USER ID = {0}; PASSWORD = {1}; ", DBUser.User, DBUser.PWD);

            return "";
        }

      

        public DataTable SQL_SERVER_GETDT(string SQL)
        {

            System.Data.DataSet ds = new System.Data.DataSet();
            using (SqlConnection myConnection = new SqlConnection())
            {
                myConnection.ConnectionString = connectionString();
                myConnection.Open();

                if (myConnection.State == ConnectionState.Open)
                {
                    SqlDataAdapter Adp = new SqlDataAdapter(SQL, myConnection);
                    Adp.Fill(ds, "temp");
                }
                myConnection.Close();
            }
            return ds.Tables[0];
        }

        public DataTable SQL_SERVER_GETDT2(string SQL, string loginID)
        {

            System.Data.DataSet ds = new System.Data.DataSet();
            using (SqlConnection myConnection = new SqlConnection())
            {
                myConnection.ConnectionString = connectionString();
                myConnection.Open();

                if (myConnection.State == ConnectionState.Open)
                {
                    SqlDataAdapter Adp = new SqlDataAdapter();
                    SqlCommand Command = new SqlCommand(SQL, myConnection);

                    Command.Parameters.Add("@sadminid", SqlDbType.VarChar).Value = loginID;

                    Adp.SelectCommand = Command;
                    Adp.Fill(ds, "temp");
                }
                myConnection.Close();
            }
            return ds.Tables[0];
        }


        public DataTable SQL_SERVER_GETDT3(string SQL, string sPassWord)
        {

            System.Data.DataSet ds = new System.Data.DataSet();
            using (SqlConnection myConnection = new SqlConnection())
            {
                myConnection.ConnectionString = connectionString();
                myConnection.Open();

                if (myConnection.State == ConnectionState.Open)
                {
                    SqlDataAdapter Adp = new SqlDataAdapter();
                    SqlCommand Command = new SqlCommand(SQL, myConnection);

                    Command.Parameters.Add("@sPassWord", SqlDbType.VarChar).Value = sPassWord;

                    Adp.SelectCommand = Command;
                    Adp.Fill(ds, "temp");
                }
                myConnection.Close();
            }
            return ds.Tables[0];
        }

        public int ExecuteNonQuery(string exesql)
        {
            int effect = 0;
            try
            {
                using (OracleConnection conn = new OracleConnection(conn_dbgen()))
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    OracleCommand execmd = new OracleCommand(exesql, conn);
                    execmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                effect = -1;
            }

            return effect;
        }



        public DataTable GETDATABASEDATA(string exesql)
        {
            DataSet DS = new DataSet();
            using (OracleConnection conn = new OracleConnection(conn_dbgen()))
            {
                OracleCommand cmd = new OracleCommand(exesql, conn);
                cmd.BindByName = true;
                cmd.Parameters.Clear();
                OracleDataAdapter DataAdapter = new OracleDataAdapter();
                DataAdapter.SelectCommand = cmd;
                DataAdapter.Fill(DS);
            }
            return DS.Tables[0];
        }

        //public DataTable GETDATABASEDATA(string exesql, string OracleConnection)
        //{
        //    DataSet DS = new DataSet();
        //    using (OracleConnection conn = new OracleConnection(OracleConnection))
        //    {
        //        OracleCommand cmd = new OracleCommand(exesql, conn);
        //        cmd.BindByName = true;
        //        cmd.Parameters.Clear();
        //        OracleDataAdapter DataAdapter = new OracleDataAdapter();
        //        DataAdapter.SelectCommand = cmd;
        //        DataAdapter.Fill(DS);
        //    }
        //    return DS.Tables[0];
        //}

    }
}