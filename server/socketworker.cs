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
using StandAloneMapView.utils;

namespace StandAloneMapView.server
{
    public class SocketWorker
    {
        protected UdpClient socket;
        public IPEndPoint clientEndPoint;
        private bool runThread;
        protected Thread worker = null;

        public ThreadSafeQueue<string> logMessages { get; private set; }

        // Thread safety is probably not too much of a concern
        // given we are only "writing" from the worker thread
        // and reading everywhere else, but call me paranoid.
        // I know for sure that those "writes" aren't atomic.

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

        protected comms.Target cachedTarget;
        private readonly object _targetUpdateLock = new object();
        private comms.Target _targetUpdate = null;
        public comms.Target TargetUpdate
        {
            get
            {
                lock(_targetUpdateLock)
                {
                    return _targetUpdate;
                }
            }
            set
            {
                lock(_targetUpdateLock)
                {
                    this._targetUpdate = value;
                }
            }
        }

        public SocketWorker()
        {
            this.logMessages = new ThreadSafeQueue<string>();
        }

        public void Start(UdpClient socket, IPEndPoint clientEndPoint)
        {
            this.Stop();
            this.socket = socket;
            this.clientEndPoint = clientEndPoint;
            this.runThread = true;
            this.worker = new Thread(Worker);
            worker.Start();
        }

        public void Stop()
        {
            this.runThread = false;
            if(this.socket != null)
                this.socket.Close(); // otherwise Worker will block

            if(this.worker != null)
                this.worker.Join();
        }

        public void Worker()
        {
            Log("Waiting to recieve UDP messages");
            bool gotFirst = false;
            while(this.runThread)
            {
                try
                {
                    var buffer = this.socket.Receive(ref this.clientEndPoint);

                    if(!gotFirst)
                    {
                        Log("Received first UDP message from {0}:{1}",
                                          this.clientEndPoint.Address.ToString(), this.clientEndPoint.Port);
                        gotFirst = true;
                    }

                    var update = comms.ClientPacket.Read(buffer);

                    // If target hasn't changed, don't process as an update
                    if(update.Target == null || !update.Target.Equals(this.cachedTarget))
                    {
                        this.cachedTarget = update.Target;
                        this.TargetUpdate = update.Target;
                    }

                    // If maneuvers haven't changed, don't process as an update
                    var maneuverUpdate = update.ManeuverList;
                    if(maneuverUpdate != null && maneuverUpdate.Equals(this.cachedManeuvers))
                        continue;

                    // They've changed on the other end, update locally
                    this.cachedManeuvers = maneuverUpdate;
                    this.ManeuverUpdate = maneuverUpdate;
                }
                catch(Exception e)
                {
                    Log("SocketWorker exception: {0} {1}", e.Message, e.StackTrace);
                    if(e is SocketException || e is System.IO.IOException)
                    {
                        if(this.runThread)
                            System.Threading.Thread.Sleep(100);
                    }
                    else
                        throw;
                }
            }
        }

        public void Log(string message, params object[] formatParams)
        {
            this.logMessages.Push(string.Format(message, formatParams));
        }
    }
}
