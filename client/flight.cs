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
        public SocketWorker socketWorker;

        public Flight()
        {
            this.LogPrefix = "samv client";
            this.socketWorker = new SocketWorker();
        }

        public override void Awake()
        {
            MapView.OnExitMapView += () => ForceMapView();
            this.Invoke("ForceMapView", 0.25f); // Call it directly in Start() and it doesn't work.
            this.InvokeRepeating("UnityWorker", 0.0f, comms.Packet.updateInterval);
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
                UpdateTime(this.socketWorker.TimeUpdate);

                var vesselUpdate = this.socketWorker.VesselUpdate;
                if(vesselUpdate != null)
                {
                    FlightGlobals.ActiveVessel.id = vesselUpdate.Id; // works?
                    FlightGlobals.ActiveVessel.name = vesselUpdate.Name;
                    FlightGlobals.ActiveVessel.vesselName = vesselUpdate.Name;

                    if(FlightGlobals.ActiveVessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.UPDATE)
                    {
                        var orbit = vesselUpdate.Orbit.GetKspOrbit(FlightGlobals.Bodies);
                        FlightGlobals.ActiveVessel.orbit.UpdateFromOrbitAtUT(orbit, Planetarium.GetUniversalTime(), orbit.referenceBody);
                    }
                    else if(FlightGlobals.ActiveVessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys)
                    {
                        // We're off rails, go back on rails
                        FlightGlobals.ActiveVessel.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
                    }
                }
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
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

        public static void UpdateTime(comms.Time timeUpdate)
        {
            if(timeUpdate == null)
                return;

            const double MAX_TIME_DELTA = 2.0; // 2 seconds
            if(Math.Abs(Planetarium.GetUniversalTime() - timeUpdate.UniversalTime) > MAX_TIME_DELTA)
                Planetarium.SetUniversalTime(timeUpdate.UniversalTime);

            if(TimeWarp.CurrentRateIndex != timeUpdate.TimeWarpRateIndex)
                TimeWarp.SetRate(timeUpdate.TimeWarpRateIndex, false);
        }
    }
}