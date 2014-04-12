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
using System.IO;
using UnityEngine;

namespace StandAloneMapView
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class Startup : utils.MonoBehaviourExtended
	{
		public override void Start()
		{
			const string SAVEFILE = "persistent";
			const string SAVEFILENAME = "persistent.sfs";
			const string SAVEDIRECTORY = "stand_alone_map_viewer_dont_touch";

			try
			{
				HighLogic.SaveFolder = SAVEDIRECTORY;

				// KSP is naughty and stores savegames in the application directory.
				var path = Path.Combine(
									Path.Combine(KSPUtil.ApplicationRootPath, "saves"),
									HighLogic.SaveFolder);

				Directory.CreateDirectory(path); //safe if already exists

				var origin = Path.Combine(
					Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
					SAVEFILENAME);
				var destination = Path.Combine(path, SAVEFILENAME);
				File.Copy(origin, destination, true);

				var game = GamePersistence.LoadGame(SAVEFILE, HighLogic.SaveFolder, true, false);
				game.startScene = GameScenes.TRACKSTATION;
				game.Start();
			}
			catch(Exception e)
			{
				LogException(e);
				throw;
			}
		}
	}
}