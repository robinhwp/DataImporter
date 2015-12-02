using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace DataImporter
{
    class ManageDb
    {
        SqlConnection m_conn = null;

        bool Init( string info )
        {
            m_conn = new SqlConnection(info);
            try
            {
                m_conn.Open();
            }
            catch( Exception ex )
            {
                return false;
            }

            return true;
        }
    }
}
