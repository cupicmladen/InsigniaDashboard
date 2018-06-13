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
	public class InfoObdViewModel : INotifyPropertyChanged
	{
	    private bool _isObdConnected;

        private CancellationTokenSource _cancellationTokenSource;
	    private IBtConnectionManager _btManager;

	    public InfoObdViewModel()
		{
		    ConnectToObdCommand = new Command(ConnectToObdCommandExecute);

            RpmCommand = new RpmViewModel();
			SpeedCommand = new SpeedViewModel();
			CoolantTemperatureCommand = new CoolantTemperatureViewModel();
			EngineOilTemperatureCommand = new EngineOilTemperatureViewModel();
			CalculatedEngineLoadCommand = new CalculatedEngineLoadViewModel();
			FuelTankLevelCommand = new FuelTankLevelViewModel();
			MafAirFlowRateCommand = new MafAirFlowRateViewModel();
			GearCommand = new CalculatedViewModel();
			CurrentConsumptionCommand = new CalculatedViewModel();
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

        public RpmViewModel RpmCommand { get; set; }

	    public SpeedViewModel SpeedCommand { get; set; }

	    public CoolantTemperatureViewModel CoolantTemperatureCommand { get; set; }

	    public EngineOilTemperatureViewModel EngineOilTemperatureCommand { get; set; }

	    public CalculatedEngineLoadViewModel CalculatedEngineLoadCommand { get; set; }

	    public FuelTankLevelViewModel FuelTankLevelCommand { get; set; }

	    public MafAirFlowRateViewModel MafAirFlowRateCommand { get; set; }

	    public CalculatedViewModel GearCommand { get; set; }

	    public CalculatedViewModel CurrentConsumptionCommand { get; set; }

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
	                    _btManager.SendCommand(RpmCommand.FormattedCommand);
	                }
	                else if (frequency == 1 && SendSpeed)
	                {
	                    _btManager.SendCommand(SpeedCommand.FormattedCommand);
	                }
	                else if (frequency == 2 && SendFuel)
	                {
	                    _btManager.SendCommand(FuelTankLevelCommand.FormattedCommand);
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

	    private ObdViewModel FindCommand(string commandShort)
        {
            if (RpmCommand.CommandShort == commandShort)
                return RpmCommand;

            if (SpeedCommand.CommandShort == commandShort)
                return SpeedCommand;

            //if (CoolantTemperatureCommand.CommandShort == commandShort)
            //	return CoolantTemperatureCommand;

            //if (EngineOilTemperatureCommand.CommandShort == commandShort)
            //	return EngineOilTemperatureCommand;

            //if (CalculatedEngineLoadCommand.CommandShort == commandShort)
            //	return CalculatedEngineLoadCommand;

            if (FuelTankLevelCommand.CommandShort == commandShort)
                return FuelTankLevelCommand;

            //if (MafAirFlowRateCommand.CommandShort == commandShort)
            //	return MafAirFlowRateCommand;

            return null;
        }

        private void CalculateGear()
        {
            if (RpmCommand.GetRpm == 0)
                return;

            var result = (decimal)SpeedCommand.Speed / RpmCommand.GetRpm;

            if (result > 0 && result <= 0.01m)
            {
                GearCommand.Value = "1";
            }
            else if (result >= 0.01m && result <= 0.02m)
            {
                GearCommand.Value = "2";
            }
            else if (result >= 0.02m && result <= 0.03m)
            {
                GearCommand.Value = "3";
            }
            else if (result >= 0.03m && result <= 0.04m)
            {
                GearCommand.Value = "4";
            }
            else if (result >= 0.04m && result <= 0.05m)
            {
                GearCommand.Value = "5";
            }
            else if (result >= 0.05m && result <= 0.06m)
            {
                GearCommand.Value = "6";
            }
            else
            {
                GearCommand.Value = "0";
            }
        }

	    public event PropertyChangedEventHandler PropertyChanged;
	    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	    {
	        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	    }
	}
}
