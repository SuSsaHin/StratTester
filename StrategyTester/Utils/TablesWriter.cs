using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Office.Interop.Excel;

namespace StrategyTester.Utils
{
    public static class TablesWriter
    {
        public static void PrintExcel(string filename, List<string> headers, List<List<string>> table)
        {
            if (headers.Count != table.First().Count)
            {
                throw new Exception("Table size and headers size must be equal");
            }
            Application excelApp = new Application();
            var fullFileName = Path.GetFullPath(filename);
            var workBook = excelApp.Workbooks.Add(System.Reflection.Missing.Value);
            excelApp.Columns.HorizontalAlignment = Constants.xlCenter;

            for (int colNum = 1; colNum <= headers.Count; ++colNum)
            {
                excelApp.Cells[1, colNum] = headers[colNum-1];
            }

            int rowNum = 2;
            foreach (var row in table)
            {
                for (int colNum = 1; colNum <= row.Count; ++colNum)
                {
                    excelApp.Cells[rowNum, colNum] = row[colNum-1];
                }
                rowNum++;
            }

            excelApp.Columns.AutoFit();

            File.Delete(fullFileName);

            try
            {
                workBook.SaveAs(fullFileName);
            }
            finally 
            {
                workBook.Close(false);

                excelApp.Quit();

                workBook = null;
                excelApp = null;
                GC.Collect();
            }
        }
    }
}
