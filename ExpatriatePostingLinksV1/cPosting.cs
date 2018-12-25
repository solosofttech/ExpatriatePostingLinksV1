using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using HtmlAgilityPack;

namespace ExpatriatePostingLinksV1
{
    class cPosting
    {
        string sRawHTML = string.Empty;
        string sRawDate = string.Empty;
        string sURL = string.Empty;
        string sPostingID = string.Empty;
        int iExpatPostingID = 0;
        string sPostingDesc = string.Empty;
        string sRegion = string.Empty;
        string sSubRegion = string.Empty;
        string sCategory = string.Empty;
        float fPrice = 0;
        bool bPicture = false;
        DateTime dtmPosted = DateTime.MinValue;
        

        public cPosting(string rawHTML, string rawDate)
        {
            sRawHTML = rawHTML;
            sRawDate = rawDate;

            if(sRawHTML != string.Empty)
            { ParseRawHTML(); }
        }

        public cPosting(string rawHTML)
        {
            sRawHTML = rawHTML;
            if (sRawHTML != string.Empty)
            { ParseRawHTML(); }
        }

        private void ParseRawHTML()
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(sRawHTML);
            var nodeCollection = htmlDoc.DocumentNode.SelectSingleNode("//li").ChildNodes;
            bool bPostingDesc = false;
            foreach (HtmlNode node in nodeCollection)
            {
                if (bPostingDesc == false)
                {
                    sPostingDesc = node.ParentNode.InnerText;

                    if (sPostingDesc.ToLower().Contains("bhd") && sPostingDesc.ToLower().Contains("/"))
                    {
                        string[] saDesc = sPostingDesc.Split("/".ToCharArray());
                        foreach (string desc in saDesc)
                        {
                            if (desc.Contains("BHD"))
                            {
                                string sPrice = desc.Replace("BHD", "");
                                sPrice = sPrice.Trim();
                                float.TryParse(sPrice, out fPrice);
                            }
                        }
                    }
                    bPostingDesc = true;
                }

                if (node.HasAttributes)
                {
                    foreach (HtmlAttribute attribute in node.Attributes)
                    {
                        if (attribute.Name == "href")
                        {
                            sURL = attribute.Value;
                            string[] saURL = sURL.Split("/".ToCharArray());
                            if(saURL.Length>0)
                            {                                
                                foreach(string url in saURL)
                                {
                                    if(url.ToLower().Contains(".html"))
                                    {
                                        sPostingID = url.Replace(".html", "");
                                        sPostingID = sPostingID.Trim();
                                        int.TryParse(sPostingID, out iExpatPostingID);
                                    }
                                }
                            }
                        }
                        else if (attribute.Name == "class" && attribute.Value == "listing-region")
                        {
                            sRegion = node.InnerText;
                        }
                        else if (attribute.Name == "class" && attribute.Value == "listing-newregion")
                        {
                            sSubRegion = node.InnerText;
                        }
                        else if (attribute.Name == "class" && attribute.Value == "listing-category")
                        {
                            sCategory = node.InnerText.Trim();
                            if(sCategory.Contains("-"))
                                sCategory = sCategory.Replace("-", "");
                        }
                        else if (attribute.Name == "class" && attribute.Value == "listing-pic")
                        {
                            bPicture = true;
                        }
                    }

                }
            }

          }
        
        public string RawHTML
        {
            get { return sRawHTML; }
        }

        public string RawDate
        {
            get { return sRawDate; }
        }

        public DateTime PostingDate
        {
            get
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US"); ;
                DateTimeStyles styles = DateTimeStyles.None; ;
                if (DateTime.TryParse(sRawDate, culture, styles, out dtmPosted) == false)
                    dtmPosted = DateTime.MinValue;
                return dtmPosted;
            }
        }

        public bool UpdatePosting(int postingID)
        {
            try
            {

                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _commandtext = "UPDATE tblPosting " +
                                     "SET iExpatPostingID = @iExpatPostingID, sPostingURL= @sPostingURL, "+
                                     "sPostingDesc=@sPostingDesc, fPrice=@fPrice, SCatID = @sCatID, " +
                                     "sRegion = @sRegion, sSubRegion = @sSubRegion " +
                                     "WHERE iPostingID =" + postingID.ToString();
                _sqlcmd.CommandText = _commandtext;
                _sqlcmd.Parameters.AddWithValue("@iExpatPostingID", iExpatPostingID);
                _sqlcmd.Parameters.AddWithValue("@sPostingURL", sURL);
                _sqlcmd.Parameters.AddWithValue("@sPostingDesc", sPostingDesc);
                _sqlcmd.Parameters.AddWithValue("@fPrice", fPrice);
                _sqlcmd.Parameters.AddWithValue("@SCatID", sCategory);
                _sqlcmd.Parameters.AddWithValue("@sRegion", sRegion);
                _sqlcmd.Parameters.AddWithValue("@sSubRegion", sSubRegion);


                _sqlconn.Open();
                int _row = _sqlcmd.ExecuteNonQuery();
                _sqlcmd.Dispose();
                _sqlcmd = null;
                _sqlconn.Close();
                _sqlconn.Dispose();
                _sqlconn = null;

               
                if (_row == 0)
                    return false;
                else
                    return true;

            }
            catch { return false; }
        }
        public static void AddRawHTMLPosting(out int iPostingID, DateTime dtmPosting, string sPostingHTML)
        {
            iPostingID = -1;

            try
            {
                
                    var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                    var _sqlcmd = new SqlCommand();
                    _sqlcmd.Connection = _sqlconn;
                    string _commandtext = "insert into tblPosting " +
                                         "(dtmPosting, sPostingHTML)" +
                                         "values (@dtmPosting,@sPostingHTML)";
                    _sqlcmd.CommandText = _commandtext;
                    _sqlcmd.Parameters.AddWithValue("@dtmPosting", dtmPosting);
                    _sqlcmd.Parameters.AddWithValue("@sPostingHTML", sPostingHTML);                   

                    _sqlconn.Open();
                    _sqlcmd.ExecuteNonQuery();
                    _sqlcmd.Parameters.Clear();
                    _sqlcmd.CommandText = "SELECT @@IDENTITY";
                    iPostingID = Convert.ToInt32(_sqlcmd.ExecuteScalar());
                    _sqlcmd.Dispose();
                    _sqlcmd = null;
                    _sqlconn.Close();
                    _sqlconn.Dispose();
                    _sqlconn = null;
               
            }
            catch { return; }

        }
        public static DataTable GetAllRawHTMLPosting()
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                //string _commandText = "Select * from tblPosting WHERE iExpatPostingID is null";
                string _commandText = "Select * from tblPosting";
                var _sqlda = new SqlDataAdapter(_commandText, _sqlconn);
                var _datatable = new DataTable();
                _sqlda.Fill(_datatable);
                return _datatable;
            }
            catch { return null; }
        }
    }
}
