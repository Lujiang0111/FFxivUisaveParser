using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Controls;
using System;
using System.Windows.Input;
using System.Configuration;

namespace FFxivUisaveParser.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public ICommand SelectFileCommand { get; set; }

        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _text = "点击选择文件";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public MainWindowViewModel()
        {
            SelectFileCommand = new DelegateCommand(OnSelectFile);
        }

        private void OnSelectFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                Text = openFileDlg.FileName;
            }
        }
    }
}
