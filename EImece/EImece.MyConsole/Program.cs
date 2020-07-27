using EImece.Domain.Helpers;
using System;
using System.Data;
using System.IO;
using System.Linq;

namespace EImece.MyConsole
{
    internal class Program
    {
        private static String connectionString = @"";

        private static void Main(string[] args)

        {
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine(GeneralHelper.RandomNumber(12).ToUpper());
            }
            Console.ReadLine();
        }

        private static void NewMethod33()
        {
            var filePath = @"D:\Projects\Temp\hubspot-contacts-everyone-2018-01-17\contacts.xlsx";
            var dt = ExcelHelper.ExcelToDataTable(filePath, "contacts$");
            dt.TableName = "Atcom_Contact_201801_5";
            ExcelHelper.SaveTable(dt, @"data source=devsqlserver;Integrated Security=SSPI;Initial Catalog=TestEY");
        }

        private static void NewMethod()
        {
            //ImportCSVFiles();
            string path = @"C:\Users\Yuce\Desktop\Atomic E-mail";
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var dt = ExcelHelper.ExcelToDataTable(f, ExcelHelper.GetWorkSheets(f).FirstOrDefault());
                Console.WriteLine(dt.Rows.Count);
            }
            System.Console.ReadLine();
        }

        private static void ImportCSVFiles()
        {
            string csvFilePath = @"C:\Users\Yuce\Desktop\GeoDatabase\GeoIP-134_20180515\GeoIPCity-134-Blocks.csv";
            string primaryKeyColumnName = "";
            String tableName = "geo18_IPCityBlocks";

            // ExcelHelper.ImportCVSFileToDatabase(connectionStringM, path, primaryKeyColumnName, tableName);
            //  var csvData = File.ReadAllText(csvFilePath);
            System.Console.WriteLine("File is read.");
            // var reader = new CSVReader(csvData);
            System.Console.WriteLine("CSVReader read text");
            DataTable dt = DataTableHelper.ConvertCSVtoDataTable(csvFilePath);
            System.Console.WriteLine("DataTable is created.");
            if (!String.IsNullOrEmpty(primaryKeyColumnName))
            {
                dt.PrimaryKey = new DataColumn[] { dt.Columns[primaryKeyColumnName] };
            }
            dt.TableName = tableName;
            // var sqlCreate = DataTableHelper.GetCreateTableSql(dt);
            // Console.WriteLine(sqlCreate);
            // ExcelHelper.ExecuteSqlCommand(connectionString, sqlCreate);
            System.Console.WriteLine("DataTable is begun saving." + dt.Rows.Count);
            ExcelHelper.SaveTable(dt, connectionString);
            System.Console.WriteLine("DataTable is done.Press any key to exit.");
            System.Console.ReadLine();
        }
    }
}