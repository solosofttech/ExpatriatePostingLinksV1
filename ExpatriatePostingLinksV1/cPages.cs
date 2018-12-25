using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ExpatriatePostingLinksV1
{
    enum eScrapStatus
    {
        Completed = 1,
        Pending = 2,
        InProgress = 3, 
        Error = 4
    }

    enum eScrapType
    {
        Pagination = 1,
        MainPosting = 2,
        DetailPosting = 3
    }

    class cPages
    {      
        
        public static void AddPage(out int pageID, int catID, string pageURL, DateTime dtmPagePosted,
           DateTime scrapStart , DateTime scrapEnd, eScrapType scrapType)
        {
            pageID = -1;
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _commandtext = "insert into tblPages " +
                                     "(catID, pageURL, dtmPagePosted,scrapStart,scrapEnd,scrapType,scrapStatus)" +
                                     "values (@catID,@pageURL,@dtmPagePosted,@scrapStart,@scrapEnd,@scrapType,@scrapStatus)";
                _sqlcmd.CommandText = _commandtext;
                _sqlcmd.Parameters.AddWithValue("@catID", catID);
                _sqlcmd.Parameters.AddWithValue("@pageURL", pageURL);
                _sqlcmd.Parameters.AddWithValue("@dtmPagePosted", dtmPagePosted);
                _sqlcmd.Parameters.AddWithValue("@scrapStart", scrapStart);
                _sqlcmd.Parameters.AddWithValue("@scrapEnd", scrapEnd);
                _sqlcmd.Parameters.AddWithValue("@scrapType", (int)scrapType);
                _sqlcmd.Parameters.AddWithValue("@scrapStatus", (int)eScrapStatus.Pending);

                _sqlconn.Open();
                _sqlcmd.ExecuteNonQuery();                
                _sqlcmd.Parameters.Clear();
                _sqlcmd.CommandText = "SELECT @@IDENTITY";
                pageID = Convert.ToInt32(_sqlcmd.ExecuteScalar());
                _sqlcmd.Dispose();
                _sqlcmd = null;
                _sqlconn.Close();
                _sqlconn.Dispose();
                _sqlconn = null;
            }
            catch(SqlException ex) { return; }
        }

        public static DataTable GetCurrentScrapPage(DateTime pageDateTime)
        {
            DataTable dtScrapPages = null;
            try
            {
                return dtScrapPages;
            }
            catch { return null; }

        }

        public static bool CatLinkToScrapExists(int catID)
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                string _commandText = string.Format("Select * from tblPages where catID ={0} and scrapStart = cast('17530101' as datetime)",catID.ToString());
                var _sqlda = new SqlDataAdapter(_commandText, _sqlconn);
                var _datatable = new DataTable();
                _sqlda.Fill(_datatable);
                if (_datatable.Rows.Count > 0)
                    return false;

                return true;
            }
            catch { return false; }
        }

        public static DataTable GetLinkScrapPages(int catID)
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                string _commandText = string.Format(
                    "SELECT tblPages.pageID, CONCAT(tblMainCategory.catLink, tblPages.pageURL) as pageLink FROM tblPages, tblMainCategory WHERE tblPages.catID = tblMainCategory.catID AND tblPages.catID = {0} AND scrapStart = cast('17530101' as datetime)", 
                    catID.ToString());
                var _sqlda = new SqlDataAdapter(_commandText, _sqlconn);
                var _datatable = new DataTable();
                _sqlda.Fill(_datatable);
                return _datatable;                
            }

            catch { return null; }
        }

        public static bool UpdatePage(int pageID, int catID, string pageURL, DateTime dtmPagePosted,
           DateTime scrapStart, DateTime scrapEnd, eScrapType scrapType)
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _CommandText = string.Format("update tblPages " +
                                    "set catID=@catID, pageURL=@pageURL,dtmPagePosted=@dtmPagePosted," +
                                    "scrapStart=@scrapStart,scrapEnd=@scrapEnd,scrapType=@scrapType" +
                                    " where pageID={0}", pageID.ToString());
                _sqlcmd.CommandText = _CommandText;
                _sqlcmd.Parameters.AddWithValue("@catID", catID);
                _sqlcmd.Parameters.AddWithValue("@pageURL", pageURL);
                _sqlcmd.Parameters.AddWithValue("@dtmPagePosted", dtmPagePosted);
                _sqlcmd.Parameters.AddWithValue("@scrapStart", scrapStart);
                _sqlcmd.Parameters.AddWithValue("@scrapEnd", scrapEnd);
                _sqlcmd.Parameters.AddWithValue("@scrapType", (int)scrapType);
                _sqlconn.Open();
                int _row = _sqlcmd.ExecuteNonQuery();
                _sqlcmd.Dispose();
                _sqlcmd = null;
                if (_row == 0)

                    return false;
                else
                    return true;
            }
            catch { return false; }
        }

        public static bool UpdateScrapStartDate(int pageID, DateTime scrapStart)
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _CommandText = string.Format("update tblPages " +
                                    "set scrapStart=@scrapStart, scrapStatus = {0}" +
                                    " where pageID={1}", (int)eScrapStatus.InProgress, pageID.ToString());
                _sqlcmd.CommandText = _CommandText;               
                _sqlcmd.Parameters.AddWithValue("@scrapStart", scrapStart);             
                _sqlconn.Open();
                int _row = _sqlcmd.ExecuteNonQuery();
                _sqlcmd.Dispose();
                _sqlcmd = null;
                if (_row == 0)

                    return false;
                else
                    return true;
            }
            catch { return false; }
        }
        
        public static bool UpdateScrapEndDate(int pageID, DateTime scrapEnd)
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _CommandText = string.Format("update tblPages " +
                                    "set scrapEnd=@scrapEnd, scrapStatus = {0}" +
                                    " where pageID={1}",(int)eScrapStatus.Completed, pageID.ToString());
                _sqlcmd.CommandText = _CommandText;
                _sqlcmd.Parameters.AddWithValue("@scrapEnd", scrapEnd);
                _sqlconn.Open();
                int _row = _sqlcmd.ExecuteNonQuery();
                _sqlcmd.Dispose();
                _sqlcmd = null;
                if (_row == 0)

                    return false;
                else
                    return true;
            }
            catch { return false; }
        }

        public static bool UpdateScrapStatus(int pageID, eScrapStatus scrapStatus)
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _CommandText = string.Format("update tblPages " +
                                    "set scrapStatus=@scrapStatus" +
                                    " where pageID={0}", pageID.ToString());
                _sqlcmd.CommandText = _CommandText;
                _sqlcmd.Parameters.AddWithValue("@scrapEnd", (int)scrapStatus);
                _sqlconn.Open();
                int _row = _sqlcmd.ExecuteNonQuery();
                _sqlcmd.Dispose();
                _sqlcmd = null;
                if (_row == 0)

                    return false;
                else
                    return true;
            }
            catch { return false; }
        }

    }
}
