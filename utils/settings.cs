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

namespace StandAloneMapView.utils
{
    public static class Settings
    {
        // tcp connection defaults
        public const string DEFAULT_SERVER = "localhost";
        public const int DEFAULT_SERVER_PORT = 26718;

        // udp connection defaults
        public const string DEFAULT_CLIENT = "localhost";
        public const int DEFAULT_CLIENT_PORT = 26719;

        public static void Save(string path, object obj, string nodeName)
        {
            var node = new ConfigNode(nodeName);
            node = ConfigNode.CreateConfigFromObject(obj, node);

            var save = new ConfigNode();
            save.AddNode(node);
            save.Save(path);
        }

        public static T Load<T>(string path, string nodeName) where T : new()
        {
            var obj = new T();
            var node = ConfigNode.Load(path);
            node = node.GetNode(nodeName);
            ConfigNode.LoadObjectFromConfig(obj, node);
            return obj;
        }
    }
}
