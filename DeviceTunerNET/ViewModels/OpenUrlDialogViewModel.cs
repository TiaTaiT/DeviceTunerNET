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
        private DelegateCommand<string> _okDialogCommand;
        public DelegateCommand<string> OkDialogCommand =>
            _okDialogCommand ??= new DelegateCommand<string>(CloseDialog, CanExecuteCloseDialog)
                .ObservesProperty(() => SelectedSheet);

        private bool CanExecuteCloseDialog(string arg)
        {
            return SelectedSheet != null;
        }

        private ObservableCollection<UrlItem> _filteredSheets = [];
        public ObservableCollection<UrlItem> FilteredSheets
        {
            get => _filteredSheets;
            set => SetProperty(ref _filteredSheets, value);
        }

        private readonly List<UrlItem> _availableSpreadsheets = [];

        private UrlItem _selectedSheet;
        public UrlItem SelectedSheet 
        { 
            get => _selectedSheet; 
            set => SetProperty(ref _selectedSheet, value); 
        }

        private string _searchTextbox = string.Empty;
        public string SearchTextbox
        {
            get => _searchTextbox;
            set
            {
                SetProperty(ref _searchTextbox, value);
                FilterSpreadsheets();
            }
        }

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
                {"urlName", SelectedSheet.Name},
                {"urlPath", SelectedSheet.Url},
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
            foreach (var url in urls)
            {
                _availableSpreadsheets.Add(url);
                FilteredSheets.Add(url);
            }
        }

        private void FilterSpreadsheets()
        {
            FilteredSheets.Clear();
            if (string.IsNullOrEmpty(_searchTextbox))
            { 
                foreach (var spreadsheet in _availableSpreadsheets)
                {
                    FilteredSheets.Add(spreadsheet);
                }
                return;
            } 
            foreach (var spreadsheet in _availableSpreadsheets)
            {
                if (spreadsheet == null)
                    return;
                if(spreadsheet.Name.Contains(_searchTextbox, StringComparison.CurrentCultureIgnoreCase))
                {
                    FilteredSheets.Add(spreadsheet);
                }
            }
        }

    }
}
