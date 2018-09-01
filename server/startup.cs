/*

Copyright 2014-2018 Daniel Kinsman.

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
using UnityEngine;


namespace StandAloneMapView.server
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Startup : utils.MonoBehaviourExtended
    {
        public Settings Settings;

        public Startup()
        {
            this.LogPrefix = "samv server";
        }

        public override void Start()
        {
            this.Settings = Settings.Load();
#if DEBUG_START_SERVER
            // Automatically load default save for quicker testing
            StandAloneMapView.utils.Saves.Load("default");
#endif
        }
    }
}
