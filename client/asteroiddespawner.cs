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

using System.IO;

namespace StandAloneMapView.client
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class AsteroidDespawner : utils.MonoBehaviourExtended
    {
        public AsteroidDespawner()
        {
            this.LogPrefix = "samv client";
        }

        public override void Awake()
        {
            GameEvents.onVesselCreate.Add(Despawn);
        }

        public override void OnDestroy()
        {
            GameEvents.onVesselCreate.Remove(Despawn);
        }

        public void Despawn(Vessel vessel)
        {
            if(vessel.vesselType != VesselType.SpaceObject && 
               vessel.vesselType != VesselType.Unknown)
            {
                return;
            }

            // Check to see if the asteroid came from the
            // server or was spawned on the client
            string saveFileContents;
            lock(TcpWorker.Instance.SaveFileLock)
            {
                saveFileContents = File.ReadAllText(Startup.SavePath);
            }

            var id = vessel.id.ToString().Replace("-", string.Empty);
            if(saveFileContents.Contains(id))
            {
                LogDebug("Asteroid {0} came from server, keeping it", vessel.vesselName);
                return;
            }

            LogDebug("Despawning client generated asteroid {0}", vessel.vesselName);
            vessel.Die();
        }
    }
}