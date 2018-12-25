using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;

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
    }
}
