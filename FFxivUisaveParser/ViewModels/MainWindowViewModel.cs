using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows.Input;
using Prism.Regions;
using Prism.Events;
using FFxivUisaveParser.Common;
using System.Runtime.CompilerServices;

namespace FFxivUisaveParser.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private IRegionManager regionManager;
        private IEventAggregator eventAggregator;

        private Uisave uisave;

        public ICommand SelectFileCommand { get; set; }

        public ICommand ParseCommand { get; set; }

        private string title = "FF14 Uisave Parser";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string filePath = "点击选择文件...";
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }

        public MainWindowViewModel(IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            uisave = new Uisave(this.eventAggregator);

            SelectFileCommand = new DelegateCommand(OnSelectFile);
            ParseCommand = new DelegateCommand(OnParse);
        }

        private void OnSelectFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (true == result)
            {
                FilePath = openFileDlg.FileName;
            }
        }

        private void OnParse()
        {
            regionManager.Regions["ContentRegion"].RequestNavigate("XmlTreeView");
            eventAggregator.GetEvent<FilePathEvent>().Publish(FilePath);
        }
    }
}
