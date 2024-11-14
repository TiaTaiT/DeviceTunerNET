using DeviceTunerNET.SharedModels;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DeviceTunerNET.ViewModels
{
    internal class OpenUrlDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        private ObservableCollection<UrlItem> _historyUrls;
        public ObservableCollection<UrlItem> HistoryUrls { get => _historyUrls; set => SetProperty(ref _historyUrls, value); }

        private UrlItem selectedUrl;
        public UrlItem SelectedUrl { get => selectedUrl; set => SetProperty(ref selectedUrl, value); }

        public DialogCloseListener RequestClose { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;
            var parameters = new DialogParameters()
            {
                {"urlName", SelectedUrl.Name},
                {"urlPath", SelectedUrl.Url},
            };
            var dialogResult = new DialogResult()
            {
                Result = result,
                Parameters = parameters
            };
            RequestClose.Invoke(dialogResult);
            //RaiseRequestClose(new DialogResult(result));
        }

        public void OnDialogClosed()
        {
            Debug.WriteLine("The Demo Dialog has been closed...");
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var urls = parameters.GetValue<IEnumerable<UrlItem>>("historyUrls");
            HistoryUrls = [];
            foreach (var url in urls)
            { 
                HistoryUrls.Add(url);
            }
        }
    }
}
