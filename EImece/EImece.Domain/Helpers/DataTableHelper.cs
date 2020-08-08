using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EImece.Domain.Helpers
{
    public static class DataTableHelper
    {
        public static bool GetValueBoolean(DataRow row, string column, bool defaultValue)
        {
            return GetValue(row, column, defaultValue).ToBool();
        }

        public static string GetValueString(DataRow row, string column, string defaultValue)
        {
            return GetValue(row, column, defaultValue).ToStr();
        }

        public static int GetValueInt(DataRow row, string column, int defaultValue)
        {
            return GetValue(row, column, defaultValue).ToInt();
        }

        private static object GetValue(DataRow row, string column, object defaultValue)
        {
            try
            {
                return row.Table.Columns.Contains(column) ? row[column] : defaultValue;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return defaultValue;
            }
        }

        public static DataTable RemoveEmptyRows(DataTable dt)
        {
            DataTable filteredRows = dt.Rows.Cast<DataRow>()
.Where(row => !row.ItemArray.All(field => String.IsNullOrEmpty(field.ToString().Trim())))
.CopyToDataTable();

            return filteredRows;
        }

        public static T ConvertToEntity<T>(this DataRow tableRow) where T : new()
        {
            // Create a new type of the entity I want
            Type t = typeof(T);
            T returnObject = new T();

            foreach (DataColumn col in tableRow.Table.Columns)
            {
                string colName = col.ColumnName;

                // Look for the object's property with the columns name, ignore case
                PropertyInfo pInfo = t.GetProperty(colName.ToLower(),
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                // did we find the property ?
                if (pInfo != null)
                {
                    object val = tableRow[colName];

                    // is this a Nullable<> type
                    bool IsNullable = (Nullable.GetUnderlyingType(pInfo.PropertyType) != null);
                    if (IsNullable)
                    {
                        if (val is System.DBNull)
                        {
                            val = null;
                        }
                        else
                        {
                            // Convert the db type into the T we have in our Nullable<T> type
                            val = System.Convert.ChangeType
                    (val, Nullable.GetUnderlyingType(pInfo.PropertyType));
                        }
                    }
                    else
                    {
                        // Convert the db type into the type of the property in our entity
                        val = System.Convert.ChangeType(val, pInfo.PropertyType);
                    }
                    // Set the value of the property with the value from the db
                    pInfo.SetValue(returnObject, val, null);
                }
            }

            // return the entity object with values
            return returnObject;
        }

        public static List<T> ConvertToList<T>(this DataTable table) where T : new()
        {
            Type t = typeof(T);

            // Create a list of the entities we want to return
            List<T> returnObject = new List<T>();

            // Iterate through the DataTable's rows
            foreach (DataRow dr in table.Rows)
            {
                // Convert each row into an entity object and add to the list
                T newRow = dr.ConvertToEntity<T>();
                returnObject.Add(newRow);
            }

            // Return the finished list
            return returnObject;
        }

        public static DataTable ConvertToDataTable(this object obj)
        {
            // Retrieve the entities property info of all the properties
            PropertyInfo[] pInfos = obj.GetType().GetProperties();

            // Create the new DataTable
            var table = new DataTable();

            // Iterate on all the entities' properties
            foreach (PropertyInfo pInfo in pInfos)
            {
                // Create a column in the DataTable for the property
                table.Columns.Add(pInfo.Name, pInfo.GetType());
            }

            // Create a new row of values for this entity
            DataRow row = table.NewRow();
            // Iterate again on all the entities' properties
            foreach (PropertyInfo pInfo in pInfos)
            {
                // Copy the entities' property value into the DataRow
                row[pInfo.Name] = pInfo.GetValue(obj, null);
            }

            // Return the finished DataTable
            return table;
        }

        public static String ToPrintConsole(DataTable dataTable)
        {
            var sb = new StringBuilder();
            // Print top line
            Console.WriteLine(new string('-', 75));
            sb.AppendLine(new string('-', 1500));
            // Print col headers
            var colHeaders = dataTable.Columns.Cast<DataColumn>().Select(arg => arg.ColumnName);
            foreach (String s in colHeaders)
            {
                Console.Write("| {0,-20}", s);
                sb.Append(String.Format("| {0,-80}", s));
            }
            Console.WriteLine();
            sb.AppendLine();
            // Print line below col headers
            Console.WriteLine(new string('-', 75));
            sb.AppendLine(new string('-', 1500));
            // Print rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (Object o in row.ItemArray)
                {
                    Console.Write("| {0,-20}", o.ToString());
                    sb.Append(String.Format("| {0,-80}", o.ToString()));
                }
                Console.WriteLine();
                sb.AppendLine("");
            }

            // Print bottom line
            Console.WriteLine(new string('-', 75));
            sb.AppendLine(new string('-', 1500));

            return sb.ToString();
        }

        /// <summary>
        /// Inspects a DataTable and return a SQL string that can be used to CREATE a TABLE in SQL Server.
        /// </summary>
        /// <param name="table">System.Data.DataTable object to be inspected for building the SQL CREATE TABLE statement.</param>
        /// <returns>String of SQL</returns>
        public static string GetCreateTableSql(DataTable table)
        {
            StringBuilder sql = new StringBuilder();
            StringBuilder alterSql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", table.TableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
#pragma warning disable CS0219 // The variable 'isNumeric' is assigned but its value is never used
                bool isNumeric = false;
#pragma warning restore CS0219 // The variable 'isNumeric' is assigned but its value is never used
#pragma warning disable CS0219 // The variable 'usesColumnDefault' is assigned but its value is never used
                bool usesColumnDefault = true;
#pragma warning restore CS0219 // The variable 'usesColumnDefault' is assigned but its value is never used

                sql.AppendFormat("\n\t[{0}]", table.Columns[i].ColumnName);

                switch (table.Columns[i].DataType.ToString().ToUpper())
                {
                    case "SYSTEM.INT16":
                        sql.Append(" smallint");
                        isNumeric = true;
                        break;

                    case "SYSTEM.INT32":
                        sql.Append(" int");
                        isNumeric = true;
                        break;

                    case "SYSTEM.INT64":
                        sql.Append(" bigint");
                        isNumeric = true;
                        break;

                    case "SYSTEM.DATETIME":
                        sql.Append(" datetime");
                        usesColumnDefault = false;
                        break;

                    case "SYSTEM.STRING":
                        sql.AppendFormat(" nvarchar({0})", table.Columns[i].MaxLength != -1 ? table.Columns[i].MaxLength : 1000);
                        break;

                    case "SYSTEM.SINGLE":
                        sql.Append(" float");
                        isNumeric = true;
                        break;

                    case "SYSTEM.DOUBLE":
                        sql.Append(" double");
                        isNumeric = true;
                        break;

                    case "SYSTEM.DECIMAL":
                        sql.AppendFormat(" decimal(18, 6)");
                        isNumeric = true;
                        break;

                    default:
                        sql.AppendFormat(" nvarchar({0})", table.Columns[i].MaxLength);
                        break;
                }

                if (table.Columns[i].AutoIncrement)
                {
                    sql.AppendFormat(" IDENTITY({0},{1})",
                        table.Columns[i].AutoIncrementSeed,
                        table.Columns[i].AutoIncrementStep);
                }
                else
                {
                    // DataColumns will add a blank DefaultValue for any AutoIncrement column.
                    // We only want to create an ALTER statement for those columns that are not set to AutoIncrement.
                    //if (table.Columns[i].DefaultValue != null)
                    //{
                    //    if (usesColumnDefault)
                    //    {
                    //        if (isNumeric)
                    //        {
                    //            alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                    //                table.TableName,
                    //                table.Columns[i].ColumnName,
                    //                table.Columns[i].DefaultValue);
                    //        }
                    //        else
                    //        {
                    //            alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ('{2}') FOR [{1}];",
                    //                table.TableName,
                    //                table.Columns[i].ColumnName,
                    //                table.Columns[i].DefaultValue);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Default values on Date columns, e.g., "DateTime.Now" will not translate to SQL.
                    //        // This inspects the caption for a simple XML string to see if there is a SQL compliant default value, e.g., "GETDATE()".
                    //        try
                    //        {
                    //            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();

                    //            xml.LoadXml(table.Columns[i].Caption);

                    //            alterSql.AppendFormat("\nALTER TABLE {0} ADD CONSTRAINT [DF_{0}_{1}]  DEFAULT ({2}) FOR [{1}];",
                    //                table.TableName,
                    //                table.Columns[i].ColumnName,
                    //                xml.GetElementsByTagName("defaultValue")[0].InnerText);
                    //        }
                    //        catch
                    //        {
                    //            // Handle
                    //        }
                    //    }
                    //}
                }

                if (!table.Columns[i].AllowDBNull)
                {
                    sql.Append(" NOT NULL");
                }

                sql.Append(",");
            }

            if (table.PrimaryKey.Length > 0)
            {
                StringBuilder primaryKeySql = new StringBuilder();

                primaryKeySql.AppendFormat("\n\tCONSTRAINT PK_{0} PRIMARY KEY (", table.TableName);

                for (int i = 0; i < table.PrimaryKey.Length; i++)
                {
                    primaryKeySql.AppendFormat("{0},", table.PrimaryKey[i].ColumnName);
                }

                primaryKeySql.Remove(primaryKeySql.Length - 1, 1);
                primaryKeySql.Append(")");

                sql.Append(primaryKeySql);
            }
            else
            {
                sql.Remove(sql.Length - 1, 1);
            }

            sql.AppendFormat("\n);\n{0}", alterSql.ToString());

            return sql.ToString();
        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }
    }
}