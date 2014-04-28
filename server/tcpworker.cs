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

namespace StandAloneMapView.server
{
    public class TcpWorker
    {
        public IPEndPoint listenerEndPoint { get; set; }
        protected TcpListener listener;

        protected bool runWorker = true;

        protected object saveLock = new object();
        protected comms.Save _save;
        public comms.Save Save
        {
            get
            {
                lock(saveLock)
                    return _save;
            }
            set
            {
                lock(saveLock)
                    this._save = value;
            }
        }

        public TcpWorker()
        {

        }

        public void Start()
        {
            this.Stop();
            Settings settings = Settings.Load();
            this.listenerEndPoint = new IPEndPoint(IPAddress.Any, settings.ListenPort);
            this.listener = new TcpListener(this.listenerEndPoint);
            this.runWorker = true;
            new Thread(Worker).Start();
        }

        public void Stop()
        {
            this.runWorker = false;
            if(this.listener != null)
                this.listener.Stop();
        }

        public void Worker()
        {
            while(this.runWorker)
            {
                try
                {
                    this.listener.Start();
                    using(var client = this.listener.AcceptTcpClient())
                    {
                        this.listener.Stop();
                        var stream = client.GetStream();
                        client.SendTimeout = 1000;
                        stream.WriteTimeout = 1000;

                        bool firstSendDone = false;
                        while(this.runWorker && client.Connected)
                        {
                            var saveToSend = this.Save;
                            this.Save = null;

                            if(!firstSendDone && saveToSend == null &&
                               HighLogic.CurrentGame != null)
                            {
                                // For newly connected clients, force a sync
                                saveToSend = comms.Save.FromCurrentGame();
                            }

                            if(saveToSend == null)
                            {
                                // If you never send anything, the client always
                                // reports that it is still connected, even when
                                // it isn't.
                                stream.WriteByte((byte)comms.TcpMessage.ConnectionTest);
                                Thread.Sleep(500);
                                continue;
                            }

                            stream.WriteByte((byte)comms.TcpMessage.SaveUpdate);
                            saveToSend.Send(stream);
                            firstSendDone = true;
                        }
                    }
                }
                catch(Exception e)
                {
                    // todo log
                    if(!(e is System.IO.IOException || e is SocketException))
                        throw;
                }

                Thread.Sleep(500);
            }
        }
    }
}