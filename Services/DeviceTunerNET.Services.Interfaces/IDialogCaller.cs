using System.Collections.Generic;
using DeviceTunerNET.SharedModels;

namespace DeviceTunerNET.Services.Interfaces
{
    public interface IDialogCaller
    {
        public string GetSerialNumber(string model, string designation);
        public void ShowMessage(string message);

        public string GetUrl(IEnumerable<UrlItem> message);
    }
}
