﻿using System;

namespace InsigniaDashboard.Interface
{
	public interface IBtConnectionManager
	{
		void ConnectToObd();
		bool IsConnected { get; }
		void SendCommand(string command);
		void StartReadingData();
		void StopReadingData();
		event Action<string> DataReceived;
	}
}
