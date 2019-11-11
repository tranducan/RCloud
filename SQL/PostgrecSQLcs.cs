using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace RCloud.SQL
{
   


    class PostgrecSQLcs
    {
        NpgsqlConnection conn = new NpgsqlConnection();
        private static object obj = null;
        private static string serverIP = "";
        private static string serverPort = "3312";
        private static string database = "";
        private string m_userName = "Toluen";
        private string m_password = "1111";
        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();
public bool CheckConnection(ref bool IsConnect)
        {
            string connstring = String.Format("Server={0};Port={1};" +
                 "User Id={2};Password={3};Database={4};",
                 serverIP, serverPort, m_userName,
                 m_password, tbDataBaseName.Text);
            // Making connection with Npgsql provider
            NpgsqlConnection conn = new NpgsqlConnection(connstring);
            return true;
        }

    }
}
