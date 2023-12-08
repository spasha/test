using log4net;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace VA.FSC.Services.Common.Helpers
{
    public static class SqlHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SqlHelper));
        public static int ExecuteNonQuery(string procedure, SqlParameter[] paramas)
        {
            int successFlag = 0;
            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlConnectionString"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(procedure, conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddRange(paramas);
                    successFlag = cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    Log.Error("Failed to Execute NonQuery.", ex); // ErrorLevel = Critical
                    Log.Error("Message: " + ex.Errors[i].Message.AntiLogForging() + "\n" +
                              "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                              "Source: " + ex.Errors[i].Source.AntiLogForging() + "\n" +
                              "Procedure: " + ex.Errors[i].Procedure.AntiLogForging() + "\n");
                }
            }

            return successFlag;
        }

       
    }
}
