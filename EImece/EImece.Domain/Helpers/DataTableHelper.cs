using EImece.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers
{
    public static class DataTableHelper
    {
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
    }
}
