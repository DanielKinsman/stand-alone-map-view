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
                var vessel = FlightGlobals.ActiveVessel;
                if(vessel == null || vesselUpdate == null)
                    return;

                if(vesselUpdate.Id != vessel.id)
                {
                    // Vessel switched, tracking station code handles that
                    TrackingStation.UpdateVessel(this, vesselUpdate);
                    return;
                }

                if(vessel.situation == Vessel.Situations.PRELAUNCH)
                    DestroyLaunchClamps(vessel);

                // Vessels near the ground have a habit of exploding
                var height = vessel.GetHeightFromSurface();
                if(height > 0.0 && height < 100.0) // GetHeightFromSurface() returns -1 when in high orbit
                {
                    vessel.GoOnRails();
                    // todo find a way to determine when we are back in the clear
                    // and can go back off rails. Unfortunately we can't use
                    // GetHeightFromSurface() for this, as it doesn't get updated
                    // when we are on rails.
                    // Perhaps just transmit this height accross the wire?
                    return;
                }

                if(vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.UPDATE)
                {
                    var orbit = vesselUpdate.Orbit.GetKspOrbit(FlightGlobals.Bodies);
                    vessel.orbit.UpdateFromOrbitAtUT(orbit, Planetarium.GetUniversalTime(), orbit.referenceBody);
                }
                else if(vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys)
                {
                    // We don't want physics to be calculated
                    vessel.orbitDriver.updateMode = OrbitDriver.UpdateMode.UPDATE;
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

            const double MAX_TIME_DELTA = 1.0; // 1 second per 1x time warp
            double maxTimeDelta = MAX_TIME_DELTA * TimeWarp.CurrentRate;
            if(Math.Abs(Planetarium.GetUniversalTime() - timeUpdate.UniversalTime) > maxTimeDelta)
                Planetarium.SetUniversalTime(timeUpdate.UniversalTime);

            if(TimeWarp.CurrentRateIndex != timeUpdate.TimeWarpRateIndex)
                TimeWarp.SetRate(timeUpdate.TimeWarpRateIndex, false);
        }

        public static void DestroyLaunchClamps(Vessel vessel)
        {
            // thanks go to hyper edit
            if(vessel == null || vessel.parts == null)
                return;

            var clamps = vessel.parts.Where(p => p.Modules != null && p.Modules.OfType<LaunchClamp>().Any()).ToList();
            foreach(var clamp in clamps)
                clamp.Die();
        }
    }
}