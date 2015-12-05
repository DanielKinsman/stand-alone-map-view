using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace StandAloneMapView.comms
{
    [Serializable]
    public class VesselList
    {
        [Serializable]
        public class VesselInfo
        {
            public Guid id {get; private set;}
            public string name {get; private set;}
            public byte type {get; private set;}
            public bool landed {get; private set;}
            public string landedAt {get; private set;}
            public bool splashed {get; private set;}
            public byte situation {get; private set;}
            public double altitude {get; private set;}
            public double latitude {get; private set;}
            public double longitude {get; private set;}
            public Orbit orbit {get; private set;}

            public VesselInfo()
            {
            }

            public VesselInfo(global::Vessel kspVessel)
            {
                this.id = kspVessel.id;
                this.name = kspVessel.vesselName;
                this.type = (byte)kspVessel.vesselType;
                this.landed = kspVessel.Landed;
                this.landedAt = kspVessel.landedAt;
                this.splashed = kspVessel.Splashed;
                this.situation = (byte)kspVessel.situation;
                this.altitude = kspVessel.altitude;
                this.latitude = kspVessel.latitude;
                this.longitude = kspVessel.longitude;
                this.orbit = new Orbit(kspVessel.orbit);
            }
        }

        public IDictionary<Guid, VesselInfo> Vessels {get; private set;}

        public VesselList()
        {
            this.Vessels = new Dictionary<Guid, VesselInfo>();
        }

        public static VesselList FromCurrentGame()
        {
            var vessels = new VesselList();

            foreach(var v in FlightGlobals.Vessels)
                vessels.Vessels.Add(v.id, new VesselInfo(v));

            return vessels;
        }

        public static VesselList FromStream(Stream stream)
        {
            var formatter = new BinaryFormatter();
            return (VesselList)formatter.Deserialize(stream);
        }

        public void Send(Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }
    }
}