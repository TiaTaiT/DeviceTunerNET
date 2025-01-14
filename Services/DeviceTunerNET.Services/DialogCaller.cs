using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedModels;
using Prism.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DeviceTunerNET.Services
{
    public class DialogCaller(IDialogService dialogService) : IDialogCaller
    {
        private readonly IDialogService _dialogService = dialogService;

#pragma warning disable CA1416 // Validate platform compatibility
        private static DialogParameters GetSerialDialogParams(string model, string designation)
        {
            return new DialogParameters
                {
                    {"title", "Ввод серийного номера."},
                    {"message", "Серийник: "},
                    {"model", model},
                    {"designation", designation}
                };
        }

        private static DialogParameters GetUrlDialogParams(IEnumerable<UrlItem> historyUrls)
        {
            return new DialogParameters
                {
                    {"title", "Open URL"},
                    {"historyUrls", historyUrls},
                };
        }

        public string GetSerialNumber(string model, string designation)
        {
            var manualReset = new ManualResetEvent(false);
            var serialNumber = string.Empty;
            var result = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var parameters = GetSerialDialogParams(model, designation);

                _dialogService.ShowDialog("SerialDialog", parameters, dialogResult =>
                {
                    if (dialogResult.Result == ButtonResult.OK
                        && dialogResult.Parameters.ContainsKey("Serial"))
                    {
                        serialNumber = dialogResult.Parameters.GetValue<string>("Serial");
                    }
                    manualReset.Set();
                });
            }));
            manualReset.WaitOne();
            return serialNumber;
        }

        public string GetUrl(IEnumerable<UrlItem> historyUrls)
        {
            var manualReset = new ManualResetEvent(false);
            string url = string.Empty;
            var result = System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var parameters = GetUrlDialogParams(historyUrls);

                _dialogService.ShowDialog("OpenUrlDialog", parameters, dialogResult =>
                {
                    if (dialogResult.Result == ButtonResult.OK
                        && dialogResult.Parameters.ContainsKey("urlPath"))
                    {
                        url = dialogResult.Parameters.GetValue<string>("urlPath");
                    }
                    manualReset.Set();
                });
            }));
            manualReset.WaitOne();
            return url;
        }

        public void ShowMessage(string message)
        {
            MessageBox.Show(message);
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
