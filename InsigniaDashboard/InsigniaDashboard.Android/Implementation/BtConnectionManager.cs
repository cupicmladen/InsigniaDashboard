using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Runtime;
using InsigniaDashboard.Droid.Implementation;
using InsigniaDashboard.Interface;
using Java.Util;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(BtConnectionManager))]
namespace InsigniaDashboard.Droid.Implementation
{
	public class BtConnectionManager : IBtConnectionManager
	{
        private BluetoothSocket _socket;
	    private bool _readingData;
	    private readonly CancellationTokenSource _ts;
	    private readonly CancellationToken _ct;

	    public event Action<string> DataReceived;

        public BtConnectionManager()
		{
			_ts = new CancellationTokenSource();
			_ct = _ts.Token;
		}

	    public bool IsConnected => _socket?.IsConnected ?? false;

        public void ConnectToObd()
		{
		    var btAdapter = BluetoothAdapter.DefaultAdapter;

		    if (btAdapter == null || !btAdapter.IsEnabled)
		    {
		        Application.Current.MainPage.DisplayAlert("BtConnectionManager", "Bluetooth default adapter not found", "Cancel");
		        return;
		    }

		    var device = btAdapter.BondedDevices.FirstOrDefault(it => it.Name.Contains("OBD"));

		    if (device == null)
		    {
		        Application.Current.MainPage.DisplayAlert("BtConnectionManager", " OBD bonded devices not found", "Cancel");
                return;
            }

		    try
		    {
		        var createRfcommSocket = JNIEnv.GetMethodID(device.Class.Handle, "createRfcommSocket", "(I)Landroid/bluetooth/BluetoothSocket;");
		        var socketTmp = JNIEnv.CallObjectMethod(device.Handle, createRfcommSocket, new Android.Runtime.JValue(1));
		        _socket = Java.Lang.Object.GetObject<BluetoothSocket>(socketTmp, JniHandleOwnership.TransferLocalRef);

                _socket.Connect();
		    }
		    catch (Exception)
		    {
		        Application.Current.MainPage.DisplayAlert("BtConnectionManager", "Catch exception", "Cancel");
            }
        }

		public void SendCommand(string command)
		{
			var array = Encoding.ASCII.GetBytes(command);
			_socket.OutputStream.Write(array, 0, array.Length);
		}

		public void StartReadingData()
		{
			Task.Factory.StartNew(() =>
			{
				_readingData = true;
				while (_readingData)
				{
					var value = ReadData();
					if (!string.IsNullOrEmpty(value))
						DataReceived?.Invoke(value);
				}
			}, _ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}

		public void StopReadingData()
		{
			_readingData = false;
			_ts.Cancel();
			_socket.Close();
		}

	    private string ReadData()
	    {
	        var buffer = new byte[1024];
	        var cont = true;
	        var value = string.Empty;
	        while (cont)
	        {
	            var count = _socket.InputStream.Read(buffer, 0, buffer.Length);
	            value += Encoding.ASCII.GetString(buffer, 0, count);
	            if (value.EndsWith(">"))
	                cont = false;
	        }

	        return value.Replace("\r", string.Empty);
	    }

	    // unused method (leave it for now)
	    private bool TryConnectOld()
	    {
	        var btAdapter = BluetoothAdapter.DefaultAdapter;
	        var devices = btAdapter.BondedDevices.ToList();

	        if (!devices.Any())
	            return false;

	        var device = devices.FirstOrDefault(it => it.Name.Contains("OBD"));
	        if (device == null)
	            return false;

	        _socket = device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

	        _socket.Connect();

	        if (_socket.IsConnected)
	            _readingData = true;

	        return _socket.IsConnected;
	    }
    }
}