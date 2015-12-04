using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{
    class FileManager
    {
        static System.IO.StreamWriter streamWriter = null;
        static bool IsNumber(string src)
        {
            foreach (char ch in src)
            {
                if (!Char.IsDigit(ch))
                {
                    return false;
                }
            }
            return true;
        }
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
        public void WriteLog(string log)
        {
            if( streamWriter != null )
            {
                streamWriter.WriteLine(log);
                streamWriter.FlushAsync();
            }
        }
        public FileManager()
        {

        }
        public bool ImportToDb( string fullName )
        {
            DbManager dbManager = new DbManager();
            if( dbManager.Init() == false)
            {
                WriteLog("DbManager initialize failed.!");
                return false;
            }

            // 파일 읽기
            string filename = System.IO.Path.GetFileName(fullName);
            string[] lines = System.IO.File.ReadAllLines(fullName, Encoding.Default);
            int curLine = 0;

            WriteLog(fullName);

            while (curLine < lines.Length)
            {
                if (lines[curLine][0] != '/' && lines[curLine][1] != '/')
                {
                    break;
                }
                ++curLine;
            }

            string[] headers = lines[curLine].Split(',');

            // 1. 테이블명 생성
            string[] names = filename.Split('.');
            string tableName = "";
            for (int i = 0; i < names.Length - 1; ++i)
            {
                tableName += names[i] + "_";
            }
            tableName += names[names.Length - 1];

            // 2. 각 컬럼의 타입을 설정한다.
            // 2-1. 기본 타입
            string strTypes = "";
            for (int i = 0; i < headers.Length; ++i)
            {
                if (strTypes.Length != 0)
                {
                    strTypes += ",";
                }
                strTypes += "bigint";
            }
            string[] types = strTypes.Split(',');
            // 2-2. 문자열 타입
            for (int i = curLine + 1; i < lines.Length; ++i)
            {
                if (lines[i][0] != '/' || lines[i][1] != '/')
                {
                    string[] datas = lines[i].Split(',');

                    for (int j = 0; j < datas.Length; ++j)
                    {
                        if (types[j].Equals("bigint") || types.Equals("float"))
                        {
                            if (!IsNumber(datas[j]))
                            {
                                types[j] = "nvarchar(200)";
                            }
                            else if (datas[j].IndexOf(".") != -1)
                            {
                                types[j] = "float";
                            }
                        }
                    }
                }
            }

            // 컬럼 type이 int인 항목에서 인덱스가 필요한 컬럼을 검출한다.
            string[] indexes = { "", "", "" };
            int index = 0;
            for (int i = 0; i < headers.Length; ++i)
            {
                if (headers[i].ToLower().IndexOf("index") != -1)
                {
                    indexes[index++] = headers[i];
                }
                if (index == indexes.Length)
                {
                    break;
                }
            }
            // 인덱스와 타입을 종합해서 테이블을 만든다.

            string strCmd = "IF EXISTS ( SELECT * FROM sys.tables WHERE name = '" + tableName + "')" + "DROP TABLE [" + tableName + "] Create table [" + tableName + "] (";

            for (int i = 0; i < headers.Length; ++i)
            {
                string header = "Unknown_" + i.ToString();
                if (headers[i].Length > 0)
                {
                    header = headers[i];
                }
                if (i != 0)
                {
                    strCmd += ",";
                }

                strCmd += "[" + header + "] " + types[i];
            }

            strCmd += ")";


            if( false == dbManager.Query(strCmd) )
            {
                WriteLog("failed : " + strCmd);
                return false;
            }
               
            // index key를 추가한다.
            if (index > 0)
            {
                for (int i = 0; i < index; ++i)
                {
                    strCmd = "CREATE NONCLUSTERED INDEX [IX_" + tableName + indexes[i] + "] ON [dbo].[" + tableName + "] ( [" + indexes[i]
                        + "] ASC )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]";
                    if (false == dbManager.Query(strCmd))
                    {
                        WriteLog("failed : " + strCmd);
                        return false;
                    }
                }

            }
            
            string data;
            for (int i = curLine + 1; i < lines.Length; ++i)
            {
                if (lines[i][0] != '/' || lines[i][1] != '/')
                {
                    bool first2 = true;
                    string[] datas = lines[i].Split(',');
                    string squery = "INSERT INTO [" + tableName + "] VALUES (";
                    for (int j = 0; j < datas.Length; ++j)
                    {
                        data = datas[j].Replace('\'', '\"');
                        if (first2 == true)
                        {
                            squery += "'" + data + "'";
                            first2 = false;
                        }
                        else
                        {
                            squery += ",'" + data + "'";
                        }
                    }
                    squery += ")";

                    if (false == dbManager.Query(squery))
                    {
                        WriteLog("failed : " + squery);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
