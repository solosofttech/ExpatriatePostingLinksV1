using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlTypes;
using System.Data.SqlClient;

namespace ExpatriatePostingLinksV1
{
    class cSinglePosting
    {
        public int iPostingID = 0;
        public string sPostingHTML = string.Empty;
        public string sBodyHTML = string.Empty;
        public int iExpatPostingID = 0;
        public string sPostingURL = string.Empty;
        public string sPostingDesc = string.Empty;
        public DateTime dtmPosting = (DateTime)SqlDateTime.MinValue;
        public float fPrice = 0;
        public string UnitOfPrice = string.Empty;
        public string sMobileNo = string.Empty;
        public string sEmailAddress = string.Empty;
        public DateTime dtmScrapDate = (DateTime)SqlDateTime.MinValue;
        public int iCatID = 0;
        public string sCatID = string.Empty;
        public int iSubCatID = 0;
        public string sSubCatID = string.Empty;
        public string sRegion = string.Empty;
        public int iRegion = 0;
        public string sSubRegion = string.Empty;
        public int iSubRegionID = 0;
        public int iPageCount = 0;
        public bool bActive = false;
        public string sMake = string.Empty;
        public string sModel = string.Empty;
        public int iCarYear = 0;
        public int iCarTransmission = 0;
        public int iCarKM = 0;
        public bool bHasPicture = false;

        public cSinglePosting(object postingID, object expatPostingID)
        {
            if (postingID != null && expatPostingID != null)
                Fill(cPosting.GetPostingByBothPostingAndExpatID((int)postingID, (int)expatPostingID));
            else if (postingID != null)
                Fill(cPosting.GetPostingByID((int)postingID));
            else if (expatPostingID != null)
                Fill(cPosting.GetPostingByExpatID((int)postingID));
        }

        public cSinglePosting(DataRow datarow)
        {
            if (datarow != null)
                Fill(datarow);
        }

        private void Fill(DataRow datarow)
        {
            try
            {
                if (datarow == null) return;

                if (datarow["iPostingID"] != DBNull.Value)
                    iPostingID = int.Parse(datarow["iPostingID"].ToString());

                if (datarow["sPostingHTML"] != DBNull.Value)
                    sPostingHTML = datarow["sPostingHTML"].ToString();

                if (datarow["sBodyHTML"] != DBNull.Value)
                    sBodyHTML = datarow["sBodyHTML"].ToString();

                if (datarow["iExpatPostingID"] != DBNull.Value)                
                    iExpatPostingID = int.Parse(datarow["iExpatPostingID"].ToString());

                if (datarow["sPostingURL"] != DBNull.Value)
                    sPostingURL = datarow["sPostingURL"].ToString();

                if (datarow["sPostingDesc"] != DBNull.Value)
                    sPostingDesc = datarow["sPostingDesc"].ToString();

                if (datarow["dtmPosting"] != DBNull.Value)
                    dtmPosting = DateTime.Parse(datarow["dtmPosting"].ToString()) ;

                if (datarow["fPrice"] != DBNull.Value)
                    fPrice = float.Parse(datarow["fPrice"].ToString());

                if (datarow["UnitOfPrice"] != DBNull.Value)
                    UnitOfPrice = datarow["UnitOfPrice"].ToString();

                if (datarow["sMobileNo"] != DBNull.Value)
                    sMobileNo = datarow["sMobileNo"].ToString();

                if (datarow["sEmailAddress"] != DBNull.Value)
                    sEmailAddress = datarow["sEmailAddress"].ToString();

                if (datarow["dtmScrapDate"] != DBNull.Value)
                    dtmScrapDate = DateTime.Parse(datarow["dtmScrapDate"].ToString());

                if (datarow["iCatID"] != DBNull.Value)
                    iCatID =  int.Parse(datarow["iCatID"].ToString());

                if (datarow["sCatID"] != DBNull.Value)
                    sCatID = datarow["sCatID"].ToString();

                if (datarow["iSubCatID"] != DBNull.Value)
                    iSubCatID = int.Parse(datarow["iSubCatID"].ToString());

                if (datarow["sSubCatID"] != DBNull.Value)
                    sSubCatID = datarow["sSubCatID"].ToString();

                if (datarow["sRegion"] != DBNull.Value)
                    sRegion = datarow["sRegion"].ToString();

                if (datarow["iRegion"] != DBNull.Value)
                    iRegion = int.Parse( datarow["iRegion"].ToString());

                if (datarow["sSubRegion"] != DBNull.Value)
                    sSubRegion = datarow["sSubRegion"].ToString();

                if (datarow["iSubRegionID"] != DBNull.Value)
                    iSubRegionID = int.Parse(datarow["iSubRegionID"].ToString());

                if (datarow["iPageCount"] != DBNull.Value)
                    iPageCount = int.Parse(datarow["iPageCount"].ToString());

                if (datarow["bActive"] != DBNull.Value)
                {
                    int iActive = int.Parse(datarow["bActive"].ToString());
                    if (iActive == 1) bActive = true; else bActive = false;                    
                }

                if (datarow["sMake"] != DBNull.Value)
                    sMake = datarow["sMake"].ToString();

                if (datarow["sModel"] != DBNull.Value)
                    sModel = datarow["sModel"].ToString();

                if (datarow["iCarYear"] != DBNull.Value)
                    iCarYear = int.Parse(datarow["iCarYear"].ToString());

                if (datarow["iCarTransmission"] != DBNull.Value)
                    iCarTransmission = int.Parse(datarow["iCarTransmission"].ToString());

                if (datarow["dCarKM"] != DBNull.Value)
                    iCarKM = int.Parse(datarow["dCarKM"].ToString());

                if (datarow["bHasPicture"] != DBNull.Value)
                {
                    int iPic = (int)datarow["bHasPicture"];
                    if (iPic == 1) bHasPicture = true; else bHasPicture = false;
                }
            }
            catch(Exception oEx) { string sMessage = oEx.Message;return; }
        }

        public bool UpdateAll()
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _commandtext = "UPDATE tblPosting " +
                                     "SET iExpatPostingID = @iExpatPostingID, sPostingURL= @sPostingURL, dtmPosting=@dtmPosting, " +
                                     "sPostingDesc=@sPostingDesc, fPrice=@fPrice, sCatID = @sCatID, sPostingHTML, " +
                                     "sRegion = @sRegion, sSubRegion = @sSubRegion, sMake= @sMake, dtmScrapDate = @dtmScrapDate, " +
                                     "sModel=@sModel, iCarTransmission=@iCarTransmission, iCarKM=@iCarKM, iCarYear=@iCarYear, bHasPicture=@bHasPicture " +
                                     "WHERE iPostingID =" + iPostingID.ToString();
                _sqlcmd.CommandText = _commandtext;
                _sqlcmd.Parameters.AddWithValue("@iExpatPostingID", iExpatPostingID);
                _sqlcmd.Parameters.AddWithValue("@sPostingURL", sPostingURL);
                _sqlcmd.Parameters.AddWithValue("@sPostingHTML", sPostingHTML);
                _sqlcmd.Parameters.AddWithValue("@sPostingDesc", sPostingDesc);
                _sqlcmd.Parameters.AddWithValue("@dtmPosting", dtmPosting);
                _sqlcmd.Parameters.AddWithValue("@fPrice", fPrice);
                _sqlcmd.Parameters.AddWithValue("@SCatID", sCatID);
                _sqlcmd.Parameters.AddWithValue("@sRegion", sRegion);
                _sqlcmd.Parameters.AddWithValue("@sSubRegion", sSubRegion);
                _sqlcmd.Parameters.AddWithValue("@sMake", sMake);
                _sqlcmd.Parameters.AddWithValue("@sModel", sModel);
                _sqlcmd.Parameters.AddWithValue("@iCarTransmission", iCarTransmission);
                _sqlcmd.Parameters.AddWithValue("@iCarKM", iCarKM);
                _sqlcmd.Parameters.AddWithValue("@iCarYear", iCarYear);
                _sqlcmd.Parameters.AddWithValue("@dtmScrapDate", DateTime.Now);
                _sqlcmd.Parameters.AddWithValue("@bHasPicture", bHasPicture);


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

        public bool UpdateNecessary()
        {
            try
            {                
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();                
                _sqlcmd.Connection = _sqlconn;
                string _commandtext = "UPDATE tblPosting " +
                                     "SET dtmPosting=@dtmPosting, sEmailAddress = @sEmailAddress, dtmScrapDate=@dtmScrapDate, " +
                                     "sCatID = @sCatID, sBodyHTML=@sBodyHTML, sMobileNo = @sMobileNo, " +
                                     "sRegion = @sRegion, sSubRegion = @sSubRegion, sMake= @sMake, " +
                                     "sModel=@sModel, iCarTransmission=@iCarTransmission, dCarKM=@iCarKM, iCarYear=@iCarYear, bActive=@bActive " +
                                     "WHERE iPostingID =" + iPostingID.ToString();
                _sqlcmd.CommandText = _commandtext;
                _sqlcmd.Parameters.AddWithValue("@dtmPosting", dtmPosting );
                _sqlcmd.Parameters.AddWithValue("@sBodyHTML", sBodyHTML);                
                _sqlcmd.Parameters.AddWithValue("@SCatID", sCatID);
                _sqlcmd.Parameters.AddWithValue("@sRegion", sRegion);
                _sqlcmd.Parameters.AddWithValue("@sSubRegion", sSubRegion );
                _sqlcmd.Parameters.AddWithValue("@sMake", sMake);
                _sqlcmd.Parameters.AddWithValue("@sModel", sModel);
                _sqlcmd.Parameters.AddWithValue("@iCarTransmission", iCarTransmission);
                _sqlcmd.Parameters.AddWithValue("@iCarKM", iCarKM);
                _sqlcmd.Parameters.AddWithValue("@iCarYear", iCarYear);
                _sqlcmd.Parameters.AddWithValue("@sEmailAddress", sEmailAddress);
                _sqlcmd.Parameters.AddWithValue("@sMobileNo", sMobileNo);
                _sqlcmd.Parameters.AddWithValue("@dtmScrapDate", DateTime.Now);
                _sqlcmd.Parameters.AddWithValue("@bActive",true);

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
            catch(Exception oEx) { string sError = oEx.Message; return false; }
        }

        public bool MarkInActive()
        {
            try
            {
                var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                var _sqlcmd = new SqlCommand();
                _sqlcmd.Connection = _sqlconn;
                string _commandtext = "UPDATE tblPosting " +
                                     "SET bActive = 0 " + 
                                     "WHERE iPostingID =" + iPostingID.ToString();
                _sqlcmd.CommandText = _commandtext;               

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
    }
    
}
