using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedModels;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

namespace DeviceTunerNET.Services
{
    public class GoogleDriveSheetsLister : IGoogleDriveSheetsLister
    {
        private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        private static readonly string ApplicationName = "Google Drive API .NET 8.0 Example";
        private readonly DriveService _service;
        private readonly ILogger _logger;
        private string _capsule;
        public GoogleDriveSheetsLister(IAuthLoader authLoader, ILogger logger) 
        {
            _logger = logger;
            _capsule = authLoader.Capsule;

            GoogleCredential credential;
            try
            {
                credential = GoogleCredential.FromJson(_capsule).CreateScoped(Scopes);
            }
            catch(Exception ex) 
            {
                _logger.Error(ex, "Parsing capsule error");
                return;
            }

            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<IEnumerable<UrlItem>> ListAllSpreadsheetsAsync()
        {
            var request = _service.Files.List();
            request.Q = "mimeType='application/vnd.google-apps.spreadsheet'";
            request.Fields = "files(id, name)";

            FileList result = await request.ExecuteAsync();
            IList<Google.Apis.Drive.v3.Data.File> files = result.Files;

            var ids = new List<UrlItem>();

            if (files != null && files.Count > 0)
            {
                Console.WriteLine("Spreadsheets:");
                foreach (var file in files)
                {
                    ids.Add(new UrlItem(file.Name, file.Id));
                    Debug.WriteLine($"Name: {file.Name}, ID: {file.Id}");
                }
            }
            else
            {
                Debug.WriteLine("No spreadsheets found.");
            }
            return ids;
        }
    }
}
