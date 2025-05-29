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


        private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private const string GROQ_API_KEY = "";
        private const string GROQ_MODEL = "meta-llama/llama-4-scout-17b-16e-instruct";
        string prodConnectionString = "Data Source=mssql04.trwww.com;Initial Catalog=yuva8905_yuvadan;User ID=yuce; Password=";
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

        public void ProcessProductWithGroq()
        {
            Dictionary<int, Tuple<string, string>> products = GetProducts(prodConnectionString);

            foreach (var kvp in products)
            {
                int productId = kvp.Key;
                string name = kvp.Value.Item1;
                string htmlDescription = kvp.Value.Item2;
                string json = null;
                try
                {
                    json = CallGroqSyncHtml(name, htmlDescription).TrimEnd().TrimStart();
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

        public string CallGroqSyncHtml(string productName, string htmlDescription)
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

Product Information:
- Product Name: {productName}
- Original HTML Description: {htmlDescription}

Your response must be a valid JSON object and nothing else — no extra text, explanation, or formatting.

The JSON object must follow this structure exactly:
{{
  ""ProductName"": ""{productName}"",
  ""Description"": ""...""
}}

Instructions:
- Keep ProductName exactly as provided, but fix any casing or spacing issues.
- Rewrite the original HTML description into **well-structured, SEO-optimized HTML** in Turkish.
  - Avoid exaggerated or unverifiable claims.
  - Use clear language for Turkish e-commerce customers.
  - Use semantic HTML tags (<p>, <ul>, <li>, <strong>, etc.).
  - Remove external links or unnecessary tags.
  - Ensure output HTML is clean, valid, and production-ready.

Constraints:
- All content must be in Turkish.
- Do NOT return any explanation, markdown, or extra content — only valid JSON.
";

            var requestBody = new
            {
                model = GROQ_MODEL,
                messages = new[]
                {
            new {
                role = "system",
                content = "You are an expert SEO copywriter specialized in Turkish e-commerce. Always return a well-formatted JSON object with ProductName and Description only — no explanations or extra text."
            },
            new {
                role = "user",
                content = prompt
            }
        },
                temperature = 0.7,
                max_tokens = 1000
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {GROQ_API_KEY}");

                var jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = client.PostAsync(GROQ_API_URL, content).Result;
                var responseText = response.Content.ReadAsStringAsync().Result;

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"GROQ API Error: {response.StatusCode} - {responseText}");
                }

                dynamic parsedResponse = JsonConvert.DeserializeObject(responseText);
                string assistantContent = parsedResponse?.choices?[0]?.message?.content?.ToString();

                if (string.IsNullOrWhiteSpace(assistantContent))
                {
                    throw new Exception("GROQ API did not return any content.");
                }

                // Strip markdown if present
                if (assistantContent.StartsWith("```json") && assistantContent.EndsWith("```"))
                {
                    assistantContent = assistantContent.Substring(7, assistantContent.Length - 10).Trim();
                }
                else if (assistantContent.StartsWith("```") && assistantContent.EndsWith("```"))
                {
                    assistantContent = assistantContent.Substring(3, assistantContent.Length - 6).Trim();
                }

                // Validate the JSON
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
The JSON object must follow this structure:
{{
  ""ProductName"": ""{productName}"",
  ""Short_Description"": """",
  ""Description"": """",
  ""Meta_Keywords"": """",
  ""Product_Code"": """",
  ""Product_keywords"": """"
}}

Product Information:
- Product Name: {productName}
- Original HTML Description: {htmlDescription}

Your tasks:
1. **Short_Description**: Write a concise (100–150 words) summary of the product in **plain Turkish** based on the original description. Avoid HTML tags.
2. **Description**: Rewrite the original HTML description into **well-structured HTML** that is SEO-optimized and ready for publishing. Ensure the content:
   - Is written in fluent, natural Turkish.
   - Emphasizes product features and benefits without exaggeration.
   - Includes relevant SEO keywords naturally.
   - Removes any external links and unnecessary tags.
   - Uses semantic HTML tags (`<p>`, `<ul>`, `<li>`, `<strong>`, etc.) for structure and readability.
   - Is clean, valid HTML suitable for an e-commerce website.
   - Ensure the HTML is clean, well-formed, and ready to be used directly on an e-commerce website.
3. **Meta_Keywords**: Generate a comma-separated list of SEO-friendly keywords in Turkish based on the improved HTML description (e.g., for meta tags).
4. **Product_Code**: Create a short, unique identifier based on the product name.
    - If the product name contains numbers or specific identifiers, include them.
    - Use lowercase letters and separate words with a dash (`-`).
    - Remove special characters and accents.
    - The code should be short, relevant, and readable.
5. **Product_keywords**: Generate a comma-separated list of relevant internal search keywords in Turkish based on the improved content.
6. **ProductName**: If the provided ProductName contains typos, incorrect casing, or formatting issues, correct them. Ensure proper capitalization and spacing. Do not translate the product name — keep it in the original language.


Constraints:
- All content must be in Turkish.
- Avoid definitive or medical claims.
- Do not use exaggerated terms like “%100 doğal”, “mucize”, or “şifa”.
- Return **only** the final JSON — no markdown formatting, no explanations, and no extra text.
";
            ;

            var requestBody = new
            {
                model = GROQ_MODEL,
                messages = new[]
                {
                //system Role — Sets the Rules
                //Acts like the initial instruction or persona setup.
                //Establishes how the model should behave.
                //Think of it as setting the "mood" or "rules of the game."
                new { role = "system",
                    content = "You are an expert SEO copywriter specialized in Turkish e-commerce. Your job is to generate clear, engaging, and SEO-optimized product descriptions in Turkish. Always return a well-formatted JSON object exactly as specified, without any explanations, markdown, or extra text. All generated fields must be in Turkish and follow the instructions strictly.",
                },
                //user Role — Provides the Task
                //Delivers the actual prompt, data, or question.
                //Specifies what the model should do.
                //Includes all the requirements, input data, and expectations.
                new { role = "user", 
                    content = prompt }
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
            string commandText = "SELECT Id, Name, Description FROM [dbo].[Products] ";
            var parameterList = new List<SqlParameter>
    {
      //  new SqlParameter("@descPattern", SqlDbType.NVarChar) { Value = "%https://www.tlosolive.com%" }
    };
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