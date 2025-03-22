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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

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
            generateImagesForAndroidAndFaviconWithDifferentSize();

            decimal productPrice = 120m;
            Console.WriteLine(productPrice.CurrencySign());
            productPrice = 133.5m;
            Console.WriteLine(productPrice.CurrencySign());
            Console.Read();

           
        }

        private static void generateImagesForAndroidAndFaviconWithDifferentSize()
        {
            // Path to your input JPEG file (update this to your actual file path)
            string inputImagePath = @"C:\Users\YUCE\Downloads\zeytinHanim\kudretNari.jpeg"; // Replace with your image path
            string outputDirectory = @"C:\Users\YUCE\Downloads\zeytinHanim\kudretNariIcons\";
            // string outputDirectory = Path.GetDirectoryName(inputImagePath) ?? Directory.GetCurrentDirectory();

            // Ensure the input file exists
            if (!File.Exists(inputImagePath))
            {
                Console.WriteLine("Input image not found. Please provide a valid JPEG file path.");
                return;
            }

            // List of sizes and corresponding filenames
            var imageSizes = new[]
            {
                ("android-chrome-36x36.png", 36, 36),
                ("android-chrome-48x48.png", 48, 48),
                ("android-chrome-72x72.png", 72, 72),
                ("android-chrome-96x96.png", 96, 96),
                ("android-chrome-144x144.png", 144, 144),
                ("android-chrome-192x192.png", 192, 192),
                ("android-chrome-256x256.png", 256, 256),
                ("android-chrome-384x384.png", 384, 384),
                ("android-chrome-512x512.png", 512, 512),
                ("apple-touch-icon.png", 180, 180), // Common size for apple-touch-icon
                ("favicon-16x16.png", 16, 16),
                ("favicon-32x32.png", 32, 32),
                ("favicon.ico", 32, 32) // ICO typically 32x32, though can support multiple sizes
            };

            try
            {
                // Load the original image
                using (Image originalImage = Image.FromFile(inputImagePath))
                {
                    foreach (var (fileName, width, height) in imageSizes)
                    {
                        string outputPath = Path.Combine(outputDirectory, fileName);

                        // Create a new bitmap with the specified size
                        using (Bitmap resizedImage = new Bitmap(originalImage, new Size(width, height)))
                        {
                            // For .ico files, save in ICO format; otherwise, use PNG
                            if (fileName.EndsWith(".ico"))
                            {
                                resizedImage.Save(outputPath, ImageFormat.Icon);
                            }
                            else
                            {
                                resizedImage.Save(outputPath, ImageFormat.Png);
                            }
                            Console.WriteLine($"Generated: {fileName} ({width}x{height})");
                        }
                    }
                }
                Console.WriteLine("All images generated successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating images: {ex.Message}");
            }
    }

    private static void ReplaceFileContent()
        {
            var parent = new DirectoryInfo(@"C:\Users\YUCE\Documents\GitHub\EImece\EImece\EImece\Views");

            foreach (var child in parent.GetDirectories())
            {
                if (child.GetDirectories() != null)
                {
                    foreach (var child2 in child.GetDirectories())
                    {
                        if (child2.GetDirectories() != null)
                        {
                            foreach (var child4 in child2.GetDirectories())
                            {
                                ChangeFileContent(child4);
                            }
                        }
                        else
                        {
                            ChangeFileContent(child2);
                        }
                    }
                }
                else
                {
                    ChangeFileContent(child);
                }
          
            }
        }

        private static void ChangeFileContent(DirectoryInfo child)
        {
            var newName = child.FullName;
            foreach (var f in Directory.GetFiles(child.FullName))
            {
                if (f.Contains("2020-") || f.Contains("2021-"))
                {
                    continue;
                }
                else
                {
                    var fileContent = File.ReadAllText(f);
                    if (fileContent.Contains("AdminResource"))
                    {
                        String newFileContent = fileContent.Replace("AdminResource", "Resource");
                        //File.WriteAllText(f, newFileContent);
                        Console.WriteLine("f:" + f);
                    }
                }
            }
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