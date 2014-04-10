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
using System.Runtime.Serialization;
using UnityEngine;

namespace StandAloneMapView
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class Server : utils.MonoBehaviourExtended
	{
		// Don't start more than one server, but also close the server when
		// user goes back to main menu.
		private static volatile Server instance = null;

		public IPEndPoint clientEndPoint { get; set; }
		protected UdpClient socket;

		public Server()
		{
			this.clientEndPoint = new IPEndPoint(IPAddress.Loopback, 8397);
			this.LogPrefix = "samv server";
		}

		public override void Awake()
		{
			if(Server.instance != null)
			{
				// User has just gone back to the space center from an already running game
				Destroy(this.gameObject);
				return;
			}

			// User has just loaded a new game
			Server.instance = this;
			DontDestroyOnLoad(this.gameObject);
			Log("Starting udp server, sending to {0}:{1}", clientEndPoint.Address, clientEndPoint.Port);
			this.socket = new UdpClient();
			this.InvokeRepeating("UnityWorker", 0.0f, comms.Packet.updateInterval);
		}

		public override void OnDestroy()
		{
			if(Server.instance != this)
				return;

			Server.instance = null;
			LogDebug("closing socket");
			this.socket.Close();
		}

		public void UnityWorker()
		{
			// Kill the server if the user exits the current game
			if(HighLogic.LoadedScene == GameScenes.MAINMENU ||
			   HighLogic.LoadedScene == GameScenes.CREDITS ||
			   HighLogic.LoadedScene == GameScenes.SETTINGS )

			{
				Destroy(this.gameObject);
			}

			// Don't send time or flight data when in the VAB, astronaut complex etc.
			if(!HighLogic.LoadedSceneHasPlanetarium)
			{
				//todo send sleep message or something, maybe cancel repeating
				return;
			}

			try
			{
				var packet = new comms.Packet();
				packet.Time = new comms.Time(Planetarium.GetUniversalTime(), TimeWarp.CurrentRate);

				if(FlightGlobals.ActiveVessel != null)
				{
					packet.Vessel = new comms.Vessel(FlightGlobals.ActiveVessel);
				}

				byte[] buffer = packet.Make();
				this.socket.BeginSend(buffer, buffer.Length, this.clientEndPoint, SendCallback, this.socket);
			}
			catch(System.IO.IOException e)
			{
				LogException(e);
			}
		}

		public static void SendCallback(IAsyncResult result)
		{
			var client = (UdpClient)result.AsyncState;
			client.EndSend(result);
		}
	}
}