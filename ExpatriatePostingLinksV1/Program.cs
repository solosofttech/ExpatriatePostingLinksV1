using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HtmlAgilityPack;
using NScrape;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using NScrape.Forms;
using System.Globalization;
using ScrapySharp;
using ScrapySharp.Network;



namespace ExpatriatePostingLinksV1
{
    class Program
    {
        static void Main(string[] args)
        {
            //ScrapCatLinkPagination();
            //ScrapPostingFromPages();
            //UpdatePostingTable();
            ScrapIndividualPages();
        }

        static void ScrapIndividualPages()
        {
            try
            {
                // Get Main Categories Links and create jobs/pages
                DataTable dtPostings = cPosting.GetActivePostings();

                if (dtPostings != null && dtPostings.Rows.Count > 0)
                {
                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

                    int iSuccessCount = 0;
                    int iNotSuccessCount = 0;
                    foreach (DataRow row in dtPostings.Rows)
                    {
                        cSinglePosting oPosting = new cSinglePosting(row);
                        bool bReturn = ScrapIndividualPostings(oPosting);
                        if (bReturn == true)
                        {
                           iSuccessCount++;
                           oPosting.UpdateNecessary();                                                       
                        }
                        else { iNotSuccessCount++; }

                        Console.WriteLine(string.Format("Total:{0} Success:{1} Not Success:{2}",
                                dtPostings.Rows.Count.ToString(), iSuccessCount.ToString(), iNotSuccessCount.ToString()));
                    }

                    Console.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    Console.ReadLine();
                }
                
            }
            catch (Exception oEx) { Console.WriteLine("Error:" + oEx.Message); }
        }

        static void ScrapCatLinkPagination()
        {
            // Get Main Categories Links and create jobs/pages
            DataTable dtMainCategories = cMainCategory.GetMainCategories();

            if (dtMainCategories != null && dtMainCategories.Rows.Count > 0)
            {
                foreach (DataRow row in dtMainCategories.Rows)
                {
                    if (row["catLink"].ToString() != "" && cPages.CatLinkToScrapExists((int)row["catID"]))
                    {
                        Console.WriteLine("Started Link: " + row["catLink"].ToString());

                        var webClient = new WebClient();
                        var response = webClient.SendRequest(new Uri(row["catLink"].ToString()));
                        int iPageCount = 0;

                        if (response.ResponseType == WebResponseType.Html)
                        {
                            var scraper = new ExpatriateScraper(((HtmlWebResponse)response).Html);

                            //scraper.GetTags();
                            List<string> lsURL = scraper.GetPageCountURL();

                            foreach (string url in lsURL)
                            {
                                int iPageID = -1;
                                cPages.AddPage(out iPageID, int.Parse(row["catID"].ToString()), url,
                                    DateTime.Now, (DateTime)SqlDateTime.MinValue, (DateTime)SqlDateTime.MinValue, eScrapType.Pagination);
                                if (iPageID > 0)
                                    iPageCount = iPageCount + 1;
                            }
                        }

                        Console.WriteLine("Total pages/job links added: " + iPageCount.ToString());
                    }
                }
            }

            //Console.ReadLine();
        }

        static void ScrapPostingFromPages()
        {

            // Get Main Categories Links and create jobs/pages
            DataTable dtMainCategories = cMainCategory.GetMainCategories();

            if (dtMainCategories != null && dtMainCategories.Rows.Count > 0)
            {
                foreach (DataRow row in dtMainCategories.Rows)
                {
                    if (row["catLink"].ToString() != "")
                    {
                        DataTable dtPages = cPages.GetLinkScrapPages((int)row["catID"]);
                        foreach (DataRow drPage in dtPages.Rows)
                        {
                            Console.WriteLine("Started Link: " + drPage["pageLink"].ToString());

                            cPages.UpdateScrapStartDate(int.Parse(drPage["pageID"].ToString()), DateTime.Now);

                            var webClient = new WebClient();
                            var response = webClient.SendRequest(new Uri(drPage["pageLink"].ToString()));
                            int iPostingCount = 0;

                            if (response.ResponseType == WebResponseType.Html)
                            {
                                var scraper = new ExpatriateScraper(((HtmlWebResponse)response).Html);

                                //scraper.GetTags();
                                List<cPosting> lsPosting = scraper.GetTags();

                                foreach (cPosting posting in lsPosting)
                                {
                                    if (posting.PostingDate != DateTime.MinValue && posting.RawHTML != string.Empty)
                                    {
                                        int iPostingID = -1;
                                        //cPosting.AddRawHTMLPosting(out iPostingID, DateTime.Now, posting.RawHTML);
                                        cPosting objPosting = new cPosting(posting.RawHTML, posting.RawDate);
                                        objPosting.AddPosting(out iPostingID);
                                        if (iPostingID > 0)
                                        {
                                            iPostingCount = iPostingCount + 1;
                                        }
                                    }
                                }
                            }

                            cPages.UpdateScrapEndDate(int.Parse(drPage["pageID"].ToString()), DateTime.Now);

                            Console.WriteLine("Total postings added: " + iPostingCount.ToString());
                        }
                    }
                }
            }

            //Console.ReadLine();
        }

        static void UpdatePostingTable()
        {
            DataTable dtRawPosting = cPosting.GetAllRawHTMLPosting();
            int iRowsCount = dtRawPosting.Rows.Count;
            int iCount = 0;
            foreach (DataRow row in dtRawPosting.Rows)
            {

                int iPostingID = int.Parse(row["iPostingID"].ToString());
                cPosting objPosting = new cPosting(row["sPostingHTML"].ToString());
                bool bOutput = objPosting.UpdatePosting(iPostingID);
                if (bOutput == true) { Console.WriteLine("Update Posting ID:" + iPostingID.ToString()); iCount++; }
            }

            Console.WriteLine("Total Rows: " + iRowsCount.ToString() + " : Total Updated Rows" + iCount.ToString());
            Console.ReadLine();
        }

        private static bool ScrapIndividualPostings(cSinglePosting posting)
        {
            try
            {
                bool bReturn = false;
                
                var html = "https://www.expatriates.com" + posting.sPostingURL;
                HtmlWeb web = new HtmlWeb();
                var htmlDoc = web.Load(html);              

                var mainTitle = htmlDoc.DocumentNode.SelectSingleNode("//*[@class='page-title']//h1");
                if (mainTitle == null) return bReturn;
                if (mainTitle.InnerText.Trim().Contains("Page Not Found"))
                {
                    //Console.WriteLine("Page In Active Now");
                    posting.MarkInActive();
                    Console.WriteLine(string.Format("Posting ID: {0} - PAGE NOT FOUND", posting.iExpatPostingID.ToString()));
                    return bReturn;
                }

                var nodeTitle = htmlDoc.DocumentNode.SelectSingleNode("//*[@class='no-bullet']");

                string sDate = string.Empty;
                string sCategory = string.Empty;
                string sRegion = string.Empty;
                string sSubRegion = string.Empty;
                string sPostingID = string.Empty;
                string sTelephone = string.Empty;
                string sEncodedEmail = string.Empty;
                string sEmail = string.Empty;

                if (nodeTitle != null && nodeTitle.HasChildNodes)
                {
                    foreach (HtmlNode n in nodeTitle.ChildNodes)
                    {
                        //Console.WriteLine("\r\nName: " + n.Name + " HTML: " + n.OuterHtml + " Text: " + n.InnerText );

                        if (n.InnerText.Contains("Date:"))
                        {
                            string[] saDate = n.InnerText.Split(":".ToCharArray());
                            if (saDate.Length == 2)
                            {
                                sDate = saDate[1].Trim();
                                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US"); ;
                                DateTimeStyles styles = DateTimeStyles.None;
                                DateTime dtmPosted = (DateTime)SqlDateTime.MinValue;
                                if (DateTime.TryParse(sDate, culture, styles, out dtmPosted) == true)
                                    posting.dtmPosting = dtmPosted;

                            }
                        }
                        else if (n.InnerText.Contains("Category:"))
                        {
                            string[] saCategory = n.InnerText.Split(":".ToCharArray());
                            if (saCategory.Length == 2) { sCategory = saCategory[1].Trim(); posting.sCatID = sCategory; }
                        }

                        else if (n.InnerText.Contains("Region:"))
                        {
                            string[] saRegion = n.InnerText.Split(":".ToCharArray());
                            if (saRegion.Length == 2)
                            {
                                sRegion = saRegion[1].Trim().Replace("\r", "").Replace("\n", "");
                                posting.sRegion = sRegion;

                                if (sRegion.Contains("(") && sRegion.Contains(")"))
                                {
                                    sSubRegion = sRegion.Substring(sRegion.IndexOf("(") + 1, (sRegion.LastIndexOf(")") - sRegion.IndexOf("(")) - 1).Trim();
                                    posting.sSubRegion = sSubRegion;
                                    sRegion = sRegion.Replace("(" + sSubRegion + ")", "");
                                    posting.sRegion = sRegion;
                                }
                            }
                        }

                        else if (n.InnerText.Contains("Posting ID:"))
                        {
                            sPostingID = n.InnerText;
                            string[] saPosting = n.InnerText.Split(":".ToCharArray());
                            if (saPosting.Length == 2)
                            {
                                sPostingID = saPosting[1].Trim();
                                int iPostingID = 0;
                                if (int.TryParse(sPostingID, out iPostingID))
                                    posting.iExpatPostingID = iPostingID;
                            }
                        }

                        else if (n.OuterHtml.Contains("tel:"))
                        {
                            sTelephone = n.InnerText.Trim();
                            posting.sMobileNo = sTelephone;
                        }

                        else if (n.InnerText.Contains("From:"))
                        {
                            var emailNode = n.SelectSingleNode("//*[@class='__cf_email__']");
                            if (emailNode != null)
                            {
                                foreach (HtmlAttribute attribute in emailNode.Attributes)
                                {
                                    if (attribute.Name.Contains("data-cfemail"))
                                    {
                                        sEncodedEmail = attribute.Value;
                                    }
                                }
                            }
                        }
                    }
                }

                /*Console.WriteLine("\nDate: " + sDate);
                Console.WriteLine("\nCategory: " + sCategory);
                Console.WriteLine("\nRegion: " + sRegion);
                Console.WriteLine("\nSub Region: " + sSubRegion);
                Console.WriteLine("\nPostingID: " + sPostingID);
                Console.WriteLine("\nTelephone: " + sTelephone);*/

                if (sEncodedEmail.Length > 0)
                {
                    string sDecodeEmail = cMain.cfDecodeEmail(sEncodedEmail);
                    if (cMain.isValidEmail(sDecodeEmail))
                    {
                        sEmail = sDecodeEmail;
                        posting.sEmailAddress = sEmail;
                        //Console.WriteLine("\nEmail: " + sEmail);
                    }
                }

                // Get Page View Count
                string sPageViewCount = string.Empty;
                int iPageViewCount = 0;
                var nodePageViewCount = htmlDoc.DocumentNode.SelectSingleNode("//*[@class='pageviewcount']");
                if (nodePageViewCount != null)
                {
                    sPageViewCount = nodePageViewCount.InnerText.Trim();
                    int.TryParse(sPageViewCount, out iPageViewCount);
                    posting.iPageCount = iPageViewCount;
                }

                string sBodyHTML = string.Empty;
                string sBodyInnerText = string.Empty;
                string sMake = string.Empty;
                string sModel = string.Empty;
                string sYear = string.Empty;
                int iYear = 0;
                string sTransmission = string.Empty;
                string sKM = string.Empty;
                int iKM = 0;
                var nodeBody = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='post-body']");
                sBodyHTML = nodeBody.OuterHtml.Trim();
                posting.sBodyHTML = sBodyHTML;
                if (nodeBody != null && posting.sCatID.ToLower().Contains("vehicles"))
                {
                    string[] saBody = sBodyHTML.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string val in saBody)
                    {
                        if (val.Trim().Contains("Make and Model:"))
                        {
                            string[] saMakeModel = val.Trim().Split(":".ToCharArray());

                            if (saMakeModel.Length == 2)
                            {
                                sMake = saMakeModel[1];

                                if (sMake.Contains(" "))
                                {
                                    string[] saModel = sMake.Trim().Split(" ".ToCharArray());
                                    for (int i = 0; i < saModel.Length; i++)
                                    {
                                        if (i == 0) { sMake = saModel[i]; }
                                        else { sModel = sModel + " " + saModel[i]; }
                                    }
                                }
                            }
                            sMake = sMake.Trim();
                            posting.sMake = sMake;
                            sModel = sModel.Trim();
                            posting.sModel = sModel;
                        }

                        else if (val.Trim().Contains("Year:"))
                        {
                            string[] saYear = val.Trim().Split(":".ToCharArray());
                            if (saYear.Length == 2)
                            {
                                if (saYear[1].Length > 0)
                                {
                                    sYear = saYear[1].Trim();
                                    int.TryParse(sYear, out iYear);
                                }
                            }

                            posting.iCarYear = iYear;
                        }

                        else if (val.Trim().Contains("Transmission:"))
                        {
                            string[] saTransmission = val.Trim().Split(":".ToCharArray());
                            if (saTransmission.Length == 2)
                            {
                                if (saTransmission[1].Length > 0)
                                {
                                    sTransmission = saTransmission[1].Trim();
                                }
                            }

                            if (sTransmission != string.Empty && sTransmission.ToLower().Contains("automatic"))
                            {
                                posting.iCarTransmission = (int)eVehicleTransmissionType.Automatic;
                            }
                            if (sTransmission != string.Empty && sTransmission.ToLower().Contains("manual"))
                            {
                                posting.iCarTransmission = (int)eVehicleTransmissionType.Manual;
                            }
                        }

                        else if (val.Trim().Contains("Odometer:"))
                        {
                            string[] saKM = val.Trim().Split(":".ToCharArray());
                            if (saKM.Length == 2)
                            {
                                if (saKM[1].Length > 0)
                                {
                                    sKM = saKM[1].Trim();
                                    sKM = sKM.Replace("KM", "").Trim();
                                    int.TryParse(sKM, out iKM);
                                }
                            }
                            posting.iCarKM = iKM;
                        }
                    }

                    //Console.WriteLine("Make: " + sMake + " Model: " + sModel + " Year: " + iYear.ToString() + " Transmission: " + sTransmission + " KM: " + iKM.ToString());
                }

                return true;
            }
            catch(Exception oEx) { string sMessage = oEx.Message; return false; }
        }
            
    }

    

    class ExpatriateScraper: Scraper
    {
        public ExpatriateScraper(string html) : base(html)
        {
        }

        public List<cPosting> GetTags()
        {            
            string rawDate= string.Empty;
            string rawPosting = string.Empty;
            List<cPosting> listPostings = new List<cPosting>();           
            
            HtmlNodeCollection ldCollection = HtmlDocument.DocumentNode.SelectNodes("//*[@class='listing-date' or @class='listing-content']");
            if (ldCollection != null && ldCollection.Count >= 0)
            {
                foreach (HtmlNode parent in ldCollection)
                {
                    HtmlNodeCollection childCollection = parent.ChildNodes;
                    if (childCollection != null && childCollection.Count > 0)
                    {
                        foreach (HtmlNode child in childCollection)
                        {
                            if (child.OuterHtml.Trim().Length > 0)
                            {
                                if (child.Name.Contains("li") == false)
                                {                                    
                                    // Parse a date and time with no styles.
                                    rawDate = child.OuterHtml;
                                }
                                else
                                {                                    
                                    rawPosting = child.OuterHtml;
                                }

                                if (rawDate != string.Empty || rawPosting!=string.Empty)
                                {
                                    listPostings.Add(new cPosting(rawPosting, rawDate));
                                    rawPosting = string.Empty;                                    
                                }
                            }
                        }
                    }
                }
            }

            /*foreach (cPosting posting in listPostings)
            {
                Console.WriteLine("Date-->" + posting.PostingDate.ToShortDateString() + ", HTML-->" + posting.RawHTML);
                Console.WriteLine("\r\n***************************");
            }
            Console.ReadLine();*/

            return listPostings;
        }

        public List<string> GetPageCountURL()
        {
            List<int> lsPages = new List<int>();
            var nodeCollection = HtmlDocument.DocumentNode.SelectNodes("//*[@class='pagination']");

            if (nodeCollection != null && nodeCollection.Count > 0)
            {
                foreach (var n in nodeCollection)
                {
                    if (n.ChildNodes.Count > 0)
                    {
                        foreach (var child in n.ChildNodes)
                        {
                            if (child.Attributes.Contains("href"))
                            {                                
                                if (IsNumeric(child.InnerText))
                                    lsPages.Add(int.Parse(child.InnerText.Trim()));
                            }
                        }
                    }
                }
            }

            List<string> lstURL = new List<string>();

            if (lsPages.Count > 0)
            {
                int max = lsPages.Max();
                int min = lsPages.Min();

                for (int i = 0; i < max; i++)
                {
                    if (i == 0)
                        lstURL.Add("index.html");
                    else
                        lstURL.Add("index" + (i * 100) + ".html");
                }
            }

            return lstURL;
        }

        public bool IsNumeric(string s)
        {
            foreach (char c in s)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }

            return true;
        }

    }
}
