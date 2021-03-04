using EImece.Domain;
using EImece.Domain.Helpers;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EImece.MyConsole
{
    internal class Program
    {
        private const string KEYID = "9182736450";
        private const string ALGOKEY = "EMINYUCEOM";
        private const string ALPHABETS = "abcdefghijklmnopqrstuvwxyz";
        private static String connectionString = @"";
       
        private static void Main(string[] args)
        {

            int number = 7199;
            Console.WriteLine(System.Convert.ToDecimal(number).ToString("#,##"));

            decimal totalPrice =69000m;
            double totalPrice2 = 7199;
            Console.WriteLine(totalPrice2.CurrencySign());
            Console.WriteLine(totalPrice.ToString("0.0", CultureInfo.GetCultureInfo(Constants.EN_US_CULTURE_INFO)));

            Console.WriteLine(decimal.Round(totalPrice, 2, MidpointRounding.AwayFromZero) + "");
            int id = 13769;
            string priceStr = "329,90".Replace(",",".");
            Console.WriteLine(priceStr.ToDouble());
            Console.WriteLine((decimal)priceStr.ToDouble());

            // 0J6G4H2D1B
            var result = GeneralHelper.ModifyId(id);
            Console.WriteLine(result);
            var id33= GeneralHelper.RevertId(result);
            Console.WriteLine(id33);
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }
 
        static public string Encode(string source, int length)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(source);

            var buffer = new StringBuilder(length);
            buffer.Append(System.Convert.ToBase64String(bytes));
            while (buffer.Length < length)
            {
                buffer.Append('=');
            }
            return buffer.ToString();
        }

        static public string Decode(string encoded)
        {
            int index = encoded.IndexOf('=');
            if (index > 0)
            {
                encoded = encoded.Substring(0, ((index + 3) / 4) * 4);
            }
            byte[] bytes = System.Convert.FromBase64String(encoded);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private static void NewMethod1()
        {
            string orderGuid = "fb348ba3-a29c-4824-94f2-8677be8b40ca";
            string userId = "3bbd5c72-4e35-44a7-9eff-b5b8ff9b6c86";

            string o = HttpUtility.UrlEncode(EncryptDecryptQueryString.Encrypt(orderGuid));
            Console.WriteLine(o);
            Console.WriteLine(EncryptDecryptQueryString.Decrypt(HttpUtility.UrlDecode(o)));

            var PaidPrice = "435.5";
            Console.WriteLine(PaidPrice.ToDecimal());
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