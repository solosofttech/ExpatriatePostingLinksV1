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
                ScrapingBrowser oBrowser = new ScrapingBrowser();
                oBrowser.AllowAutoRedirect = true;
                oBrowser.AllowMetaRedirect = true;               
                WebPage oPageResult = oBrowser.NavigateToPage(new Uri("https://www.expatriates.com/cls/41052337.html"));

                // Get Page View Count
                string sPageViewCount = string.Empty;
                var nodePageViewCount = oPageResult.Html.SelectSingleNode("//*[@class='pageviewcount']");
                if (nodePageViewCount != null)
                {
                    //sPageViewCount = nodePageViewCount.InnerText.Trim();
                    Console.WriteLine(nodePageViewCount.Name + " : " + nodePageViewCount.OuterHtml + " : " + nodePageViewCount.InnerText);

                    foreach (HtmlNode node in nodePageViewCount.ChildNodes)
                    {
                        Console.WriteLine(node.Name + " : " + node.OuterHtml + " : " + node.InnerText);
                    }
                }

                Console.ReadLine();
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
                    if (row["catLink"].ToString() != "" )
                    {
                        DataTable dtPages = cPages.GetLinkScrapPages((int)row["catID"]);
                        foreach (DataRow drPage in dtPages.Rows)
                        {
                            Console.WriteLine("Started Link: " + drPage["pageLink"].ToString());

                            cPages.UpdateScrapStartDate(int.Parse(drPage["pageID"].ToString()),DateTime.Now);

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
                                        cPosting objPosting = new cPosting(posting.RawHTML,posting.RawDate);                                        
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
            foreach(DataRow row in dtRawPosting.Rows)
            {
                
                int iPostingID = int.Parse(row["iPostingID"].ToString());
                cPosting objPosting = new cPosting(row["sPostingHTML"].ToString());
                bool bOutput = objPosting.UpdatePosting(iPostingID);
                if (bOutput == true) { Console.WriteLine("Update Posting ID:" + iPostingID.ToString()); iCount++; }
            }

            Console.WriteLine("Total Rows: " + iRowsCount.ToString() + " : Total Updated Rows" + iCount.ToString());
            Console.ReadLine();
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
