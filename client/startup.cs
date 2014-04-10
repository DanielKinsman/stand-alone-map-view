using KSP;
using UnityEngine;

namespace StandAloneMapView
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class Startup : MonoBehaviour
	{
		public void Start()
		{
			HighLogic.SaveFolder = "samv";
			//todo create it, wipe it, newgame it whatever
			var game = GamePersistence.LoadGame("persistent", HighLogic.SaveFolder, true, false);

			//todo prevent saving of any kind
			//FlightAutoSave.fetch.bypassAutoSave = true;
			//FlightDriver.BypassPersistence = true;
			//FlightDriver.CanRevert = false;

			//todo connect to server, update vessels, Planetarium, time etc

			// todo go to active vessel, if no active vessel go to tracking station

			if(game != null && game.flightState != null && game.compatible)
            {
                FlightDriver.StartAndFocusVessel(game, 0);
            }
		}
	}
}