/*

Copyright 2018 Daniel Kinsman.

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
using UnityEngine;

namespace StandAloneMapView.utils
{
    public static class Saves
    {
        private static GameScenes _startScene = GameScenes.SPACECENTER;
        private static MonoBehaviourExtended _logger = null;

        public static void Load(
            string saveDir, string file="persistent",
            GameScenes startScene=GameScenes.SPACECENTER, MonoBehaviourExtended logger=null)
        {
            _logger = logger;
            var node = GamePersistence.LoadSFSFile(file, saveDir);
            if(node == null)
            {
                LogDebug("Couldn't load sfs {0}, {1}", "default", "persistent");
                return;
            }

            // total HACK because I don't have python's functools.partial
            _startScene = startScene;
            KSPUpgradePipeline.Process(
                node, "persistent", SaveUpgradePipeline.LoadContext.SFS, OnLoadPipe, OnLoadPipeFail);
        }

        public static void OnLoadPipe(ConfigNode node)
        {
            LogDebug("Did the upgrade pipeline");
            HighLogic.CurrentGame = GamePersistence.LoadGameCfg(node, "default", true, false);
            if(HighLogic.CurrentGame == null)
            {
                LogDebug("Couldn't load game config node");
                return;
            }
            HighLogic.CurrentGame.startScene = _startScene;
            HighLogic.SaveFolder = "default";
            HighLogic.CurrentGame.Start();
        }

        public static void OnLoadPipeFail(KSPUpgradePipeline.UpgradeFailOption o, ConfigNode n)
        {
            LogDebug("Failed to do the upgrade pipeline");
        }

        public static void LogDebug(string message, params object[] formatParams)
        {
            if(_logger != null)
                _logger.LogDebug(message, formatParams);
        }
    }
}
