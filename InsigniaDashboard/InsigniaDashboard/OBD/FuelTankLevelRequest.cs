using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class FuelTankLevelRequest : ObdRequest
	{
	    private const int FuelTankCapacity = 70;
	    private const double AveregeConsumption = 7.2; // TODO: calculate this later;
	    private double _remainingFuelInLitres;
	    private int _range;

        public FuelTankLevelRequest()
		{
			Command = "012F";
			CommandShort = "2F";
			Unit = "%";
			Value = "0";
		}

		public int GetFuelTankLevel => int.Parse(Value);
		public string FuelRange => RemainingFuelInLitres + " lit / " + Range + " km";

		public double RemainingFuelInLitres
		{
			get { return _remainingFuelInLitres; }
			set
			{
				_remainingFuelInLitres = value;
				OnPropertyChanged();
				OnPropertyChanged("FuelRange");
			}
		}

		public int Range
		{
			get { return _range; }
			set
			{
				_range = value;
				OnPropertyChanged();
				OnPropertyChanged("FuelRange");
			}
		}

		public override void CalculateValue(IList<string> hexValue)
		{
			var fuelLevel = (0.392 * Convert.ToInt64(hexValue[0], 16));
			//RemainingFuelInLitres = (fuelLevel / 100) * FuelTankCapacity;
			//Range = (int) ((RemainingFuelInLitres / AveregeConsumption)*100);
			Value = "" + fuelLevel;
		}
	}
}
