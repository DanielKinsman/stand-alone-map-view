/*

Copyright 2014-2018 Daniel Kinsman.

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

using ProtoBuf;
using System;
using System.Linq;

namespace StandAloneMapView.comms
{
    [ProtoContract]
    public class Target
    {
        [ProtoMember(1)]
        public string CelestialBodyName;

        [ProtoMember(2)]
        public Guid VesselId;

        public Target()
        {
        }

        public Target(ITargetable target)
        {
            this.VesselId = Guid.Empty;

            var body = target as CelestialBody;
            if(body != null)
            {
                this.CelestialBodyName = body.name;
                return;
            }

            this.CelestialBodyName = null;

            var vessel = target as global::Vessel;
            if(vessel == null)
                return;

            this.VesselId = vessel.id;
        }

        public void UpdateTarget()
        {
            ITargetable target = null;
            if(this.CelestialBodyName != null)
            {
                target = FlightGlobals.Bodies.FirstOrDefault(
                                        b => b.name == this.CelestialBodyName);
            }
            else if(this.VesselId != Guid.Empty)
            {
                target = FlightGlobals.Vessels.FirstOrDefault(
                                                v => v.id == this.VesselId);
            }

            if(FlightGlobals.fetch.VesselTarget != target)
                FlightGlobals.fetch.SetVesselTarget(target);
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;

            return this.Equals(obj as Target);
        }

        public bool Equals(Target target)
        {
            if(target == null)
                return false;

            return this.CelestialBodyName == target.CelestialBodyName &&
                            this.VesselId == target.VesselId;
        }

        public override int GetHashCode()
        {
            // the magic numbers in here are primes
            int hash = 3271;
            if(this.CelestialBodyName != null)
                hash = (hash * 1327) + this.CelestialBodyName.GetHashCode();
            hash = (hash * 1327) + this.VesselId.GetHashCode();
            return hash;
        }
    }
}
