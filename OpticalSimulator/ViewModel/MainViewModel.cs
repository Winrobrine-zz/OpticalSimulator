using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using OpticalSimulator.Service;
using System.Windows.Input;
using System.Windows.Media;

namespace OpticalSimulator.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private OpticalService service;
        public OpticalService Service
        {
            get { return service ?? (service = new OpticalService(800, 600)); }
        }

        public MainViewModel()
        {
            CompositionTarget.Rendering += delegate
            {
                B = (int)((Service?.B ?? 0) / 10);
                ImageSize = (int)((Service?.ImageSize ?? 0) / 10);
            };
        }

        private string objectPath;
        public string ObjectPath
        {
            get { return objectPath; }
            set
            {
                if (Set(() => ObjectPath, ref objectPath, value))
                    Service.ObjectPath = ObjectPath;
            }
        }

        private ICommand browseCommand;
        public ICommand BrowseCommand
        {
            get { return browseCommand ?? (browseCommand = new RelayCommand(DoBrowse)); }
        }

        private void DoBrowse()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png",
            };

            if (dialog.ShowDialog().Value)
            {
                ObjectPath = dialog.FileName;
            }
        }

        private ICommand ogaCommand;
        public ICommand OgaCommand
        {
            get { return ogaCommand ?? (ogaCommand = new RelayCommand(DoOga)); }
        }

        private void DoOga()
        {
            Service.Optical = OpticalType.oga;
        }

        private ICommand bolgaCommand;
        public ICommand BolgaCommand
        {
            get { return bolgaCommand ?? (bolgaCommand = new RelayCommand(DoBolga)); }
        }

        private void DoBolga()
        {
            Service.Optical = OpticalType.bolga;
        }

        private ICommand bollenCommand;
        public ICommand BollenCommand
        {
            get { return bollenCommand ?? (bollenCommand = new RelayCommand(DoBollen)); }
        }

        private void DoBollen()
        {
            Service.Optical = OpticalType.bollen;
        }

        private ICommand olenCommand;
        public ICommand OlenCommand
        {
            get { return olenCommand ?? (olenCommand = new RelayCommand(DoOlen)); }
        }

        private void DoOlen()
        {
            Service.Optical = OpticalType.olen;
        }

        private int a;
        public int A
        {
            get { return a; }
            set
            {
                if (Set(() => A, ref a, value))
                {
                    Service.A = A * 10;
                }
            }
        }

        private int b;
        public int B
        {
            get { return b; }
            set
            {
                Set(() => B, ref b, value);
            }
        }

        private int f;
        public int F
        {
            get { return f; }
            set
            {
                if (Set(() => F, ref f, value))
                {
                    Service.F = F * 10;
                }
            }
        }

        private int objectSize = 10;
        public int ObjectSize
        {
            get { return objectSize; }
            set
            {
                if (Set(() => ObjectSize, ref objectSize, value))
                {
                    Service.ObjectSize = ObjectSize * 10;
                }
            }
        }

        private int imageSize;
        public int ImageSize
        {
            get { return imageSize; }
            set
            {
                Set(() => ImageSize, ref imageSize, value);
            }
        }
    }
}