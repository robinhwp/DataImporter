using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{
    class Program
    {
        // functions
        static bool IsNumber( string src )
        {
            foreach( char ch in src )
            {
                if( !Char.IsDigit( ch ))
                {
                    return false;
                }
            }
            return true;
        }

        static void Main(string[] args)
        {
            // argument가 directory면 파일을 만들고
            string listFileName = "";
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(args[0]);
            if( dirInfo.Exists )
            {
                listFileName = args[0] + "\\data_table.lst";
                System.IO.StreamWriter listWriter = new System.IO.StreamWriter(listFileName);
                foreach ( System.IO.FileInfo fileInfo in dirInfo.GetFiles())
                {
                    if(fileInfo.Extension == ".csv" )
                    {
                        listWriter.WriteLine(fileInfo.FullName);
                    }
                }
                listWriter.Close();
            }
            else
            {
                listFileName = args[0];
            }

            // 시간 측정
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();

            string[] dataFiles = System.IO.File.ReadAllLines(listFileName, Encoding.Default);
            if( dataFiles.Length == 0)
            {
                Console.WriteLine("Error : invalid file or directory.!");
                return;
            }
            
            // 파일이면 해당 파일의 리스트를 읽어서 사용한다.            
            ManageFile.Init(System.IO.Path.GetDirectoryName(listFileName) + "\\error.log");
            
            Parallel.For(0, dataFiles.Length, (i) =>
           {
               ManageFile fileManager = new ManageFile();
               fileManager.ImportToDb(dataFiles[i]);
           });

            ManageFile.Release();

            sw.Stop();
            Console.WriteLine("수행시간 : {0}", sw.ElapsedMilliseconds / 1000.0f);
            Console.WriteLine(" 수행시간(ToString()) :" + sw.ToString());
            Console.ReadLine();
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//// db
//using System.Data.SqlClient;

//namespace tcsharp
//{
//    class Program
//    {
//        static System.IO.StreamWriter s_streamWriter = null;

//        static bool IsNumber(string myString)
//        {
//            foreach (char ch in myString)
//            {
//                if (!Char.IsDigit(ch))
//                {
//                    return false;
//                }
//            }
//            return true;
//        }

//        static void ImportToDb(string fullname)
//        {
//            string strConn = "Data Source=10.101.157.5,8522;Initial Catalog=Mu2Game_robintest;USER ID=sa;PASSWORD=webzen@2";
//            // 아래의 문자열은 어떻게 만들어야 할지 모르겠다 레지스트리에 넣으면 될지..

//            SqlConnection conn = new SqlConnection(strConn);
//            try
//            {
//                conn.Open();

//                // 파일 읽기
//                string filename = System.IO.Path.GetFileName(fullname);
//                string[] lines = System.IO.File.ReadAllLines(fullname, Encoding.Default);
//                int curLine = 0;

//                Program.s_streamWriter.WriteLine(fullname);

//                while (curLine < lines.Length)
//                {
//                    if (lines[curLine][0] != '/' && lines[curLine][1] != '/')
//                    {
//                        break;
//                    }
//                    ++curLine;
//                }

//                string[] headers = lines[curLine].Split(',');

//                // 1. 테이블명 생성
//                string[] names = filename.Split('.');
//                string tableName = "";
//                for (int i = 0; i < names.Length - 1; ++i)
//                {
//                    tableName += names[i] + "_";
//                }
//                tableName += names[names.Length - 1];

//                // 2. 각 컬럼의 타입을 설정한다.
//                // 2-1. 기본 타입
//                string strTypes = "";
//                for (int i = 0; i < headers.Length; ++i)
//                {
//                    if (strTypes.Length != 0)
//                    {
//                        strTypes += ",";
//                    }
//                    strTypes += "bigint";
//                }
//                string[] types = strTypes.Split(',');
//                // 2-2. 문자열 타입
//                for (int i = curLine + 1; i < lines.Length; ++i)
//                {
//                    if (lines[i][0] != '/' || lines[i][1] != '/')
//                    {
//                        string[] datas = lines[i].Split(',');

//                        for (int j = 0; j < datas.Length; ++j)
//                        {
//                            if (types[j].Equals("bigint") || types.Equals("float"))
//                            {
//                                if (!IsNumber(datas[j]))
//                                {
//                                    types[j] = "nvarchar(200)";
//                                }
//                                else if (datas[j].IndexOf(".") != -1)
//                                {
//                                    types[j] = "float";
//                                }
//                            }
//                        }
//                    }
//                }

//                // 컬럼 type이 int인 항목에서 인덱스가 필요한 컬럼을 검출한다.
//                string[] indexes = { "", "", "" };
//                int index = 0;
//                for (int i = 0; i < headers.Length; ++i)
//                {
//                    if (headers[i].ToLower().IndexOf("index") != -1)
//                    {
//                        indexes[index++] = headers[i];
//                    }
//                    if (index == indexes.Length)
//                    {
//                        break;
//                    }
//                }
//                // 인덱스와 타입을 종합해서 테이블을 만든다.

//                string strCmd = "IF EXISTS ( SELECT * FROM sys.tables WHERE name = '" + tableName + "')" + "DROP TABLE [" + tableName + "] Create table [" + tableName + "] (";

//                for (int i = 0; i < headers.Length; ++i)
//                {
//                    string header = "Unknown_" + i.ToString();
//                    if (headers[i].Length > 0)
//                    {
//                        header = headers[i];
//                    }
//                    if (i != 0)
//                    {
//                        strCmd += ",";
//                    }

//                    strCmd += "[" + header + "] " + types[i];
//                }

//                strCmd += ")";

//                try
//                {
//                    using (SqlCommand cmd = new SqlCommand(strCmd, conn)) cmd.ExecuteNonQuery();

//                    // index key를 추가한다.
//                    if (index > 0)
//                    {
//                        for (int i = 0; i < index; ++i)
//                        {
//                            strCmd = "CREATE NONCLUSTERED INDEX [IX_" + tableName + indexes[i] + "] ON [dbo].[" + tableName + "] ( [" + indexes[i]
//                                + "] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";
//                            using (SqlCommand cmd = new SqlCommand(strCmd, conn)) cmd.ExecuteNonQuery();
//                        }

//                    }

//                }
//                catch (Exception ex)
//                {
//                    Program.s_streamWriter.WriteLine(strCmd);
//                }

//                string data;
//                for (int i = curLine + 1; i < lines.Length; ++i)
//                {
//                    if (lines[i][0] != '/' || lines[i][1] != '/')
//                    {
//                        bool first2 = true;
//                        string[] datas = lines[i].Split(',');
//                        string squery = "INSERT INTO [" + tableName + "] VALUES (";
//                        for (int j = 0; j < datas.Length; ++j)
//                        {
//                            data = datas[j].Replace('\'', '\"');
//                            if (first2 == true)
//                            {
//                                squery += "'" + data + "'";
//                                first2 = false;
//                            }
//                            else
//                            {
//                                squery += ",'" + data + "'";
//                            }
//                        }
//                        squery += ")";

//                        try
//                        {
//                            using (SqlCommand query = new SqlCommand(squery, conn)) query.ExecuteNonQuery();
//                        }
//                        catch (Exception ex)
//                        {
//                            Program.s_streamWriter.WriteLine(squery);
//                        }
//                    }

//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.GetBaseException());
//            }
//            finally
//            {
//                if (conn != null)
//                {
//                    conn.Close();
//                }
//            }
//        }

//        static void Main(string[] args)
//        {
//            string[] files = null;
//            string listFile = "";
//            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(args[0]);
//            if (di.Exists)
//            {
//                listFile = args[0] + "\\data_table.lst";
//                // 디렉토리인 경우 디렉토리를 읽어서 파일 리스트를 만든다.
//                System.IO.StreamWriter writerList = new System.IO.StreamWriter(listFile);
//                foreach (System.IO.FileInfo file in di.GetFiles())
//                {
//                    if (file.Extension == ".csv")
//                    {
//                        writerList.WriteLine(file.FullName);
//                    }
//                }
//                writerList.Close();
//            }
//            else
//            {
//                listFile = args[0];
//            }

//            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
//            sw.Reset();

//            sw.Start();
//            // 파일인 경우 파일을 읽어서 사용한다.
//            files = System.IO.File.ReadAllLines(listFile, Encoding.Default);
//            if (files.Length == 0)
//            {
//                Console.WriteLine("error : Invalid file or Directory.");
//                return;
//            }
//            // 각각의 파일을 돌면서 처리한다.

//            string errorFileName = System.IO.Path.GetDirectoryName(listFile) + "\\error.lst";
//            //System.IO.File.Delete(errorFileName);

//            s_streamWriter = new System.IO.StreamWriter(errorFileName);

//            Parallel.For(0, files.Length, (i) =>
//            {
//                ImportToDb(files[i]);
//            });

//            s_streamWriter.Close();
//            //for (int i = 0; i < files.Length; ++ i )
//            //{
//            //    ImportToDb(files[i]);
//            //}

//            //
//            sw.Stop();
//            Console.WriteLine("수행시간:{0}", sw.ElapsedMilliseconds / 1000.0F);
//            Console.ReadLine();
//        }
//    }
//}
