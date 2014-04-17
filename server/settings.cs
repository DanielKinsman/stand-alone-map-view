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

namespace StandAloneMapView.server
{
    [Serializable]
    public class Settings
    {
        public const string SETTINGS_NODENAME = "samv_server";
        public const string SETTINGS_FILENAME = "samv_server.cfg";
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

        // udp client host
        [Persistent]
        public string Client;

        // udp client port
        [Persistent]
        public int ClientPort;

        // tcp port for server to listen on
        [Persistent]
        public int ListenPort;

        public Settings()
        {
            this.Client = utils.Settings.DEFAULT_CLIENT;
            this.ClientPort = utils.Settings.DEFAULT_CLIENT_PORT;
            this.ListenPort = utils.Settings.DEFAULT_SERVER_PORT;
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