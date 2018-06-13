using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using InsigniaDashboard.Annotations;
using InsigniaDashboard.OBD;
using Xamarin.Forms;

namespace InsigniaDashboard.ViewModel
{
    public class AveregeSpeedViewModel : INotifyPropertyChanged
    {
        private SpeedRequest _speedViewModel;
        private bool _averegeSpeedMeasuringStarted;

        public AveregeSpeedViewModel(SpeedRequest speedViewModel)
        {
            _speedViewModel = speedViewModel;
            MeasureAveregeSpeedCommand = new Command(MeasureAveregeSpeedCommandExecute);
        }

        public ICommand MeasureAveregeSpeedCommand { get; private set; }

        private void MeasureAveregeSpeedCommandExecute()
        {
            _averegeSpeedMeasuringStarted = !_averegeSpeedMeasuringStarted;

            if(_averegeSpeedMeasuringStarted)
                _speedViewModel.PropertyChanged -= _speedViewModel_PropertyChanged;
            else
                _speedViewModel.PropertyChanged += _speedViewModel_PropertyChanged;
        }

        private void _speedViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                var speed = _speedViewModel.Speed;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
