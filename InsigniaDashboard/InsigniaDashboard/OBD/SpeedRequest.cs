using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class SpeedRequest : ObdRequest
	{
		public SpeedRequest()
		{
			Command = "010D";
			CommandShort = "0D";
			Unit = "Km/H";
			Value = "0";
		}

		public int Speed => int.Parse(Value);

		public override void CalculateValue(IList<string> hexValue)
		{
			Value = "" + Convert.ToInt64(hexValue[0], 16);
		}
	}
}
