using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ExpatriatePostingLinksV1
{
    class cMain
    {
        // Retrieves a connection string by name.
        // Returns null if the name is not found.
        public static string GetConnectionString()
        {
            // Assume failure.
            string returnValue = null;

            // Look for the name in the connectionStrings section.
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["ExpatriateConnectString"];

            // If found, return the connection string.
            if (settings != null)
                returnValue = settings.ConnectionString;

            return returnValue;
        }

        // Retrieves a connection string by name.
        // Returns null if the name is not found.
        public static int GetPagePeriod()
        {
            // Assume failure.
            string returnValue = null;

            // Look for the name in the connectionStrings section.
            returnValue = 
                ConfigurationManager.AppSettings["PagePeriod"];           

            return int.Parse(returnValue);
        }

        public static string cfDecodeEmail(string encodedString)
        {
            string email = "";
            int r = Convert.ToInt32(encodedString.Substring(0, 2), 16), n, i;
            for (n = 2; encodedString.Length - n > 0; n += 2)
            {
                i = Convert.ToInt32(encodedString.Substring(n, 2), 16) ^ r;
                char character = (char)i;
                email += Convert.ToString(character);
            }

            return email;
        }

        public static bool isValidEmail(string inputEmail)
        {
            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
          @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
          @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }
    }
}
