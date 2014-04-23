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

namespace StandAloneMapView.client
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

        public override void Update()
        {
            UnityWorker();
        }

        public void UnityWorker()
        {
            try
            {
                UpdateTime(this.socketWorker.TimeUpdate);

                var vesselUpdate = this.socketWorker.VesselUpdate;
                var vessel = FlightGlobals.ActiveVessel;
                if(vessel == null || vesselUpdate == null)
                {
                    // Main process has probably gone back to space center,
                    // so let's go back to the tracking station.
                    HighLogic.LoadScene(GameScenes.TRACKSTATION);
                    return;
                }

                if(vesselUpdate.Id != vessel.id)
                {
                    // This might look odd, but it aint my fault.
                    // If you try and switch loaded scenes too quickly,
                    // data structures in squad's code don't get populated
                    // and it's exception city.
                    if(vessel.patchedConicRenderer == null ||
                       vessel.patchedConicRenderer.solver == null ||
                       vessel.patchedConicRenderer.solver.maneuverNodes == null)
                    {
                        return;
                    }

                    // Vessel switched, tracking station code handles that
                    TrackingStation.UpdateVessel(this, vesselUpdate);
                    return;
                }

                this.UpdateManeuverNodes();
                this.UpdateTarget();
                this.socketWorker.Send(vessel.patchedConicSolver.maneuverNodes,
                                       vessel.targetObject);

                // We can't release launch clamps, so delete them
                if(vessel.situation == Vessel.Situations.PRELAUNCH)
                    DestroyLaunchClamps(vessel);

                // Vessels near the ground have a habit of exploding, so force them "on rails"
                const float OFF_RAILS_HEIGHT = 300.0f;
                var height = Math.Min(vessel.GetHeightFromTerrain(), (float)vessel.altitude);

                // Careful, GetHeightFromTerrain() returns -1 in high orbit
                if(height > 0.0f && height < OFF_RAILS_HEIGHT)
                {
                    // Unfortunately GetHeightFromTerrain() doesn't get updated when on rails, so
                    // we can't use it check when we should go back off rails. Instead we cheat and
                    // send it across the wire.
                    if(vesselUpdate.Height > 0.0f && vesselUpdate.Height < OFF_RAILS_HEIGHT)
                    {
                        // We're close the the ground, prevent impact
                        vessel.GoOnRails();
                        return;
                    }

                    // We're now at a safe altitude
                    vessel.GoOffRails();

                    // Sometimes the vessel can get stuck in the landed state,
                    // most often when starting a new launch first thing after
                    // starting the game. See issue #17.
                    vessel.checkLanded();
                }

                if(vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.UPDATE)
                {
                    // Finally update the vessel's orbit from the network data
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

        public void UpdateTarget()
        {
            var update = this.socketWorker.TargetUpdate;
            if(update == null)
                return;

            this.socketWorker.TargetUpdate = null;
            update.UpdateTarget();
        }

        public void UpdateManeuverNodes()
        {
            var update = this.socketWorker.ManeuverUpdate;
            if(update == null)
                return;

            this.socketWorker.ManeuverUpdate = null;

            var vessel = FlightGlobals.ActiveVessel;
            if(vessel == null)
                return;

            update.UpdateManeuverNodes(vessel.patchedConicSolver);
        }

        public void ForceMapView()
        {
            MapView.EnterMapView();
            var blocks = ControlTypes.MAP | ControlTypes.ACTIONS_SHIP | ControlTypes.ALL_SHIP_CONTROLS |
                    ControlTypes.GROUPS_ALL | ControlTypes.LINEAR | ControlTypes.QUICKLOAD | ControlTypes.QUICKSAVE |
                        ControlTypes.TIMEWARP | ControlTypes.VESSEL_SWITCHING;
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

            if(timeUpdate.TimeWarpRate > 1.0f && timeUpdate.TimeWarpRate <= 4.0f)
            {
                // Server most likely physics warping. We can't do that, so just set it to 1x.
                if(TimeWarp.CurrentRateIndex != 0)
                    TimeWarp.SetRate(0, false);
                return;
            }

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