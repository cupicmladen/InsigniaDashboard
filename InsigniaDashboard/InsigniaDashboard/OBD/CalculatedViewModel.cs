using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InsigniaDashboard.OBD
{
	public class CalculatedViewModel : INotifyPropertyChanged
	{
	    private string _value;

        public CalculatedViewModel()
		{
			Value = "0";
		}

		public string Value
		{
			get { return _value; }
			set
			{
				_value = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
