using KSP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace StandAloneMapView
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class Flight : utils.MonoBehaviourExtended
	{
		protected UdpClient socket;
		public IPEndPoint clientEndPoint { get; set; }
		private bool runThread;

		private readonly object _updatedTimeLock = new object();
		private comms.Time _updatedTime;
		public comms.Time UpdatedTime
		{
			get
			{
				lock(_updatedTimeLock)
				{
					return _updatedTime;
				}
			}
			set
			{
				lock(_updatedTimeLock)
				{
					this._updatedTime = value;
				}
			}
		}

		public Flight()
		{
			this.clientEndPoint = new IPEndPoint(IPAddress.Loopback, 8397);
			this.LogPrefix = "samv client";
		}

		public override void Awake()
		{
			MapView.OnExitMapView += () => ForceMapView();
			this.Invoke("ForceMapView", 0.25f); // Call it directly in Start() and it doesn't work.

			Log("Starting udp client listening for packets");
			this.socket = new UdpClient(this.clientEndPoint);

			this.InvokeRepeating("UnityWorker", 0.0f, comms.Time.updateInterval);
			StartWorker();
		}

		public override void OnDestroy()
		{
			this.socket.Close();
			this.runThread = false;
		}

		public void StartWorker()
		{
			this.runThread = true;
			new Thread(Worker).Start();
		}

		public void Worker()
		{
			// Can't log from in here because we aren't in a unity thread.
			while(this.runThread)
			{
				try
				{
					var serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] packet = this.socket.Receive(ref serverEndPoint);
					this.UpdatedTime = comms.Time.ReadPacket(packet);
				}
				catch(System.IO.IOException e)
				{
					LogException(e);
					System.Threading.Thread.Sleep(100);
				}
				catch(Exception e)
				{
					LogException(e);
					return;
				}
			}

			// todo update vessel
		}

		public void UnityWorker()
		{
			//todo Check universal time difference within some delta, avoid forcing when possible
			if(this.UpdatedTime != null)
			{
				Planetarium.SetUniversalTime(this.UpdatedTime.UniversalTime);
				//TimeWarp.SetRate();
			}
		}

		public void ForceMapView()
		{
			MapView.EnterMapView();
			var blocks = ControlTypes.MAP | ControlTypes.ACTIONS_SHIP | ControlTypes.ALL_SHIP_CONTROLS |
					ControlTypes.GROUPS_ALL | ControlTypes.LINEAR | ControlTypes.QUICKLOAD | ControlTypes.QUICKSAVE |
						ControlTypes.PAUSE | ControlTypes.TIMEWARP | ControlTypes.VESSEL_SWITCHING;
			InputLockManager.SetControlLock(blocks, "stand-alone-map-view");
		}
	}
}