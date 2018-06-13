using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class MafAirFlowRateViewModel : ObdViewModel
	{
		public MafAirFlowRateViewModel()
		{
			Command = "0110";
			CommandShort = "10";
			Unit = "gram/sec";
			Value = "0";
		}

		public int GetMafAirFlowRate => int.Parse(Value);

		public override void CalculateValue(IList<string> hexValue)
		{
			var a = Convert.ToInt64(hexValue[0], 16);
			var b = Convert.ToInt64(hexValue[1], 16);

			Value = "" + ((256*a) + b)/100;
		}
	}
}
