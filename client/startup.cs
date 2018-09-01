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
using System.Reflection;

namespace StandAloneMapView.client
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Startup : utils.MonoBehaviourExtended
    {
        public const string BASESAVEFILE = "base_save";
        public const string SAVEFILE = "persistent";
        public const string SAVEDIRECTORY = "stand_alone_map_viewer_dont_touch";

        public static string KspSaveDirectory
        {
            get
            {
                // KSP is naughty and stores savegames in the application directory.
                var saveDir = Path.Combine(
                    Path.Combine(KSPUtil.ApplicationRootPath, "saves"), Startup.SAVEDIRECTORY);
                Directory.CreateDirectory(saveDir); //safe if already exists
                return saveDir;
            }
        }

        public Settings Settings;
        public bool start = false;

        public Startup()
        {
            this.LogPrefix = "samv";
        }

        public override void Awake()
        {
            Startup.CopyBaseSave();
        }

        public override void Start()
        {
            try
            {
                TcpWorker.Instance.Start();
                this.Settings = Settings.Load();
                this.Reset();
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public override void Update()
        {
            var message = TcpWorker.Instance.logMessages.TryPop(null);
            if(message != null)
                LogDebug(message);

            if(TcpWorker.Instance.VesselUpdateReceived.WaitOne(0) || this.start)
            {
                this.start = false;
                try
                {
                    StandAloneMapView.utils.Saves.Load(
                        Startup.SAVEDIRECTORY, Startup.SAVEFILE, GameScenes.TRACKSTATION, this);
                    // None of the settings below actually work :/
                    HighLogic.CurrentGame.Parameters.Difficulty.EnableCommNet = false;
                    HighLogic.CurrentGame.Parameters.Difficulty.IndestructibleFacilities = true;
                    HighLogic.CurrentGame.Parameters.Difficulty.ReentryHeatScale = 0.0f;
                    CheatOptions.NoCrashDamage = true;
                    CheatOptions.UnbreakableJoints = true;
                    CheatOptions.InfiniteElectricity = true;
                }
                catch(Exception e)
                {
                    LogException(e);
                }
            }
        }

        public void Reset()
        {
            TcpWorker.Instance.Stop();
            TcpWorker.Instance.Start();
            this.start = this.Settings.StartAutomatically;
        }

        public static void CopyBaseSave()
        {
            var saveFile = Path.Combine(Startup.KspSaveDirectory, Startup.SAVEFILE + ".sfs");
            var sourceSaveFile = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Startup.BASESAVEFILE + ".sfs");

            File.Copy(sourceSaveFile, saveFile, true);
        }
    }
}
