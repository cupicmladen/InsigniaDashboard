using System;
using System.Collections.Generic;

namespace InsigniaDashboard.OBD
{
	public class CalculatedEngineLoadRequest : ObdRequest
	{
		public CalculatedEngineLoadRequest()
		{
			Command = "0104";
			CommandShort = "04";
			Value = "0";
			Unit = "%";
		}

		public override void CalculateValue(IList<string> hexValue)
		{
			Value = "" + Convert.ToInt64(hexValue[0], 16)/2.55;
		}
	}
}
