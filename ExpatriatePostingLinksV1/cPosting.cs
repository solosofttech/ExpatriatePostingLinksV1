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
        enum eVehicleTransmissionType
        {
            None = 0,
            Automatic = 1,
            Manual = 2
        }

        protected string sRawHTML = string.Empty;
        protected string sRawDate = string.Empty;
        protected string sURL = string.Empty;
        protected string sPostingID = string.Empty;
        protected int iExpatPostingID = 0;
        protected string sPostingDesc = string.Empty;
        protected string sRegion = string.Empty;
        protected string sSubRegion = string.Empty;
        protected string sCategory = string.Empty;
        protected float fPrice = 0;
        protected bool bPicture = false;
        string sMake = string.Empty;
        string sModel = string.Empty;
        int iYear = 0;
        double dKM = 0;
        string sTransmission = string.Empty;
        eVehicleTransmissionType eTransmission = eVehicleTransmissionType.None;
        protected DateTime dtmPosted = DateTime.MinValue;
        
        public cPosting(string rawHTML, string rawDate)
        {
            sRawHTML = rawHTML;
            sRawDate = rawDate;

            if(sRawHTML != string.Empty)
            { ParseRawHTML(); ParseForVehicle(); }
        }

        public cPosting(string rawHTML)
        {
            sRawHTML = rawHTML;
            if (sRawHTML != string.Empty)
            { ParseRawHTML(); ParseForVehicle(); }
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
                    string[] saDesc = { "" };
                    if (sPostingDesc.ToLower().Contains("bhd") && sPostingDesc.ToLower().Contains("/"))
                    {
                        saDesc = sPostingDesc.Split("/".ToCharArray());
                    }
                    else if (sPostingDesc.ToLower().Contains("bhd") && sPostingDesc.ToLower().Contains(","))
                    {
                        saDesc = sPostingDesc.Split(",".ToCharArray());
                    }
                    else if (sPostingDesc.ToLower().Contains("bhd") && sPostingDesc.ToLower().Contains("-"))
                    {
                        saDesc = sPostingDesc.Split("-".ToCharArray());
                    }

                    foreach (string desc in saDesc)
                    {
                        if (desc.ToLower().Contains("bhd"))
                        {
                            string sPrice = desc.ToLower().Replace("bhd", "");
                            sPrice = sPrice.Trim();
                            float.TryParse(sPrice, out fPrice);
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
                                sCategory = sCategory.Replace("-", "").Trim();
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

        public bool CheckExistsPosting(out int postingID, int expatPostingID)
        {
            postingID = -1;

            if (expatPostingID <= 0) return false;

            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                //string _commandText = "Select * from tblPosting WHERE iExpatPostingID is null";
                string _commandText = "Select * from tblPosting where iExpatPostingID = " + expatPostingID.ToString();
                var _sqlda = new SqlDataAdapter(_commandText, _sqlconn);
                var _datatable = new DataTable();
                _sqlda.Fill(_datatable);
                if(_datatable.Rows.Count>0)
                {
                    int.TryParse(_datatable.Rows[0]["iPostingID"].ToString(), out postingID);
                    return true;
                }
                return false;
                
            }
            catch
            {
                return false;
            }
        }
        
        public bool AddPosting(out int postingID)
            {
                postingID=-1;
                try
                {
                    if (PostingDate != DateTime.MinValue && RawHTML != string.Empty)
                    {
                        bool bExistsPosting  = CheckExistsPosting(out postingID, iExpatPostingID);

                        if (bExistsPosting == false)
                        {   
                            var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                            var _sqlcmd = new SqlCommand();
                            _sqlcmd.Connection = _sqlconn;
                            string _commandtext = "insert into tblPosting " +
                                                 "(dtmPosting, sPostingHTML,iExpatPostingID,sPostingURL," +
                                                 "sPostingDesc,fPrice,sCatID,sRegion,sSubRegion,sCarMake," +
                                                 "sCarModel,iCarTransmission,dCarKM,iCarYear)" +
                                                 "values (@dtmPosting,@sPostingHTML,@iExpatPostingID,@sPostingURL," +
                                                 "@sPostingDesc,@fPrice,@sCatID,@sRegion,@sSubRegion," +
                                                 "@sCarMake,@sCarModel,@iCarTransmission,@dCarKM,@iCarYear)";
                            _sqlcmd.CommandText = _commandtext;
                            _sqlcmd.Parameters.AddWithValue("@dtmPosting", PostingDate);
                            _sqlcmd.Parameters.AddWithValue("@sPostingHTML", RawHTML);
                            _sqlcmd.Parameters.AddWithValue("@iExpatPostingID", iExpatPostingID);
                            _sqlcmd.Parameters.AddWithValue("@sPostingURL", sURL);
                            _sqlcmd.Parameters.AddWithValue("@sPostingDesc", sPostingDesc);
                            _sqlcmd.Parameters.AddWithValue("@fPrice", fPrice);
                            _sqlcmd.Parameters.AddWithValue("@sCatID", sCategory);
                            _sqlcmd.Parameters.AddWithValue("@sRegion", sRegion);
                            _sqlcmd.Parameters.AddWithValue("@sSubRegion", sSubRegion);
                            _sqlcmd.Parameters.AddWithValue("@sCarMake", sMake);
                            _sqlcmd.Parameters.AddWithValue("@sCarModel", sModel);
                            _sqlcmd.Parameters.AddWithValue("@iCarTransmission", (int)eTransmission);
                            _sqlcmd.Parameters.AddWithValue("@dCarKM", dKM);
                            _sqlcmd.Parameters.AddWithValue("@iCarYear", iYear);

                            _sqlconn.Open();
                            _sqlcmd.ExecuteNonQuery();
                            _sqlcmd.Parameters.Clear();
                            _sqlcmd.CommandText = "SELECT @@IDENTITY";
                            postingID = Convert.ToInt32(_sqlcmd.ExecuteScalar());
                            _sqlcmd.Dispose();
                            _sqlcmd = null;
                            _sqlconn.Close();
                            _sqlconn.Dispose();
                            _sqlconn = null;
                            return true;
                        }
                        else
                        {
                            if (postingID > 0) { UpdatePosting(postingID); return true; }
                        }
                    }

                return false;
                }
                catch(Exception oEx){ string sMessage = oEx.Message; return false;}
            }

        public bool UpdatePosting(int postingID)
        {
            try
            {

                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _commandtext = "UPDATE tblPosting " +
                                     "SET iExpatPostingID = @iExpatPostingID, sPostingURL= @sPostingURL, dtmPosting=@dtmPosting, "+
                                     "sPostingDesc=@sPostingDesc, fPrice=@fPrice, SCatID = @sCatID, " +
                                     "sRegion = @sRegion, sSubRegion = @sSubRegion, sCarMake= @sCarMake, " +
                                     "sCarModel=@sCarModel, iCarTransmission=@iCarTransmission, dCarKM=@dCarKM, iCarYear=@iCarYear " +
                                     "WHERE iPostingID =" + postingID.ToString();
                _sqlcmd.CommandText = _commandtext;
                _sqlcmd.Parameters.AddWithValue("@iExpatPostingID", iExpatPostingID);
                _sqlcmd.Parameters.AddWithValue("@sPostingURL", sURL);
                _sqlcmd.Parameters.AddWithValue("@sPostingDesc", sPostingDesc);
                _sqlcmd.Parameters.AddWithValue("@dtmPosting", PostingDate);
                _sqlcmd.Parameters.AddWithValue("@fPrice", fPrice);
                _sqlcmd.Parameters.AddWithValue("@SCatID", sCategory);
                _sqlcmd.Parameters.AddWithValue("@sRegion", sRegion);
                _sqlcmd.Parameters.AddWithValue("@sSubRegion", sSubRegion);
                _sqlcmd.Parameters.AddWithValue("@sCarMake", sMake);
                _sqlcmd.Parameters.AddWithValue("@sCarModel", sModel);
                _sqlcmd.Parameters.AddWithValue("@iCarTransmission", (int)eTransmission);
                _sqlcmd.Parameters.AddWithValue("@dCarKM", dKM);
                _sqlcmd.Parameters.AddWithValue("@iCarYear", iYear);


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
            catch(Exception oEx) { string sMessage = oEx.Message; return false; }
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

        private void ParseForVehicle()
        {
            if (sPostingDesc == string.Empty) { return; }

            if (sCategory != "vehicles") { return; }

            string[] saVehicle = { "" };
            bool bHasPrice = false;

            if (sPostingDesc.Contains("/") && sPostingDesc.ToLower().Contains("bhd"))
            {
                saVehicle = sPostingDesc.Split("/".ToCharArray());
                bHasPrice = true;
            }
            else
                saVehicle = sPostingDesc.Split(",".ToCharArray());

            string[] saVehicleParts = { "" };

            if (bHasPrice)
                saVehicleParts = saVehicle[1].Split(",".ToCharArray());
            else
                saVehicleParts = saVehicle;

            // get make and model
            string[] saMakeModel = saVehicleParts[0].Trim().Split(" ".ToCharArray());
            if (saMakeModel.Length > 0)
            {
                for (int i = 0; i < saMakeModel.Length; i++)
                {
                    if (i == 0) { sMake = saMakeModel[i]; }
                    else { sModel = sModel + " " + saMakeModel[i]; }
                }

                sMake = sMake.Trim();
                sModel = sModel.Trim();                
            }           

            // year
            if (saVehicleParts.Length >= 2)
            {
                int.TryParse(saVehicleParts[1], out iYear);
            }

            // transmission type
            if (saVehicleParts.Length >= 3)
            {
                if (saVehicleParts[2].ToLower().Contains("automatic"))
                {
                    sTransmission = "Automatic";
                    eTransmission = eVehicleTransmissionType.Automatic;
                }
                else if (saVehicleParts[2].ToLower().Contains("manual"))
                {
                    sTransmission = "Manual";
                    eTransmission = eVehicleTransmissionType.Manual;
                }
            }

            // transmission type
            if (saVehicleParts.Length >= 4)
            {
                if (saVehicleParts[3].ToLower().Contains("km"))
                {
                    string sKM = saVehicleParts[3].ToLower().Trim().Replace("km", "");
                    sKM = sKM.Trim();
                    double.TryParse(sKM, out dKM);
                }
            }

        }
    }
}
