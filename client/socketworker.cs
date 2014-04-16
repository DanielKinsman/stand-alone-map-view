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
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StandAloneMapView
{
    public class SocketWorker
    {
        protected UdpClient socket;
        public IPEndPoint clientEndPoint { get; set; }
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

        public SocketWorker()
        {
            this.clientEndPoint = new IPEndPoint(IPAddress.Loopback, 8397);
        }

        public void Start()
        {
            this.socket = new UdpClient(this.clientEndPoint);
            this.runThread = true;
            new Thread(Worker).Start();
        }

        public void Stop()
        {
            this.runThread = false;
            this.socket.Close();
        }

        public void Worker()
        {
            while(this.runThread)
            {
                try
                {
                    var serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    comms.Packet packet = comms.Packet.Read(this.socket.Receive(ref serverEndPoint));

                    if(packet.Time != null)
                        this.TimeUpdate = packet.Time;

                    this.VesselUpdate = packet.Vessel;
                }
                catch(System.IO.IOException)
                {
                    //todo log
                    System.Threading.Thread.Sleep(100);
                }
                catch(Exception)
                {
                    //todo log
                    throw;
                }
            }
        }
    }
}