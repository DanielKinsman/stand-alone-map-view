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
            public string name {get; private set;}
            public byte type {get; private set;}

            public VesselInfo(global::Vessel kspVessel)
            {
                this.name = kspVessel.name;
                this.type = (byte)kspVessel.vesselType;
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