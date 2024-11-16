using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedDataModel;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class GoogleTablesDriver : ITablesManager
    {
        private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private static readonly string ApplicationName = "DeviceTunerNET";
        private readonly SheetsService _sheetsService;
        private readonly IGoogleSpreadsheetCache _spreadsheetCache;
        private string _spreadsheetId;
        private string _sheetName;
        private const string _credentialsPath = "C:\\Users\\texvi\\Downloads\\firm-capsule-441717-e2-2f90d66e4ff2.json";

        public ITablesManager Driver
        { 
            get;
            set;
        }
        public int Rows { get; set; }
        public int Columns { get; set; }

        public GoogleTablesDriver(IGoogleSpreadsheetCache googleSpreadsheetCache)
        {
            _spreadsheetCache = googleSpreadsheetCache;
            GoogleCredential credential;
            using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            _sheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public bool SetCurrentDocument(string spreadsheetId)
        {
            _spreadsheetId = spreadsheetId;
            SetCurrentPageByName("Адреса");
            return true;
        }

        public bool SetCurrentPageByName(string pageName)
        {
            _sheetName = pageName;
            _spreadsheetCache.PopulateCache(_spreadsheetId, _sheetName, _credentialsPath);
            GetLastRowAndColumnIndex();
            return true;
        }

        public void GetLastRowAndColumnIndex()
        {
            Rows = _spreadsheetCache.Cache.GetLength(0);
            Columns = _spreadsheetCache.Cache.GetLength(1);
        }

        public string GetCellValueByIndex(int row, int column)
        {
            var cellValue = _spreadsheetCache.Cache[row - 1, column - 1].Value;
            return cellValue;
        }

        public void SetCellValueByIndex(string cellValue, int row, int column)
        {
            _spreadsheetCache.Cache[row - 1, column - 1].Value = cellValue;
        }

        public void SetCellColor(System.Drawing.Color color, int row, int column)
        {
            //throw new NotImplementedException();
        }

        public async Task SaveAsync()
        {
            // List to store all updated values
            var valueRanges = new List<ValueRange>();

            // Iterate through the cache to find changed cells
            for (int row = 0; row < _spreadsheetCache.Cache.GetLength(0); row++)
            {
                for (int col = 0; col < _spreadsheetCache.Cache.GetLength(1); col++)
                {
                    var cell = _spreadsheetCache.Cache[row, col];
                    if (cell?.WasChanged == true)
                    {
                        // Create range and value update for the changed cell
                        string cellAddress = $"{_sheetName}!{GetColumnName(col + 1)}{row + 1}";
                        valueRanges.Add(new ValueRange
                        {
                            Range = cellAddress,
                            Values = [[cell.Value]]
                        });

                        // Reset the WasChanged property
                        cell.WasChanged = false;
                    }
                }
            }

            // If no changes were detected, exit early
            if (valueRanges.Count == 0)
                return;

            // Batch update the changed values
            var batchUpdateRequest = new BatchUpdateValuesRequest
            {
                ValueInputOption = "RAW", // Use "RAW" to write values as-is
                Data = valueRanges
            };

            await _sheetsService.Spreadsheets.Values.BatchUpdate(batchUpdateRequest, _spreadsheetId).ExecuteAsync();
        }

        private string GetColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)(remainder + 'A') + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnName;
        }
    }
}
