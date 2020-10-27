using EImece.Domain.Helpers;
using System;
using System.Data;
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
        private const string ALPHABETS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static String connectionString = @"";
        public static string ReverseString(string srtVarable)
        {
            return new string(srtVarable.Reverse().ToArray());
        }
        private static void Main(string[] args)
        {
            int id = 13769;
            // 0J6G4H2D1B
            var result = GenerateId(id);
            var id33=GenerateReserveId(result);
            Console.WriteLine(id33);
            Console.Write("Press any key to continue . . . ");
            Console.ReadKey(true);
        }

        private static int GenerateReserveId(string result)
        {
            string result2 = new String(result.Where(Char.IsDigit).ToArray());

            var result3 = new StringBuilder();
            char[] result2Ids = result2.ToCharArray();
            char[] keyIds = KEYID.ToCharArray();
            for (int i = 0; i < result2Ids.Length; i++)
            {
                var nnn = result2Ids[i].ToInt();
                for (int j = 0; j < keyIds.Length; j++)
                {
                    if (nnn == keyIds[j].ToInt())
                        result3.Append(j);
                }
            }
            return ReverseString(result3.ToString()).ToInt();
        }

        private static string GenerateId(int id)
        {
            string result = "";
            char[] keyIds = KEYID.ToCharArray();
            char[] alpha = ALPHABETS.ToCharArray();
            char[] idCharacters = id.ToString().ToCharArray();
            for (int i = idCharacters.Length - 1; i >= 0; i--)
            {
                var num = idCharacters[i].ToInt();
                result += keyIds[num] + "" + alpha[num];
                Console.WriteLine(num);
            }
            Console.WriteLine(result);
            return result.ToLower();
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