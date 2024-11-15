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
            if(!SetCurrentPageByName("Адреса"))
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
            return worksheet.Cells[row, column].Value?.ToString();
        }

        public int GetLastColumnIndex()
        {
            return worksheet.Dimension.Columns;
        }

        public int GetLastRowIndex()
        {
            return worksheet.Dimension.Rows;
        }

        public void Save()
        {
            package.Save();
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
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
