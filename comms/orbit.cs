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
using ProtoBuf;
using System;
using System.Linq;
using System.Collections.Generic;

namespace StandAloneMapView.comms
{
    [ProtoContract]
    public class Orbit
    {
        [ProtoMember(1)]
        public double meanAnomalyAtEpoch;

        [ProtoMember(2)]
        public double LAN;

        [ProtoMember(3)]
        public string ReferenceBody;

        [ProtoMember(4)]
        public double epoch;

        [ProtoMember(5)]
        public double eccentricity;

        [ProtoMember(6)]
        public double semiMajorAxis;

        [ProtoMember(7)]
        public double argumentOfPeriapsis;

        [ProtoMember(8)]
        public double inclination;

        public Orbit()
        {
        }

        public Orbit(global::Orbit kspOrbit)
        {
            this.meanAnomalyAtEpoch = kspOrbit.meanAnomalyAtEpoch;
            this.LAN = kspOrbit.LAN;
            this.ReferenceBody = kspOrbit.referenceBody.name;
            this.epoch = kspOrbit.epoch;
            this.eccentricity = kspOrbit.eccentricity;
            this.semiMajorAxis = kspOrbit.semiMajorAxis;
            this.argumentOfPeriapsis = kspOrbit.argumentOfPeriapsis;
            this.inclination = kspOrbit.inclination;
        }

        public global::Orbit GetKspOrbit(IList<CelestialBody> bodies)
        {
            return new global::Orbit(
                this.inclination,
                this.eccentricity,
                this.semiMajorAxis,
                this.LAN,
                this.argumentOfPeriapsis,
                this.meanAnomalyAtEpoch,
                this.epoch,
                bodies.First((x) => x.name == this.ReferenceBody));
        }
    }
}