using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class CoolantTemperatureRequest : ObdRequest
	{
		public CoolantTemperatureRequest()
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
