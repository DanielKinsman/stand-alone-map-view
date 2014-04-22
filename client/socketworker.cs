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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StandAloneMapView.client
{
    public class SocketWorker
    {
        protected UdpClient socket;
        public IPEndPoint clientEndPoint { get; set; }
        public IPEndPoint serverEndPoint = null;
        private bool runThread;

        // Thread safety is probably not too much of a concern
        // given we are only "writing" from the worker thread
        // and reading everywhere else, but call me paranoid.
        // I know for sure that those "writes" aren't atomic.

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

        protected comms.ManeuverList cachedManeuvers;
        private readonly object _maneuverUpdateLock = new object();
        private comms.ManeuverList _maneuverUpdate = null;
        public comms.ManeuverList ManeuverUpdate
        {
            get
            {
                lock(_maneuverUpdateLock)
                {
                    return _maneuverUpdate;
                }
            }
            set
            {
                lock(_maneuverUpdateLock)
                {
                    this._maneuverUpdate = value;
                }
            }
        }

        public void Start()
        {
            this.Stop();
            var settings = Settings.Load();
            this.serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
            this.clientEndPoint = new IPEndPoint(IPAddress.Loopback, settings.RecievePort);
            this.socket = new UdpClient(this.clientEndPoint);
            this.runThread = true;
            new Thread(Worker).Start();
        }

        public void Stop()
        {
            this.runThread = false;
            if(this.socket != null)
                this.socket.Close();
        }

        public void Worker()
        {
            while(this.runThread)
            {
                try
                {
                    comms.Packet packet = comms.Packet.Read(this.socket.Receive(ref this.serverEndPoint));

                    if(packet.Time != null)
                        this.TimeUpdate = packet.Time;

                    this.VesselUpdate = packet.Vessel;

                    var maneuverUpdate = packet.ManeuverList;
                    // If they haven't changed, don't process as an update
                    if(maneuverUpdate != null && maneuverUpdate.Equals(this.cachedManeuvers))
                        continue;

                    // They've changed on the other end, update locally
                    this.cachedManeuvers = maneuverUpdate;
                    this.ManeuverUpdate = maneuverUpdate;
                }
                catch(Exception e)
                {
                    //todo log
                    if(!(e is System.IO.IOException || e is SocketException))
                        throw;
                }
            }
        }

        public void Send(IList<ManeuverNode> maneuvers, ITargetable target)
        {
            if(this.serverEndPoint == null)
                throw new InvalidOperationException("The server hasn't contacted us yet!");

            var packet = new comms.ClientPacket();
            packet.ManeuverList = new comms.ManeuverList(maneuvers);
            packet.Target = new comms.Target(target);

            var buffer = packet.Make();
            this.socket.BeginSend(buffer, buffer.Length, this.serverEndPoint, SendCallback, this.socket);
        }

        public static void SendCallback(IAsyncResult result)
        {
            var client = (UdpClient)result.AsyncState;
            client.EndSend(result);
        }
    }
}