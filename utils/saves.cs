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
        public static void Load(string slot, string file="persistent", MonoBehaviourExtended logger=null)
        {
            var node = GamePersistence.LoadSFSFile(file, slot);
            if(node == null)
            {
                LogDebug(logger, "Couldn't load sfs {0}, {1}", "default", "persistent");
                return;
            }

            KSPUpgradePipeline.Process(
                node, "persistent", SaveUpgradePipeline.LoadContext.SFS, OnLoadPipe, OnLoadPipeFail);
        }

        public static void OnLoadPipe(ConfigNode node)
        {
            LogDebug(null, "Did the upgrade pipeline");
            HighLogic.CurrentGame = GamePersistence.LoadGameCfg(node, "default", true, false);
            if(HighLogic.CurrentGame == null)
            {
                LogDebug(null, "Couldn't load game config node");
                return;
            }
            HighLogic.CurrentGame.startScene = GameScenes.SPACECENTER;
            HighLogic.SaveFolder = "default";
            HighLogic.CurrentGame.Start();
        }

        public static void OnLoadPipeFail(KSPUpgradePipeline.UpgradeFailOption o, ConfigNode n)
        {
            LogDebug(null, "Failed to do the upgrade pipeline");
        }

        public static void LogDebug(MonoBehaviourExtended logger, string message, params object[] formatParams)
        {
            if(logger != null)
                logger.LogDebug(message, formatParams);
        }
    }
}
