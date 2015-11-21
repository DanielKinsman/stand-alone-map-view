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

using ProtoBuf;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace StandAloneMapView.comms
{
    public class Save
    {
        public const string SAVEFILE = "samv_sync";
        public const string SAVEFILENAME = "samv_sync.sfs";
        public static string SavePath
        {
            get
            {
                var path = Path.Combine(
                                Path.Combine(KSPUtil.ApplicationRootPath, "saves"),
                                HighLogic.SaveFolder);

                return Path.Combine(path, SAVEFILENAME);
            }
        }

        public byte[] saveContent;

        public Save()
        {
        }

        public static Save FromCurrentGame()
        {
            if(HighLogic.CurrentGame == null)
                throw new InvalidOperationException("Cannot send the current game - no game has been started");

            // I would love to just use Game.Save() here, but nothing
            // apart from GamePersistnce.SaveGame() will actully update
            // the current flight state (e.g. when going on eva) immediately.
            GamePersistence.SaveGame(SAVEFILE, HighLogic.SaveFolder, SaveMode.OVERWRITE);
            return new Save() {saveContent=File.ReadAllBytes(SavePath)};
            // perhaps read as text instead to avoid line endings and other platform problems?
        }

        public void Send(Stream stream)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, this.saveContent);
        }

        public static void ReadAndSave(Stream stream, string file, object fileLock)
        {
            var formatter = new BinaryFormatter();
            var saveContent = (byte[])formatter.Deserialize(stream);

            lock(fileLock)
            {
                File.WriteAllBytes(file, saveContent);
            }
        }
    }
}