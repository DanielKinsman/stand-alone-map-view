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
using System.Linq;

namespace StandAloneMapView.client
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class TrackingStation : utils.MonoBehaviourExtended
    {
        public SocketWorker socketWorker;
        public bool LoadRequired = false;

        public TrackingStation()
        {
            this.LogPrefix = "samv client";
            this.socketWorker = new SocketWorker();
        }

        public override void Awake()
        {
            // Invoke the worker on a 2 second delay to let things "settle in"
            // Seems unstable if you don't.
            this.InvokeRepeating("UnityWorker", 2.0f, 0.05f);
            this.socketWorker.Start();

        }

        public override void OnDestroy()
        {
            if(this.socketWorker != null)
            {
                this.socketWorker.Stop();
                this.socketWorker = null;
            }
        }

        public void UnityWorker()
        {
            try
            {
                if(TcpWorker.Instance.SaveReceived.WaitOne(0))
                {
                    LogDebug("New save received from server.");
                    TcpWorker.Instance.SaveReceived.Reset();
                    this.LoadRequired = true;
                }

                Flight.UpdateTime(this.socketWorker.TimeUpdate);
                this.UpdateVessel();
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public void UpdateVessel()
        {
            if(this.socketWorker.VesselUpdate == null)
                return;

            if(this.LoadRequired)
            {
                Startup.LoadSave(true);
                this.LoadRequired = false;
                return;
            }

            TrackingStation.UpdateVessel(this, this.socketWorker.VesselUpdate);
        }

        public static void UpdateVessel(utils.MonoBehaviourExtended logger, comms.Vessel vesselUpdate)
        {
            if(vesselUpdate == null)
                return;

            // go through vessels
            // find the one with the correct id
            // switch to it

            var vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselUpdate.Id);
            if(vessel != null)
            {
                FlightDriver.StartAndFocusVessel(HighLogic.CurrentGame, FlightGlobals.Vessels.IndexOf(vessel));
                return;
            }

            logger.Log("Vessel {0} not found, reloading save (vessel id {1})", vesselUpdate.Name, vesselUpdate.Id);
            Startup.LoadSave(true);
        }
    }
}