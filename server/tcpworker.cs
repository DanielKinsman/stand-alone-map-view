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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using StandAloneMapView.utils;

namespace StandAloneMapView.server
{
    public class TcpWorker
    {
        public IPEndPoint listenerEndPoint { get; set; }
        protected TcpListener listener;

        protected bool runWorker = true;
        protected Thread worker = null;

        public ThreadSafeQueue<string> logMessages { get; private set; }

        protected object vesselListLock = new object();
        protected comms.VesselList _vessels;
        public comms.VesselList Vessels
        {
            get
            {
                lock(vesselListLock)
                    return _vessels;
            }
            set
            {
                lock(vesselListLock)
                    this._vessels = value;
            }
        }

        protected comms.VesselList GetAndNullVessels()
        {
            lock(this.vesselListLock)
            {
                var vessels = this.Vessels;
                this.Vessels = null;
                return vessels;
            }
        }

        public TcpWorker()
        {
            this.logMessages = new ThreadSafeQueue<string>();
        }

        public void Start()
        {
            this.Stop();
            Settings settings = Settings.Load();
            this.listenerEndPoint = new IPEndPoint(IPAddress.Any, settings.ListenPort);
            this.listener = new TcpListener(this.listenerEndPoint);
            this.runWorker = true;
            this.worker = new Thread(Worker);
            worker.Start();
        }

        public void Stop()
        {
            this.runWorker = false;
            if(this.listener != null)
                this.listener.Stop();

            if(this.worker != null)
                this.worker.Join();
        }

        public void Worker()
        {
            while(this.runWorker)
            {
                try
                {
                    this.listener.Start();
                    this.Log("tcp listening");
                    using(var client = this.listener.AcceptTcpClient())
                    {
                        this.listener.Stop();
                        this.Log("tcp connected");
                        var stream = client.GetStream();
                        client.SendTimeout = 1000;
                        stream.WriteTimeout = 1000;

                        while(this.runWorker && client.Connected)
                        {
                            var vesselsToSend = this.GetAndNullVessels();
                            if(vesselsToSend == null)
                            {
                                // If you never send anything, the client always
                                // reports that it is still connected, even when
                                // it isn't.
                                stream.WriteByte((byte)comms.TcpMessage.ConnectionTest);
                                Thread.Sleep(500);
                                continue;
                            }

                            if(vesselsToSend != null)
                            {
                                stream.WriteByte((byte)comms.TcpMessage.VesselList);
                                vesselsToSend.Send(stream);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    // todo log
                    if(!(e is System.IO.IOException || e is SocketException))
                        throw;
                }

                if(this.runWorker)
                    Thread.Sleep(500);
            }
        }

        public void Log(string message, params object[] formatParams)
        {
            this.logMessages.Push(string.Format(message, formatParams));
        }
    }
}
