using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class RpmViewModel : ObdViewModel
	{
		public RpmViewModel()
		{
			Command = "010C";
			CommandShort = "0C";
			Unit = "RPM";
			Value = "0";
		}

		public int GetRpm => int.Parse(Value);

		public override void CalculateValue(IList<string> hexValue)
		{
			var a = Convert.ToInt64(hexValue[0], 16);
			var b = Convert.ToInt64(hexValue[1], 16);

			Value = "" + ((256*a) + b)/4;
		}
	}
}
