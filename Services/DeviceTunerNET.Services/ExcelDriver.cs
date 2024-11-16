using DeviceTunerNET.Services.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class ExcelDriver : ITablesManager
    {
        private FileInfo sourceFile;
        private ExcelPackage package;
        private ExcelWorksheet worksheet;

        public int Rows { get; set; }
        public int Columns { get; set; }

        public ExcelDriver()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public bool SetCurrentDocument(string document)
        {
            try
            {
                sourceFile = new FileInfo(document);
                package = new ExcelPackage(sourceFile);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            if(SetCurrentPageByName("Адреса") == false)
            { 
                return false; 
            }

            // get number of rows and columns in the sheet
            Rows = worksheet.Dimension.Rows; // 20
            Columns = worksheet.Dimension.Columns; // 7
            return true;
        }

        public string GetCellValueByIndex(int row, int column)
        {
            string value = worksheet.Cells[row, column].Value?.ToString();
            return value;
        }

        public void GetLastRowAndColumnIndex()
        {
            var rows = worksheet.Dimension.Rows;
            var columns = worksheet.Dimension.Columns;
        }

        public Task SaveAsync()
        {
            package.Save();
            return Task.CompletedTask;
        }

        public void SetCellColor(System.Drawing.Color color, int row, int column)
        {
            worksheet.Cells[row, column].Style.Font.Color.SetColor(color);
        }

        public void SetCellValueByIndex(string cellValue, int row, int column)
        {
            worksheet.Cells[row, column].Value = cellValue;
        }

        public bool SetCurrentPageByName(string pageName)
        {
            try
            {
                worksheet = package.Workbook.Worksheets[pageName];
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
