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
        public const string SAVEFILE = "persistent";
        public const string SAVEFILENAME = "persistent.sfs";
        public const string SAVEDIRECTORY = "stand_alone_map_viewer_dont_touch";

        public static string SavePath
        {
            get
            {
                // KSP is naughty and stores savegames in the application directory.
                var path = Path.Combine(
                                Path.Combine(KSPUtil.ApplicationRootPath, "saves"),
                                SAVEDIRECTORY);

                Directory.CreateDirectory(path); //safe if already exists

                return Path.Combine(path, SAVEFILENAME);
            }
        }

        public bool firstLoadDone = false;

        public override void Start()
        {
            try
            {
                TcpWorker.Instance.Start();
                CheatOptions.NoCrashDamage = true;
                CheatOptions.UnbreakableJoints = true;
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public override void Update()
        {
            if(this.firstLoadDone)
                return;

            if(TcpWorker.Instance.saveReceived.WaitOne(1))
            {
                this.firstLoadDone = true;
                LoadSave();
            }
        }

        public static void LoadSave()
        {
            HighLogic.SaveFolder = SAVEDIRECTORY;
            var game = GamePersistence.LoadGame(SAVEFILE, HighLogic.SaveFolder, true, false);
            game.startScene = GameScenes.TRACKSTATION;
            game.Start();
        }
    }
}