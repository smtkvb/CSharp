using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        //helper method for populating the datatable
        private static void addRow(DataTable dt, string notPivotedColumn, string columnName  ,  string value)
        {
            var row = dt.NewRow();
            row["notPivotedColumn"] = notPivotedColumn;
            row["colValue"] = columnName;
            row["value"] = value;
            dt.Rows.Add(row);
        }


        static void Main(string[] args)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("notPivotedColumn");
            dt.Columns.Add("colValue");
            dt.Columns.Add("value");

            addRow(dt, "1", "col1", "a");
            addRow(dt, "1", "col2", "b");
            addRow(dt, "2", "col1", "aa");
            addRow(dt, "2", "col2", "bb");

            DataTable dt2 = Pivot(dt, dt.Columns["colValue"], dt.Columns["value"]);

        }




        private static DataTable Pivot(DataTable dt, DataColumn pivotColumn, DataColumn pivotValue)
        {
            // find primary key columns 
            //(i.e. everything but pivot column and pivot value)
            DataTable temp = dt.Copy();
            temp.Columns.Remove(pivotColumn.ColumnName);
            temp.Columns.Remove(pivotValue.ColumnName);
            string[] pkColumnNames = temp.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToArray();
 
            // prep results table
            DataTable result = temp.DefaultView.ToTable(true, pkColumnNames).Copy();
            result.PrimaryKey = result.Columns.Cast<DataColumn>().ToArray();
            dt.AsEnumerable()
                .Select(r => r[pivotColumn.ColumnName].ToString())
                .Distinct().ToList()
                .ForEach(c => result.Columns.Add(c, pivotValue.DataType));
            //.ForEach(c => result.Columns.Add(c, pivotColumn.DataType));
 
            // load it
            foreach (DataRow row in dt.Rows)
            {
                // find row to update
                DataRow aggRow = result.Rows.Find(
                    pkColumnNames
                        .Select(c => row[c])
                        .ToArray());
                // the aggregate used here is LATEST 
                // adjust the next line if you want (SUM, MAX, etc...)
                aggRow[row[pivotColumn.ColumnName].ToString()] = row[pivotValue.ColumnName];
            }
 
            return result;
        }
    }
}
