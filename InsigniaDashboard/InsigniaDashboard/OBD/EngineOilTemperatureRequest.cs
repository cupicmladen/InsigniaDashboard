using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class EngineOilTemperatureRequest : ObdRequest
	{
		public EngineOilTemperatureRequest()
		{
			Command = "015C";
			CommandShort = "5C";
			Value = "0";
			Unit = "C";
		}

		public override void CalculateValue(IList<string> hexValue)
		{
			Value = "" + (Convert.ToInt64(hexValue[0], 16) - 40);
		}
	}
}
