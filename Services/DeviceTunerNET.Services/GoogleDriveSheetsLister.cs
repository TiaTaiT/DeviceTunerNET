using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedModels;
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
        private static readonly string[] Scopes = { DriveService.Scope.DriveReadonly };
        private static readonly string ApplicationName = "Google Drive API .NET 8.0 Example";
        private readonly DriveService _service;
        private string _capsule;
        public GoogleDriveSheetsLister(IAuthLoader authLoader) 
        {
            _capsule = authLoader.Capsule;

            var credential = GoogleCredential.FromJson(_capsule).CreateScoped(Scopes);

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
