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

using System;

namespace StandAloneMapView.client
{
    [Serializable]
    public class Settings
    {
        public const string SETTINGS_NODENAME = "samv_client";
        public const string SETTINGS_FILENAME = "samv_client.cfg";
        public static string Path
        {
            get
            {
                // KSP stores settings in the application path. That is naughty.
                // Put settings instead in the saves directory, which people like
                // myself are inclined to move to their home directory.
                var path = System.IO.Path.Combine(KSPUtil.ApplicationRootPath, "saves");
                return System.IO.Path.Combine(path, SETTINGS_FILENAME);
            }
        }

        // tcp server host
        [Persistent]
        public string Server;

        // tcp server port
        [Persistent]
        public int ServerPort;

        // udp receive port
        [Persistent]
        public int RecievePort;

        [Persistent]
        public bool StartAutomatically;

        public Settings()
        {
            this.Server = utils.Settings.DEFAULT_SERVER;
            this.ServerPort = utils.Settings.DEFAULT_SERVER_PORT;
            this.RecievePort = utils.Settings.DEFAULT_CLIENT_PORT;
            this.StartAutomatically = false;
        }

        public void Save()
        {   
            utils.Settings.Save(Settings.Path, this, SETTINGS_NODENAME);
        }

        public static Settings Load()
        {
            if(!System.IO.File.Exists(Settings.Path))
                return new Settings();

            return utils.Settings.Load<Settings>(Settings.Path, SETTINGS_NODENAME);
        }
    }
}