using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using InsigniaDashboard.Helper;
using InsigniaDashboard.Interface;
using InsigniaDashboard.OBD;
using Xamarin.Forms;

namespace InsigniaDashboard.ViewModel
{
	public sealed class InfoObdViewModel : INotifyPropertyChanged
	{
	    private bool _isObdConnected;

        private CancellationTokenSource _cancellationTokenSource;
	    private IBtConnectionManager _btManager;

	    public InfoObdViewModel()
		{
		    ConnectToObdCommand = new Command(ConnectToObdCommandExecute);

            RpmRequest = new RpmRequest();
			SpeedRequest = new SpeedRequest();
			CoolantTemperatureRequest = new CoolantTemperatureRequest();
			EngineOilTemperatureRequest = new EngineOilTemperatureRequest();
			CalculatedEngineLoadRequest = new CalculatedEngineLoadRequest();
			FuelTankLevelRequest = new FuelTankLevelRequest();
			MafAirFlowRateRequest = new MafAirFlowRateRequest();
			GearRequest = new CalculatedViewModel();
		}
	    
	    public bool SendRpm { get; set; }

	    public bool SendSpeed { get; set; }

	    public bool SendFuel { get; set; }

	    public bool SendGear { get; set; }
	    
        public bool IsObdConnected
        {
            get => _isObdConnected;
            set
            {
                _isObdConnected = value;
                OnPropertyChanged();
            }
        }

	    public ICommand ConnectToObdCommand { get; private set; }

        public RpmRequest RpmRequest { get; set; }

	    public SpeedRequest SpeedRequest { get; set; }

	    public CoolantTemperatureRequest CoolantTemperatureRequest { get; set; }

	    public EngineOilTemperatureRequest EngineOilTemperatureRequest { get; set; }

	    public CalculatedEngineLoadRequest CalculatedEngineLoadRequest { get; set; }

	    public FuelTankLevelRequest FuelTankLevelRequest { get; set; }

	    public MafAirFlowRateRequest MafAirFlowRateRequest { get; set; }

	    public CalculatedViewModel GearRequest { get; set; }

	    private void ConnectToObdCommandExecute()
	    {
	        _btManager = DependencyService.Get<IBtConnectionManager>();
	        _btManager.ConnectToObd();

	        if (_btManager == null || !_btManager.IsConnected)
	            IsObdConnected = false;

	        var initCommands = new List<string> { "ati\r", "atl0\r", "ath0\r", "ats0\r", "atsp6\r", "atcra 7e8\r", "0100\r", "0100\r" };

	        var index = 0;
	        Device.StartTimer(TimeSpan.FromMilliseconds(500), () =>
	        {
	            _btManager.SendCommand(initCommands[index]);
	            index++;

	            if (index == initCommands.Count)
	                return false;

	            return true;
	        });

	        _btManager.DataReceived += BtDataReceived;
	        _btManager.StartReadingData();

	        StartSendingRequests();

	        IsObdConnected = _btManager.IsConnected;
        }

	    private void BtDataReceived(string command)
	    {
	        //TODO: check if this need to be on main thread
	        Device.BeginInvokeOnMainThread(() =>
	        {
	            var commandsSplit = command.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
	            for (var i = 0; i < commandsSplit.Length; i++)
	            {
	                var response = commandsSplit[i].SplitAndRefactor(2);

	                if (response.Count == 0)
	                {
	                    continue;
	                }

	                var obdCommand = FindCommand(response[0]);

	                if (obdCommand == null)
	                {
	                    continue;
	                }

	                response.RemoveAt(0);
	                obdCommand.CalculateValue(response);

	                if (SendRpm && SendSpeed && SendGear)
	                    CalculateGear();
	            }
	        });
	    }

        private void StartSendingRequests()
	    {
	        if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
	            _cancellationTokenSource = new CancellationTokenSource();

	        Task.Factory.StartNew((o) =>
	        {
	            var frequency = 0;

	            while (!_cancellationTokenSource.IsCancellationRequested)
	            {
	                if (frequency == 0 && SendRpm)
	                {
	                    _btManager.SendCommand(RpmRequest.FormattedCommand);
	                }
	                else if (frequency == 1 && SendSpeed)
	                {
	                    _btManager.SendCommand(SpeedRequest.FormattedCommand);
	                }
	                else if (frequency == 2 && SendFuel)
	                {
	                    _btManager.SendCommand(FuelTankLevelRequest.FormattedCommand);
	                }

	                if (frequency == 2)
	                    frequency = -1;

	                frequency++;
	                Task.Delay(50).Wait();
	            }
	        }, TaskCreationOptions.LongRunning, _cancellationTokenSource.Token);
	    }

        private void StopReadingData()
		{
			_cancellationTokenSource.Cancel();
		}

	    private ObdRequest FindCommand(string commandShort)
        {
            if (RpmRequest.CommandShort == commandShort)
                return RpmRequest;

            if (SpeedRequest.CommandShort == commandShort)
                return SpeedRequest;

            //if (CoolantTemperatureCommand.CommandShort == commandShort)
            //	return CoolantTemperatureCommand;

            //if (EngineOilTemperatureCommand.CommandShort == commandShort)
            //	return EngineOilTemperatureCommand;

            //if (CalculatedEngineLoadCommand.CommandShort == commandShort)
            //	return CalculatedEngineLoadCommand;

            if (FuelTankLevelRequest.CommandShort == commandShort)
                return FuelTankLevelRequest;

            //if (MafAirFlowRateCommand.CommandShort == commandShort)
            //	return MafAirFlowRateCommand;

            return null;
        }

        private void CalculateGear()
        {
            if (RpmRequest.GetRpm == 0)
                return;

            var result = (decimal)SpeedRequest.Speed / RpmRequest.GetRpm;

            if (result > 0 && result <= 0.01m)
            {
                GearRequest.Value = "1";
            }
            else if (result >= 0.01m && result <= 0.02m)
            {
                GearRequest.Value = "2";
            }
            else if (result >= 0.02m && result <= 0.03m)
            {
                GearRequest.Value = "3";
            }
            else if (result >= 0.03m && result <= 0.04m)
            {
                GearRequest.Value = "4";
            }
            else if (result >= 0.04m && result <= 0.05m)
            {
                GearRequest.Value = "5";
            }
            else if (result >= 0.05m && result <= 0.06m)
            {
                GearRequest.Value = "6";
            }
            else
            {
                GearRequest.Value = "0";
            }
        }

	    public event PropertyChangedEventHandler PropertyChanged;

	    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
	    {
	        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	    }
	}
}
