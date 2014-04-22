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
using ProtoBuf;

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
    }
}