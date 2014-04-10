/*

Copyright 2014 Daniel Kinsman.

This file is part of Stand Alone Map View.

Stand Alone Map View is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Stand Alone Map View is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Stand Alone Map View.  If not, see <http://www.gnu.org/licenses/>.

*/

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

		private readonly object _timeUpdateLock = new object();
		private comms.Time _timeUpdate = null;
		public comms.Time TimeUpdate
		{
			get
			{
				lock(_timeUpdateLock)
				{
					return _timeUpdate;
				}
			}
			set
			{
				lock(_timeUpdateLock)
				{
					this._timeUpdate = value;
				}
			}
		}

		private readonly object _vesselUpdateLock = new object();
		private comms.Vessel _vesselUpdate = null;
		public comms.Vessel VesselUpdate
		{
			get
			{
				lock(_vesselUpdateLock)
				{
					return _vesselUpdate;
				}
			}
			set
			{
				lock(_vesselUpdateLock)
				{
					this._vesselUpdate = value;
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

			this.InvokeRepeating("UnityWorker", 0.0f, comms.Packet.updateInterval);
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
			new Thread(SocketWorker).Start();
		}

		public void SocketWorker()
		{
			// Can't log from in here because we aren't in a unity thread.
			while(this.runThread)
			{
				try
				{
					var serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
					comms.Packet packet = comms.Packet.Read(this.socket.Receive(ref serverEndPoint));

					if(packet.Time != null)
						this.TimeUpdate = packet.Time;

					if(packet.Vessel != null)
						this.VesselUpdate = packet.Vessel;
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
			if(this.TimeUpdate != null)
			{
				Planetarium.SetUniversalTime(this.TimeUpdate.UniversalTime);
				//TimeWarp.SetRate();
			}

			if(this.VesselUpdate != null)
			{
				//todo Check if orbital parameters match within some delta, avoid forcing when possible
				FlightGlobals.ActiveVessel.id = this.VesselUpdate.Id;
				FlightGlobals.ActiveVessel.name = this.VesselUpdate.Name;
				FlightGlobals.ActiveVessel.orbitDriver.orbit = this.VesselUpdate.Orbit.GetKspOrbit(FlightGlobals.Bodies);
			}
		}

		public void ForceMapView()
		{
			MapView.EnterMapView();

			//todo find a way to keep the vessel on rails without resorting to time warp
			FlightGlobals.ActiveVessel.GoOnRails();

			while(TimeWarp.CurrentRate < 5.0f)
				TimeWarp.SetRate(TimeWarp.CurrentRateIndex+1, true);

			var blocks = ControlTypes.MAP | ControlTypes.ACTIONS_SHIP | ControlTypes.ALL_SHIP_CONTROLS |
					ControlTypes.GROUPS_ALL | ControlTypes.LINEAR | ControlTypes.QUICKLOAD | ControlTypes.QUICKSAVE |
						ControlTypes.PAUSE | ControlTypes.TIMEWARP | ControlTypes.VESSEL_SWITCHING;
			InputLockManager.SetControlLock(blocks, "stand-alone-map-view");
		}
	}
}