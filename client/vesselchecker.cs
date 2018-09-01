using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using StandAloneMapView.utils;

namespace StandAloneMapView.client
{
    public class VesselChecker
    {
        protected ConfigNode DummyVesselConfigNode;
        protected string DummyPath;
        protected MonoBehaviourExtended Logger; // totally cheating

        public VesselChecker(MonoBehaviourExtended logger)
        {
            this.Logger = logger;
            var dummyPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "dummy.protovessel");
            this.DummyVesselConfigNode = ConfigNode.Load(dummyPath);
        }

        public void Check()
        {
            try
            {
                var vesselList = TcpWorker.Instance.Vessels;
                if(vesselList == null)
                    return;

                var serverVessels = vesselList.Vessels;
                var vesselsToKill = FlightGlobals.Vessels.Where(v => !serverVessels.ContainsKey(v.id)).ToList();
                foreach(var v in vesselsToKill)
                {
                    this.Logger.LogDebug("Vessel {0} ({1}) does not exist on server, killing it", v.vesselName, v.id);
                    v.Die();
                }

                // Could be faster to keep this around and maintain it rather than reconstructing it
                // every time.
                var vesselIds = new HashSet<Guid>(FlightGlobals.Vessels.Select(v => v.id));
                var vesselsToAdd = serverVessels.Where(p => !vesselIds.Contains(p.Key)).Select(p => p.Value);
                foreach(var v in vesselsToAdd)
                    CreateVessel(v);
            }
            catch(Exception e)
            {
                this.Logger.LogException(e);
                throw;
            }
        }

        public void CreateVessel(comms.VesselList.VesselInfo vesselInfo)
        {
            try
            {
                this.Logger.LogDebug("creating vessel {0} {1}", vesselInfo.name, vesselInfo.id);
                var pv = new ProtoVessel(this.DummyVesselConfigNode, HighLogic.CurrentGame);

                pv.vesselID = vesselInfo.id;
                pv.vesselName = vesselInfo.name;
                pv.vesselType = (VesselType)vesselInfo.type;
                pv.altitude = vesselInfo.altitude;
                pv.landed = vesselInfo.landed;
                pv.landedAt = vesselInfo.landedAt;
                pv.latitude = vesselInfo.latitude;
                pv.longitude = vesselInfo.longitude;
                pv.situation = (Vessel.Situations)vesselInfo.situation;
                pv.splashed = vesselInfo.splashed;
                pv.orbitSnapShot = new OrbitSnapshot(vesselInfo.orbit.GetKspOrbit(FlightGlobals.Bodies));

                pv.Load(HighLogic.CurrentGame.flightState);
            }
            catch(Exception e)
            {
                this.Logger.LogException(e);
                throw;
            }
        }
    }
}
