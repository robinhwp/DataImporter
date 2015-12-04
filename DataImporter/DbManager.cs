using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace DataImporter
{
    class DbManager
    {
        static string connString = "";
        public static void SetConnString( string conn )
        {
            connString = conn;
        }
        

        SqlConnection m_conn = null;

        public bool Init()
        {

            m_conn = new SqlConnection( connString );
            try
            {
                m_conn.Open();
            }
            catch( Exception )
            {
                return false;
            }

            return true;
        }
        ~DbManager()
        {
            if( m_conn != null )
            {
                m_conn.Close();
            }
        }

        public bool Query( string sentens )
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sentens, m_conn)) cmd.ExecuteNonQuery();
                return true;
            }
            catch ( Exception )
            {
                return false;
            }
        }

    }
}
