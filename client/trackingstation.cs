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
        public VesselChecker VesselChecker;

        public TrackingStation()
        {
            this.LogPrefix = "samv client";
            this.socketWorker = new SocketWorker();
            this.VesselChecker = new VesselChecker(this);
        }

        public override void Awake()
        {
            // Invoke the worker on a 2 second delay to let things "settle in"
            // Seems unstable if you don't.
            this.InvokeRepeating("UnityWorker", 2.0f, 0.05f);
            this.socketWorker.Start();
            this.InvokeRepeating("CheckVessels", 15.0f, 5.0f);
        }

        public override void OnDestroy()
        {
            this.CancelInvoke("CheckVessels");
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
                var message = this.socketWorker.logMessages.TryPop(null);
                if(message != null)
                    this.Log(message);

                Flight.UpdateTime(this.socketWorker.TimeUpdate);
                this.UpdateVessel();
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public void CheckVessels()
        {
            this.VesselChecker.Check();
        }

        public void UpdateVessel()
        {
            var vesselUpdate = this.socketWorker.VesselUpdate;
            if(vesselUpdate == null)
                return;


            TrackingStation.UpdateVessel(this, vesselUpdate);
        }

        public static void UpdateVessel(utils.MonoBehaviourExtended logger, comms.Vessel vesselUpdate)
        {
            var vessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id == vesselUpdate.Id);
            if(vessel != null)
            {
                GamePersistence.SaveGame(Startup.SAVEFILE, Startup.SAVEDIRECTORY, SaveMode.OVERWRITE);
                FlightDriver.StartAndFocusVessel(HighLogic.CurrentGame, FlightGlobals.Vessels.IndexOf(vessel));

                return;
            }

            logger.Log("Vessel {0} not found (vessel id {1}).", vesselUpdate.Name, vesselUpdate.Id);
        }
    }
}