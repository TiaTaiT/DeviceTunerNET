using DeviceTunerNET.Services;
using DeviceTunerNET.Services.Interfaces;
using DeviceTunerNET.SharedModels;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceTunerNET.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Шей да пори! 2";
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
        }

        private async Task GetUrlWithDataAsync()
        {
            IEnumerable<UrlItem> historyUrls = await _googleDriveSheetsLister.ListAllSpreadsheetsAsync();
            var result = _dialogCaller.GetUrl(historyUrls);
            if (string.IsNullOrEmpty(result))
                return;

            _dataRepositoryService.SetDevices(2, result);
        }
    }
}
