using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class CoolantTemperatureViewModel : ObdViewModel
	{
		public CoolantTemperatureViewModel()
		{
			Command = "0105";
			CommandShort = "05";
			Value = "0";
			Unit = "C";
		}

		public override void CalculateValue(IList<string> hexValue)
		{
			Value = "" + (Convert.ToInt64(hexValue[0], 16) - 40);
		}
	}
}
