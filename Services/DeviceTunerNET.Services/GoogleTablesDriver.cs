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
    public class GoogleTablesDriver : IDataDecoder
    {
        private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        private static readonly string ApplicationName = "Google Sheets API .NET 8.0 Example";
        private readonly SheetsService _service;
        private const string credentialsPath = "C:\\Users\\texvi\\Downloads\\firm-capsule-441717-e2-2f90d66e4ff2.json";

        public ITablesManager Driver
        { 
            get;
            set;
        }

        public GoogleTablesDriver()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            _service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public List<Cabinet> GetCabinetsAsync(string ExcelFileFullPath)
        {
            var id = "1HCHED1c9MnpMkSqdq0j5oS8eKJPAvcOe23u9bDszbUU";
            var range = "Sheet1!A1:A13";
            Task.Run(() => ReadDataAsync(id, range)).GetAwaiter().GetResult();
            return new List<Cabinet> { };
        }

        public bool SaveQualityControlPassed(int id, bool qualityControlPassed)
        {
            throw new NotImplementedException();
        }

        public bool SaveSerialNumber(int id, string serialNumber)
        {
            throw new NotImplementedException();
        }

        private async Task ReadDataAsync(string spreadsheetId, string range)
        {
            var request = _service.Spreadsheets.Values.Get(spreadsheetId, range);
            ValueRange response;
            try
            {
                response = await request.ExecuteAsync();
                var values = response.Values;

                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        Debug.WriteLine(string.Join(", ", row));
                    }
                }
                else
                {
                    Debug.WriteLine("No data found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void SetDecoder(ITablesManager tablesManager)
        {
            throw new NotImplementedException();
        }
    }
}
