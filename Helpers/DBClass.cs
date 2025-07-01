using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace GBSWarehouse.Helpers
{
    public static class DBClass
    {
        private static SqlConnection Con = new SqlConnection();
        private static string ConStr = string.Empty;
        private static bool OpenConnection()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("appsettings.json")
                            .Build();

            ConStr = configuration.GetConnectionString("DefaultConnection");
            Con = new SqlConnection();
            Con.ConnectionString = ConStr;
            try
            {
                if (Con.State == ConnectionState.Closed) Con.Open();
                return true;
            }
            catch
            {
                try
                {
                    if (Con.State == ConnectionState.Closed) Con.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public static bool GetSelected(string SelectString, ref DataTable DTST, string TableName = "")
        {
            if (Con.State == ConnectionState.Open || OpenConnection())
            {
                SqlDataAdapter tmpDA;
                // Dim tmpDT As Data.DataTable
                int ReturnValue;
                tmpDA = new SqlDataAdapter(SelectString, Con);
                // tmpDT = New Data.DataTable
                try
                {
                    if (TableName.Equals(""))
                    {
                        DTST = new DataTable();
                        ReturnValue = tmpDA.Fill(DTST);
                    }
                    else
                        ReturnValue = tmpDA.Fill(DTST);
                }
                catch
                {
                    return false;
                }
                if (ReturnValue == 0)
                    return false;
                else
                    tmpDA = null;
                return true;
            }
            else
                return false;
        }
        public static List<string> CallStoredProcedure(string[,] InParam, string[,] OutParam, string _procename)
        {
            try
            {
                string _result;
                string[,] _temparray = new string[OutParam.Length / 2 - 1 + 1, 2];
                List<string> TList = new List<string>();
                if (Con.State == ConnectionState.Open || OpenConnection())
                {
                    using (SqlCommand sqlCmd = new SqlCommand(_procename, Con))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        // InputParaMeter
                        for (int i = 0; i <= InParam.Length / 2 - 1; i++)
                            sqlCmd.Parameters.AddWithValue(InParam[i, 0], InParam[i, 1]);
                        // OutPutParamater
                        for (int i = 0; i <= OutParam.Length / 2 - 1; i++)
                        {
                            if (OutParam[i, 1] == "SqlDbType.Int")
                                sqlCmd.Parameters.Add(OutParam[i, 0], SqlDbType.Int);
                            else
                                sqlCmd.Parameters.Add(OutParam[i, 0], SqlDbType.NVarChar, 1000);
                            sqlCmd.Parameters[OutParam[i, 0]].Direction = ParameterDirection.Output;
                        }
                        sqlCmd.ExecuteNonQuery();

                        for (int i = 0; i <= OutParam.Length / 2 - 1; i++)
                        {
                            _result = sqlCmd.Parameters[OutParam[i, 0]].Value.ToString();
                            _temparray[i, 0] = OutParam[i, 0];
                            _temparray[i, 1] = _result;
                            TList.Add(_result);
                        }
                    }
                }
                return TList;
            }
            catch (Exception ex)
            {
                List<string> EList = new List<string>();
                EList.Add("3");//ReturnValue
                EList.Add("E");//ErrorNo
                EList.Add("E");//ErrorStatus
                EList.Add(ex.Message);//ErrorMsg
                return EList;
            }
        }
    }
}
