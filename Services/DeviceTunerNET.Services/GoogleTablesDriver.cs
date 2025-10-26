using DeviceTunerNET.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class GoogleTablesDriver : ITablesManager
    {
        private static readonly string ApplicationName = "DeviceTunerNET";
        private readonly SheetsService _sheetsService;
        private readonly IGoogleSpreadsheetCache _spreadsheetCache;
        private readonly string _capsule;
        private string _spreadsheetId;
        private string _sheetName;

        public int Rows { get; set; }
        public int Columns { get; set; }

        public GoogleTablesDriver(IGoogleSpreadsheetCache googleSpreadsheetCache, IAuthLoader authLoader)
        {
            _spreadsheetCache = googleSpreadsheetCache;
            _capsule = authLoader.Capsule ?? "";
            GoogleCredential credential;

            credential = GoogleCredential.FromJson(_capsule);
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
            _spreadsheetCache.PopulateCache(_spreadsheetId, _sheetName);
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
        }

        public async Task<bool> Save()
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
                return false;

            // Batch update the changed values
            var batchUpdateRequest = new BatchUpdateValuesRequest
            {
                ValueInputOption = "RAW", // Use "RAW" to write values as-is
                Data = valueRanges
            };
            try
            {
                var result = await _sheetsService.Spreadsheets.Values.BatchUpdate(batchUpdateRequest, _spreadsheetId).ExecuteAsync();
                Debug.WriteLine(result.Responses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
            return true;
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
