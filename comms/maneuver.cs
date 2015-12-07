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

using ProtoBuf;
using System;
using System.Collections.Generic;

namespace StandAloneMapView.comms
{
    [ProtoContract]
    public class Maneuver
    {
        [ProtoMember(1)]
        public double UniversalTime {get;set;}

        [ProtoMember(2)]
        public double DeltaVX {get;set;}

        [ProtoMember(3)]
        public double DeltaVY {get;set;}

        [ProtoMember(4)]
        public double DeltaVZ {get;set;}

        public Vector3d DeltaV
        {
            get
            {
                return new Vector3d(this.DeltaVX, this.DeltaVY, this.DeltaVZ);
            }
        }

        public Maneuver()
        {
        }

        public Maneuver(ManeuverNode maneuver)
        {
            this.UniversalTime = maneuver.UT;
            this.DeltaVX = maneuver.DeltaV.x;
            this.DeltaVY = maneuver.DeltaV.y;
            this.DeltaVZ = maneuver.DeltaV.z;
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;

            return this.Equals(obj as Maneuver);
        }

        public bool Equals(Maneuver maneuver)
        {
            if(maneuver == null)
                return false;

            return this.UniversalTime == maneuver.UniversalTime &&
                            this.DeltaVX == maneuver.DeltaVX &&
                            this.DeltaVY == maneuver.DeltaVY &&
                            this.DeltaVZ == maneuver.DeltaVZ;
        }

        public override int GetHashCode()
        {
            // the magic numbers in here are primes
            int hash = 3271;
            hash = (hash * 1327) + this.UniversalTime.GetHashCode();
            hash = (hash * 1327) + this.DeltaVX.GetHashCode();
            hash = (hash * 1327) + this.DeltaVY.GetHashCode();
            hash = (hash * 1327) + this.DeltaVZ.GetHashCode();
            return hash;
        }
    }

    [ProtoContract]
    public class ManeuverList
    {
        [ProtoMember(1)]
        public Maneuver[] Maneuvers;

        public ManeuverList()
        {
            this.Maneuvers = null;
        }

        public ManeuverList(IList<ManeuverNode> maneuverList)
        {
            this.Maneuvers = new Maneuver[maneuverList.Count];
            for(int i = 0; i < maneuverList.Count; i++)
                this.Maneuvers[i] = new Maneuver(maneuverList[i]);
        }


        public void UpdateManeuverNodes(PatchedConicSolver solver)
        {
            if(solver == null)
                return;

            // Avoid flickering by not overwriting nodes where possible

            var maneuversLength = this.Maneuvers == null ? 0 : this.Maneuvers.Length;
            int commonLength = Math.Min(solver.maneuverNodes.Count, maneuversLength);

            // update the common ones
            for(int i = 0; i < commonLength; i++)
            {
                var node = solver.maneuverNodes[i];
                var nodeUpdate = this.Maneuvers[i];
                node.UT = nodeUpdate.UniversalTime;
                node.DeltaV = nodeUpdate.DeltaV;
                node.OnGizmoUpdated(node.DeltaV, node.UT);
            }

            // remove any extra ones
            for(int i = solver.maneuverNodes.Count; i > maneuversLength; i--)
            {
                var node = solver.maneuverNodes[i-1];
                solver.RemoveManeuverNode(node);
            }

            // add any new ones
            for(int i = solver.maneuverNodes.Count; i < maneuversLength; i++)
            {
                var maneuver = this.Maneuvers[i];
                ManeuverNode node = solver.AddManeuverNode(maneuver.UniversalTime);
                node.DeltaV = maneuver.DeltaV;
                node.OnGizmoUpdated(node.DeltaV, node.UT);
            }
        }

        public override bool Equals(object obj)
        {
            if(obj == null)
                return false;

            return this.Equals(obj as ManeuverList);
        }

        public bool Equals(ManeuverList list)
        {
            if(list == null)
                return false;

            if(this.Maneuvers == null && list.Maneuvers == null)
                return true;

            if(this.Maneuvers == null)
                return false;

            if(list.Maneuvers == null)
                return false;

            if(this.Maneuvers.Length != list.Maneuvers.Length)
                return false;

            for(int i = 0; i < this.Maneuvers.Length; i++)
            {
                if(!this.Maneuvers[i].Equals(list.Maneuvers[i]))
                   return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            // the magic numbers in here are primes
            int hash = 3271;
            if(this.Maneuvers == null)
                return hash;

            foreach(var maneuver in this.Maneuvers)
                hash = (hash * 1327) + maneuver.GetHashCode();

            return hash;
        }
    }
}