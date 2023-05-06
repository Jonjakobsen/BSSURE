using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bssure.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotScanning))]
        bool isScanning;

        [ObservableProperty]
        string title;

        public bool IsNotBusy
        {
            get
            {
                return !isBusy;
            }
        }

        public bool IsNotScanning
        {
            get
            {
                return !isScanning;
            }
        }
    }
}
