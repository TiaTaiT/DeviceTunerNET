using DeviceTunerNET.Services.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Management;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DeviceTunerNET.Services
{
    public class AuthLoader : IAuthLoader
    {
        private string _serial = string.Empty;
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public AuthLoader(ILogger logger)
        {
            _logger = logger;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://crudaster.pro/")
            };
        }

        public IEnumerable<string> AvailableServicesNames { get; set; } = [];
        public string Capsule { get; private set; } = string.Empty;

        public async Task<IEnumerable<string>> GetAvailableServices()
        {
            _serial = GetCurrentMotherboardSerial();

            return await FetchDataAsync();
        }

        private async Task<IEnumerable<string>> FetchDataAsync()
        {
            var featureNames = new List<string>();
            try
            {
                var response = await _httpClient.GetAsync($"api/hardware/byserial/{_serial}");
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadFromJsonAsync<HardwareDto>().Result;
                    foreach (var feature in result.Functionalities)
                    {
                        featureNames.Add(feature.Name);
                    }
                    Capsule = result.Capsule;
                }

                AvailableServicesNames = featureNames;
                // Optionally handle 404, 400, etc.
                return AvailableServicesNames;
            }
            catch (Exception ex)
            {
                // Log or handle network errors
                _logger.Error(ex, "An error occured in FetchFataAsync");
                return [];
            }
        }

        private static string GetCurrentMotherboardSerial()
        {
            var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            var information = searcher.Get();

            var motherBoardSerial = "";
            foreach(var m in information)
            {
                motherBoardSerial = m["SerialNumber"].ToString();
            }
            return motherBoardSerial;
        }

        public record HardwareDto(
        int Id,
        string Serial,
        string Description,
        IEnumerable<FunctionalitySimpleDto> Functionalities,
        string Capsule);

        public record FunctionalitySimpleDto(
        int Id,
        string Name);
    }
}
