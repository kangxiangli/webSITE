using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel;
using System.Reflection;

namespace Whiskey.Web.Helper
{
   public static class DatabaseHelper
    {
       public static void BulkInsert<T>(string connection, string tableName, IList<T> list)
       {
           using (var bulkCopy = new SqlBulkCopy(connection))
           {
               bulkCopy.BulkCopyTimeout = 1000000;
               bulkCopy.BatchSize = list.Count;
               bulkCopy.DestinationTableName = tableName;

               var table = new DataTable();
               var props = TypeDescriptor.GetProperties(typeof(T))
                                          .Cast<PropertyDescriptor>()
                                          .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                          .ToArray();

               foreach (var propertyInfo in props)
               {
                   bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                   table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
               }

               var values = new object[props.Length];
               foreach (var item in list)
               {
                   for (var i = 0; i < values.Length; i++)
                   {
                       values[i] = props[i].GetValue(item);
                   }

                   table.Rows.Add(values);
               }

               bulkCopy.WriteToServer(table);
           }
       }

    }
}
