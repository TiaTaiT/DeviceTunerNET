using DeviceTunerNET.Services.Interfaces;
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
        private readonly HttpClient _httpClient;

        public AuthLoader()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://crudaster.pro/")
            };
        }

        public IEnumerable<string> AvailableServicesNames { get; set; } = [];

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
                }

                AvailableServicesNames = featureNames;
                // Optionally handle 404, 400, etc.
                return AvailableServicesNames;
            }
            catch (Exception ex)
            {
                // Log or handle network errors
                throw new ApplicationException("Error fetching hardware", ex);
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
        IEnumerable<FunctionalitySimpleDto> Functionalities);

        public record FunctionalitySimpleDto(
        int Id,
        string Name);
    }
}
