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
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Error : invalid file or directory.!");
                Console.ReadKey();
                return;
            }
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
                Console.ReadLine();
                return;
            }
            
            // 파일이면 해당 파일의 리스트를 읽어서 사용한다.            
            FileManager.Init(System.IO.Path.GetDirectoryName(listFileName) + "\\error.log");
            
            Parallel.For(0, dataFiles.Length, (i) =>
           {
               FileManager fileManager = new FileManager();
               fileManager.ImportToDb(dataFiles[i]);
           });

            FileManager.Release();

            sw.Stop();
            Console.WriteLine("수행시간 : {0}", sw.ElapsedMilliseconds / 1000.0f);
            Console.WriteLine(" 수행시간(ToString()) :" + sw.ToString());
            Console.ReadLine();
        }
    }
}
