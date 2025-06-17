using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DeviceTunerNET.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private const string AppTitle = "Шей да пори 2!";
        private string _title = AppTitle;

        private string _spreadsheetId = string.Empty;
        public string SpreadsheetId 
        {
            get => _spreadsheetId;
            set
            {
                SetProperty(ref _spreadsheetId, value);
            }
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private readonly IFileDialogService _dialogService;
        private readonly IDataRepositoryService _dataRepositoryService;
        private readonly IDialogCaller _dialogCaller;
        private readonly IGoogleDriveSheetsLister _googleDriveSheetsLister;

        public DelegateCommand OpenFileCommand { get; }
        public DelegateCommand OpenUrlCommand { get; }
        public DelegateCommand SaveFileCommand { get; }
        public DelegateCommand CloseAppCommand { get; }
        public DelegateCommand OpenUrlInBrowserCommand { get; }

        public MainWindowViewModel(
            IFileDialogService dialogService, 
            IDataRepositoryService dataRepositoryService, 
            IDialogCaller dialogCaller,
            IGoogleDriveSheetsLister googleDriveSheetsLister)
        {
            _dialogService = dialogService;
            _dataRepositoryService = dataRepositoryService;
            _dialogCaller = dialogCaller;
            _googleDriveSheetsLister = googleDriveSheetsLister;

            OpenFileCommand = new DelegateCommand(OpenFileExecute, OpenFileCanExecute);
            OpenUrlCommand = new DelegateCommand(async () => await OpenUrlExecute(), OpenUrlCanExecute);
            SaveFileCommand = new DelegateCommand(SaveFileExecute, SaveFileCanExecute);
            CloseAppCommand = new DelegateCommand(CloseAppExecute, CloseAppCanExecute);
            OpenUrlInBrowserCommand = new DelegateCommand(OpenUrlInBrowserCommandExecute, OpenUrlInBrowserCommandCanExecute)
                .ObservesProperty(() => SpreadsheetId);
        }

        private bool OpenUrlInBrowserCommandCanExecute()
        {
            return !string.IsNullOrEmpty(SpreadsheetId);
        }

        private void OpenUrlInBrowserCommandExecute()
        {
            var url = "https://docs.google.com/spreadsheets/d/" + SpreadsheetId;
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }

        private bool OpenUrlCanExecute()
        {
            return true;
        }

        private Task OpenUrlExecute()
        {
            
            return Task.Run(GetUrlWithDataAsync);
        }

        private bool CloseAppCanExecute()
        {
            return true;
        }

        private void CloseAppExecute()
        {
            throw new NotImplementedException();
        }

        private bool SaveFileCanExecute()
        {
            return true;
        }

        private void SaveFileExecute()
        {
            _dialogService.SaveFileDialog();
        }

        private bool OpenFileCanExecute()
        {
            return true;
        }

        private void OpenFileExecute()
        {
            if (!_dialogService.OpenFileDialog()) 
                return;

            var selectedFile = _dialogService.FullFileNames; // Путь к Excel-файлу

            // 1 - Поставщик данных - Excel
            _dataRepositoryService.SetDevices(1, selectedFile); //Устанавливаем список всех устройств в репозитории

            var cabinetQuanitity = _dataRepositoryService.GetFullCabinets().Count;
            var newTitle = AppTitle + "   Кол-во шкафов: " + cabinetQuanitity;
            Title = newTitle;
            SpreadsheetId = string.Empty;
        }

        private async Task GetUrlWithDataAsync()
        {
            IEnumerable<UrlItem> historyUrls = await _googleDriveSheetsLister.ListAllSpreadsheetsAsync();
            SpreadsheetId = _dialogCaller.GetUrl(historyUrls);
            if (string.IsNullOrEmpty(SpreadsheetId))
                return;

            _dataRepositoryService.SetDevices(2, SpreadsheetId);
            var cabinetQuanitity = _dataRepositoryService.GetFullCabinets().Count;
            var newTitle = AppTitle + "     Кол-во шкафов: " + cabinetQuanitity;
            Title = newTitle;
        }
    }
}
