using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LumenWorks.Framework.IO.Csv;
using System.Data;
using System.IO;

namespace StandardDeviation
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //------Get the datatable defintion and column names-------
                DataTable csvTable = new DataTable();
                csvTable.Columns.Add("Depth", typeof(decimal));
                csvTable.Columns.Add("TVGR", typeof(decimal));
                csvTable.Columns.Add("TVRES", typeof(decimal));
                csvTable.Columns.Add("TVDT", typeof(decimal));

                //-----Output Data Table--------
                DataTable dtOutput = new DataTable();
                dtOutput.Columns.Add("Iteration", typeof(int));
                dtOutput.Columns.Add("StartDepth", typeof(decimal));
                dtOutput.Columns.Add("EndDepth", typeof(decimal));
                dtOutput.Columns.Add("Avg_TVGR", typeof(decimal));
                dtOutput.Columns.Add("Avg_TVRES", typeof(decimal));
                dtOutput.Columns.Add("Avg_TVDT", typeof(decimal));


                //----Read Datatable from CSV---------
                using (var csvReader = new CsvReader(new StreamReader(System.IO.File.OpenRead(@"input.csv")), true))
                {
                    csvTable.Load(csvReader);
                }

                //-----Read Start Depth, End Depth and Interval Gap-----
                Console.WriteLine("Enter Start Depth..");
                decimal startDepth = decimal.Parse(Console.ReadLine());
                Console.WriteLine("Enter End Depth..");
                decimal endDepth = decimal.Parse(Console.ReadLine());
                Console.WriteLine("Enter Interval Gap..");
                decimal intervalGap = decimal.Parse(Console.ReadLine());

                //-------Filter out only rows that will be needed..
                DataTable dtSubset = csvTable.Select("Depth >=" + startDepth + "AND Depth<=" + endDepth).CopyToDataTable();


                //-------Calculation
                decimal startPoint = startDepth;
                decimal endPoint = startPoint + intervalGap;
                int iteration = 1;
                while (startPoint < endDepth)
                {
                    DataTable dtFocusDataset = dtSubset.Select("Depth >=" + startPoint + "AND Depth<=" + endPoint).CopyToDataTable();
                    decimal avgTVGR = decimal.Parse(dtFocusDataset.Compute("Avg(TVGR)", "").ToString());
                    decimal avgTVRES = decimal.Parse(dtFocusDataset.Compute("Avg(TVRES)", "").ToString());
                    decimal avgTVDT = decimal.Parse(dtFocusDataset.Compute("Avg(TVDT)", "").ToString());

                    //---Write to output datatable:
                    dtOutput.Rows.Add(iteration, startPoint, endPoint, avgTVGR, avgTVRES, avgTVDT);

                    startPoint = endPoint;
                    endPoint = endPoint + intervalGap;
                    iteration++;
                }
                writeToCSV(dtOutput);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + "Error.Please contact developer!. \nPress any key to close app.");
                Console.ReadKey();
            }

        }

        private static void writeToCSV(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("Output.csv", sb.ToString());
        }
    }
}
