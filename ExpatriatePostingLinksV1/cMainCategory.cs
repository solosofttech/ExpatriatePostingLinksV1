using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ExpatriatePostingLinksV1
{
    class cMainCategory
    {
        public static DataTable GetMainCategories()
        {
            try
            {
                   var _sqlconn = new SqlConnection(cMain.GetConnectionString());
                    string _commandText = "Select * from tblMainCategory";
                    var _sqlda = new SqlDataAdapter(_commandText, _sqlconn);
                    var _datatable = new DataTable();
                    _sqlda.Fill(_datatable);
                    return _datatable;
                
            }
            catch { return null; }
        }
    }
}
