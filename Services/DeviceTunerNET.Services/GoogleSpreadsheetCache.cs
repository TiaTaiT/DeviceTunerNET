using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedModels;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class GoogleSpreadsheetCache : IGoogleSpreadsheetCache
    {
        public Cell[,] Cache { get; private set; }

        public void PopulateCache(string spreadsheetId, string sheetName, string credentialsPath)
        {
            // Initialize Google Sheets API
            var credential = GoogleCredential.FromFile(credentialsPath)
                .CreateScoped(SheetsService.Scope.SpreadsheetsReadonly);
            var service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "SpreadsheetCacheApp",
            });

            // Fetch spreadsheet metadata to validate the sheet
            var metadataRequest = service.Spreadsheets.Get(spreadsheetId);
            var metadataResponse = metadataRequest.Execute();

            // Find the specified sheet
            var sheet = metadataResponse.Sheets.FirstOrDefault(s => s.Properties.Title == sheetName);
            if (sheet == null)
            {
                throw new Exception($"Sheet with name '{sheetName}' not found.");
            }

            // Fetch the first row to determine column count
            string headerRange = $"{sheetName}!1:1";
            var headerRequest = service.Spreadsheets.Values.Get(spreadsheetId, headerRange);
            var headerResponse = headerRequest.Execute();
            var headerRow = headerResponse.Values?.FirstOrDefault();

            if (headerRow == null || headerRow.Count == 0)
            {
                throw new Exception($"No header row found in sheet '{sheetName}'.");
            }

            int columnCount = headerRow.Count; // Number of captions
            int rowCount;

            // Fetch all rows starting from the first
            string fullRange = $"{sheetName}!A1:{GetColumnName(columnCount)}";
            var dataRequest = service.Spreadsheets.Values.Get(spreadsheetId, fullRange);
            var dataResponse = dataRequest.Execute();
            var values = dataResponse.Values;

            rowCount = values.Count; // Total number of rows including the header row

            // Create a two-dimensional Cell array
            Cache = new Cell[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                //var row = gridData.RowData[i];
                for (int j = 0; j < columnCount; j++)
                {
                    string cellValue = (j < values[i].Count) ? values[i][j]?.ToString() : null;
                    Cache[i, j] = new Cell(cellValue);
                }
            }
        }

        private string GetColumnName(int columnNumber)
        {
            string columnName = string.Empty;
            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = (char)(65 + remainder) + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnName;
        }
    }
}
