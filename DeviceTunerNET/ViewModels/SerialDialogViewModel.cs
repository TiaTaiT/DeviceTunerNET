using System;
using System.Diagnostics;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;

namespace DeviceTunerNET.ViewModels
{
    public class SerialDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);


        private string title = "Serial";
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        private string _serial;
        public string Serial
        {
            get => _serial;
            set => SetProperty(ref _serial, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private string _model;
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }

        private string _designation;
        public string Designation
        {
            get => _designation;
            set => SetProperty(ref _designation, value);
        }

        public DialogCloseListener RequestClose { get; }

        public bool CanCloseDialog() => true;

        protected virtual void CloseDialog(string parameter)
        {
            
            
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.Cancel;
            var parameters = new DialogParameters()
            {
                {"Serial", Serial}
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

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            //RequestClose?.Invoke(dialogResult);
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Message = parameters.GetValue<string>("message");
            Model = parameters.GetValue<string>("model");
            Designation = parameters.GetValue<string>("designation");
        }

        //public event Action<IDialogResult> RequestClose;
    }
}