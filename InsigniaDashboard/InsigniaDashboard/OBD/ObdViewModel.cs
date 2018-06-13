using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace InsigniaDashboard.OBD
{
	public class ObdViewModel : INotifyPropertyChanged
	{
	    private string _command;
	    private string _commandShort;
	    private string _value;
	    private string _unit;

        public string Command
		{
			get { return _command; }
			set { _command = value; }
		}

		public string CommandShort
		{
			get { return _commandShort; }
			set { _commandShort = value; }
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

		public string Unit
		{
			get { return _unit; }
			set { _unit = value; }
		}

		public string FormattedCommand => Command + " 1\r";

		public virtual void CalculateValue(IList<string> hexValue)
		{
			throw new NotImplementedException();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
