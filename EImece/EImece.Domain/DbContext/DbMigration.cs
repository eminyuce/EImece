using EImece.Domain.Helpers;
using EImece.Domain.Models.MigrationModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace EImece.Domain.DbContext
{
    public class DbMigration
    {
        public static List<ProductImageExternalUrl> GetProductImages(string connectionString)
        {
            var result = new List<ProductImageExternalUrl>();
            string commandText = "[ewsiste].[ProductImageExternalUrl]";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.StoredProcedure;

            DataSet dataSet = DatabaseUtility.ExecuteDataSet(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray());

            if (dataSet.Tables.Count > 0)
            {
                using (DataTable dt = dataSet.Tables[0])
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var productImage = GetProductImageFromDataRow(dr);
                        result.Add(productImage);
                    }
                }
            }

            return result;
        }


        string prodConnectionString = "Data Source=mssql04.trwww.com;Initial Catalog=yuva8905_yuvadan;User ID=;Password=";
        string devConnectionString = @"Data Source=YUCE\SQLEXPRESS;Initial Catalog=yuva8905_yuvadan;Integrated Security=True";

        public void updateProductDescription()
        {
            Dictionary<int, Tuple<string, string>> devProducts = GetProducts(devConnectionString);
            foreach (var kvp in devProducts)
            {
                int productId = kvp.Key;
                string name = kvp.Value.Item1;
                string htmlDescription = kvp.Value.Item2;
                string commandText = "UPDATE [Products] SET Description = @htmlDescription WHERE Name = @Name ";
                var parameterList = new List<SqlParameter>
                {
                    DatabaseUtility.GetSqlParameter("Name", name, SqlDbType.NVarChar),
                    DatabaseUtility.GetSqlParameter("htmlDescription", htmlDescription, SqlDbType.NVarChar)
                };
                DatabaseUtility.ExecuteNonQuery(new SqlConnection(prodConnectionString), commandText, CommandType.Text, parameterList.ToArray());
                Console.WriteLine("Product Name:" + name + " is updated by UPDATE statement");
            }
        }

        private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private const string GROQ_API_KEY = "";
        //private const string GROQ_MODEL = "meta-llama/llama-4-scout-17b-16e-instruct";
        //private const string GROQ_MODEL = "meta-llama/Llama-Guard-4-12B";
        private const string GROQ_MODEL = "deepseek-r1-distill-llama-70b";

        public void ProcessProductWithGroq()
        {
            Dictionary<int, Tuple<string, string>> products = GetProducts(prodConnectionString);

            foreach (var kvp in products)
            {
                int productId = kvp.Key;
                string name = kvp.Value.Item1;
                string htmlDescription = kvp.Value.Item2;

                try
                {
                    string json = CallGroqSync(name, htmlDescription);
                    Console.WriteLine("ProductId:" + productId + " is processed by LLM");
                    InsertProductWithTags(productId, json, prodConnectionString);
                    Console.WriteLine("ProductId:" + productId + " is updated by Stored Proc");
                    Thread.Sleep(2000);
                    Console.WriteLine("----------------------------------------------");
                }
                catch (Exception ex)
                {
                    // Log error or handle gracefully
                    Console.WriteLine($"Error processing product {productId}: {ex.Message}");
                }
            }
        }
        public string CallGroqSync(string productName, string htmlDescription)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(productName))
            {
                throw new ArgumentException("Product name cannot be null or empty.", nameof(productName));
            }
            if (string.IsNullOrWhiteSpace(htmlDescription))
            {
                throw new ArgumentException("HTML description cannot be null or empty.", nameof(htmlDescription));
            }

            string prompt = $@"
You are a helpful assistant that generates SEO-optimized e-commerce product descriptions in Turkish.
Your response must be a valid JSON object and nothing else — no extra text, explanation, or formatting.
The JSON object should have the following structure:
{{
  ""ProductName"": ""{productName}"",
  ""Short_Description"": """",
  ""Description"": """",
  ""Meta_Keywords"": """",
  ""Product_Code"": """",
  ""Product_keywords"": """"
}}

Here's the product information:
Product Name: {productName}
Original HTML Description: {htmlDescription}

Your responsibilities:
- Generate a concise **Short_Description** (100-150 words) in Turkish based on the original HTML description content. This should be a plain text summary.
- Improve and optimize the original HTML description content for SEO in Turkish. This improved HTML content should be placed in the **Description** field with SEO Optimization, **Description** field is HTML content.
    - Remove any external links from the HTML content.
    - Enhance SEO with relevant keywords.
    - Make it a more effective and engaging product description.
    - Ensure the text flows naturally and is grammatically correct in Turkish.
- Generate a list of relevant **Meta_Keywords** (e.g., for meta tags) in Turkish based on the improved content. Return them as a comma-separated string.
- Generate a unique **Product_Code** based on the product name. If the product name contains numbers or specific identifiers, incorporate them. Otherwise, create a concise, relevant code.
- Generate a list of relevant **Product_keywords** (e.g., for internal search or tagging) in Turkish based on the improved content. Return them as a comma-separated string (e.g., ""keyword1,keyword2,keyword3"").

Constraints:
- Do not include any definitive or medical claims.
- Avoid terms like “100% natural,” “healing,” “miracle,” or similar exaggerated expressions.
- All generated content (Short_Description, Description, Meta_Keywords, Product_Code, Product_keywords) must be in Turkish.
- Return only the final JSON result. Do not include markdown code blocks (e.g., ```json), explanations, or any other text outside the JSON object.
";

            var requestBody = new
            {
                model = GROQ_MODEL,
                messages = new[]
                {
                new { role = "system", content = "You are a Turkish e-commerce product description expert. Return only valid JSON." },
                new { role = "user", content = prompt }
            },
                temperature = 0.7,
                max_tokens = 1500
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GROQ_API_KEY}");

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Synchronously wait for the HTTP POST to complete
                HttpResponseMessage response = client.PostAsync(GROQ_API_URL, content).Result;

                // Synchronously wait for the response content to be read
                string responseText = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"GROQ API Error: {response.StatusCode} - {responseText}");
                }

                // Deserialize the entire response
                dynamic parsedResponse = JsonConvert.DeserializeObject(responseText);

                // Access the content of the assistant's message
                string assistantContent = parsedResponse?.choices?[0]?.message?.content?.ToString();

                if (string.IsNullOrWhiteSpace(assistantContent))
                {
                    throw new Exception("GROQ API did not return any content.");
                }

                // Remove markdown if present (e.g., ```json)
                if (assistantContent.StartsWith("```json") && assistantContent.EndsWith("```"))
                {
                    assistantContent = assistantContent.Substring(7, assistantContent.Length - 10).Trim();
                }
                else if (assistantContent.StartsWith("```") && assistantContent.EndsWith("```"))
                {
                    assistantContent = assistantContent.Substring(3, assistantContent.Length - 6).Trim();
                }

                // Validate that the returned content is indeed a valid JSON object
                try
                {
                    JsonConvert.DeserializeObject(assistantContent);
                }
                catch (JsonException ex)
                {
                    throw new Exception($"GROQ API returned invalid JSON: {assistantContent}", ex);
                }

                return assistantContent;
            }
        }

        public string CallGroqRestClient(string productName, string htmlDescription)
        {
            string prompt = $@"
You are a helpful assistant that must respond only with a valid JSON object and nothing else — no extra text, explanation, or formatting.
Your task is to generate a JSON object with the following structure:
{{
  ""ProductName"": ""{productName}"",
  ""Short_Description"": ""Generate short description (100-150 words) based on original HTML description content"",
  ""Description"": ""Improved and Better HTML content with SEO Optimization in Turkish"",
  ""Meta_Keywords"": ""Generate Meta Keywords"",
  ""Product_Code"": ""Generate Product Code"",
  ""Product_keywords"": ""keyword1,keyword2,keyword3""
}}

Input:
{productName}: The name of the product.
{htmlDescription}: The original HTML description content.

Your responsibilities:
- Remove any external link of the product in html content
- Improve and Make Better HTML content with SEO Optimization based on the original HTML description content, 
- Improve the provided HTML description to:
    - Enhance SEO with relevant keywords.
    - Make it a more effective and engaging product description.
    - Ensure the text flows naturally in Turkish.
    - Generate a list of relevant keywords based on the improved content. Return them as a comma-separated string.

Constraints:
- Do not include any definitive or medical claims.
- Avoid terms like “100% natural,” “healing,” “miracle,” or similar exaggerated expressions.
- All content must be in Turkish.
- Return only the final JSON result, no markdown, explanations, or other text.";

            var requestBody = new
            {
                model = GROQ_MODEL,
                messages = new[]
                {
            new { role = "system", content = "Türkçe bir e-ticaret ürün açıklaması uzmanısın. Yalnızca geçerli JSON döndür." },
            new { role = "user", content = prompt }
        },
                temperature = 0.7,
                max_tokens = 800
            };

            var client = new RestClient(GROQ_API_URL);
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", $"Bearer {GROQ_API_KEY}");
            request.AddHeader("Content-Type", "application/json");

            string jsonRequest = JsonConvert.SerializeObject(requestBody);
            request.AddJsonBody(jsonRequest);

            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw new Exception($"GROQ API Error: {response.StatusCode} - {response.Content}");
            }

            dynamic parsed = JsonConvert.DeserializeObject(response.Content);
            string assistantContent = parsed.choices[0].message.content.ToString();

            assistantContent = assistantContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            return assistantContent;
        }

        public static void InsertProductWithTags(int productId, string productJson, string connectionString)
        {
            string commandText = "[dbo].[InsertProductWithTags]";
            var parameterList = new List<SqlParameter>
    {
        DatabaseUtility.GetSqlParameter("ProductId", productId, SqlDbType.Int),
        DatabaseUtility.GetSqlParameter("ProductJson", productJson, SqlDbType.NVarChar)
    };

            DatabaseUtility.ExecuteScalar(new SqlConnection(connectionString), commandText, CommandType.StoredProcedure, parameterList.ToArray());
        }

        public static Dictionary<int,Tuple<string, string>> GetProducts(string connectionString)
        {
            var result = new Dictionary<int, Tuple<string, string>>();
            string commandText = "SELECT Id,Name, Description FROM [dbo].[Products] where Id<>176367";
            var parameterList = new List<SqlParameter>();
            DataSet dataSet = DatabaseUtility.ExecuteDataSet(new SqlConnection(connectionString), commandText, CommandType.Text, parameterList.ToArray());

            if (dataSet.Tables.Count > 0)
            {
                DataTable dt = dataSet.Tables[0];
                foreach (DataRow dr in dt.Rows)
                {
                    int productId = dr["Id"].ToInt();
                    string name = dr["Name"].ToString();
                    string description = dr["Description"].ToString();
                    result.Add(productId, new Tuple<string,string>(name,description));
                }
            }

            return result;
        }


        private static ProductImageExternalUrl GetProductImageFromDataRow(DataRow dr)
        {
            return new ProductImageExternalUrl
            {
                ProductId = dr["ProductId"].ToInt(),
                ProductName = dr["ProductName"].ToStr(),
                ImageFullPath = dr["ImageFullPath"].ToStr(),
                EntityImageType = dr["EntityImageType"].ToStr()
            };
        }

        public static EntityImage GetImages(String connectionString)
        {
            var result = new EntityImage();
            String commandText = @"GetImages";
            var parameterList = new List<SqlParameter>();
            var commandType = CommandType.StoredProcedure;
            DataSet dataSet = DatabaseUtility.ExecuteDataSet(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray());
            if (dataSet.Tables.Count > 0)
            {
                result.EntityMainImages = new List<EntityMainImage>();
                using (DataTable dt = dataSet.Tables[0])
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var e = GetEntityMainImageFromDataRow(dr);
                        result.EntityMainImages.Add(e);
                    }
                }
                result.EntityMediaFiles = new List<EntityMediaFile>();
                using (DataTable dt = dataSet.Tables[1])
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var e = GetEntityMediaFileFromDataRow(dr);
                        result.EntityMediaFiles.Add(e);
                    }
                }
            }
            return result;
        }

        public static void InsertProductImages(string connectionString, ProductImageExternalUrl entityMainImage, Entities.FileStorage image)
        {
            var commandText = @"[ewsiste].[InsertProductImage]";
            var parameterList = new List<SqlParameter>();
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductId", entityMainImage.ProductId, SqlDbType.Int));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ProductName", entityMainImage.ProductName, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("EntityImageType", entityMainImage.EntityImageType, SqlDbType.NVarChar));
            parameterList.Add(DatabaseUtility.GetSqlParameter("ImageId", image.Id, SqlDbType.Int));
            var commandType = CommandType.StoredProcedure;
            DatabaseUtility.ExecuteScalar(new SqlConnection(connectionString), commandText, commandType, parameterList.ToArray());
        }

        private static EntityMediaFile GetEntityMediaFileFromDataRow(DataRow dr)
        {
            var item = new EntityMediaFile();
            item.CategoryName = dr["CategoryName"].ToStr();
            item.File_Type = dr["File_Type"].ToStr();
            item.Modul_Name = dr["Modul_Name"].ToStr();
            item.Mod = dr["Mod"].ToStr();
            item.Name = dr["Name"].ToStr();
            item.File_Path = dr["File_Path"].ToStr();
            item.File_Name = dr["File_Name"].ToStr();
            item.File_Desc = dr["File_Desc"].ToStr();
            item.File_Format = dr["File_Format"].ToStr();

            return item;
        }

        private static EntityMainImage GetEntityMainImageFromDataRow(DataRow dr)
        {
            var item = new EntityMainImage();

            item.EntityImageType = dr["EntityImageType"].ToStr();
            item.ImagePath = dr["ImagePath"].ToStr();
            item.ImagePath2 = dr["ImagePath2"].ToStr();
            item.Name = dr["Name"].ToStr();
            item.CategoryName = dr["CategoryName"].ToStr();
            return item;
        }
    }
}