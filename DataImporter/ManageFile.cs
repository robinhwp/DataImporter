using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{
    class ManageFile
    {
        static System.IO.StreamWriter streamWriter = null;
        public static void Init( string fullname )
        {
            streamWriter = new System.IO.StreamWriter(fullname);
        }
        public static void Release()
        {
            if(streamWriter != null)
            {
                streamWriter.Close();
            }
        }
        public void WriteError(string err)
        {
            if( streamWriter != null )
            {
                streamWriter.WriteLine(err);
                streamWriter.FlushAsync();
            }
        }
        public ManageFile()
        {

        }
        public void ImportToDb( string fullName )
        {

        }
    }
}
