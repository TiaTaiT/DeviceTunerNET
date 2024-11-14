using DeviceTunerNET.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;

namespace DeviceTunerNET.Services
{
    public class GoogleDriveSheetsLister : IGoogleDriveSheetsLister
    {
        private const string credentialsPath = "C:\\Users\\texvi\\Downloads\\firm-capsule-441717-e2-2f90d66e4ff2.json";
        private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        private static readonly string ApplicationName = "Google Drive API .NET 8.0 Example";
        private readonly DriveService _service;
        GoogleDriveSheetsLister() 
        {
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
            }

            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<IEnumerable<string>> ListAllSpreadsheetsAsync()
        {
            var request = _service.Files.List();
            request.Q = "mimeType='application/vnd.google-apps.spreadsheet'";
            request.Fields = "files(id, name)";

            FileList result = await request.ExecuteAsync();
            IList<Google.Apis.Drive.v3.Data.File> files = result.Files;

            var ids = new List<string>();

            if (files != null && files.Count > 0)
            {
                Console.WriteLine("Spreadsheets:");
                foreach (var file in files)
                {
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
