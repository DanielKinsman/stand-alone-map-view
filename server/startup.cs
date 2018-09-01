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
using UnityEngine;

namespace StandAloneMapView.server
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Startup : utils.MonoBehaviourExtended
    {
        public Settings Settings;
        //public utils.ToggleableWindow toggleWindow;

        public Startup()
        {
            this.LogPrefix = "samv server";
        }

        /*public override void Awake()
        {
            this.ShowGUI = true;
            this.toggleWindow = new utils.ToggleableWindow("samv_server/icon");
            this.WindowCaption = "samv";
            this.WindowBounds.width = 1;
            this.WindowBounds.height = 1;
        }*/

        public override void Start()
        {
            this.Settings = Settings.Load();
#if DEBUG_START_SERVER
            // Automatically load default save for quicker testing
            HighLogic.SaveFolder = "default";
            var game = GamePersistence.LoadGame("persistent",
                                        HighLogic.SaveFolder, true, false);
            game.startScene = GameScenes.SPACECENTER;
            game.Start();
#endif
        }

        /*public override void DrawGUI()
        {
            if(this.toggleWindow.WasToggled)
            {
                if(this.toggleWindow.IsOn)
                {
                    this.WindowCaption = "Stand alone map view settings";
                    this.WindowBounds.width = 250;
                    this.WindowBounds.height = 100;
                }
                else
                {
                    this.WindowCaption = "samv";
                    this.WindowBounds.width = this.toggleWindow.CompactedWidth;
                    this.WindowBounds.height = this.toggleWindow.CompactedHeight;
                }
            }

            base.DrawGUI();
        }

        public override void OnGUI()
        {
            this.toggleWindow.OnGUI();

            base.OnGUI();
        }

        public override void DrawWindow(int id)
        {
            this.toggleWindow.DrawWindow();

            if(this.toggleWindow.IsOn)
                DrawWindowContents();

            GUI.DragWindow();
        }

        public void DrawWindowContents()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Enabled:");
            this.Settings.Enabled=Convert.ToBoolean(
                GUILayout.Toggle(this.Settings.Enabled, string.Empty));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("client ip/hostname:");
            this.Settings.Client = GUILayout.TextField(this.Settings.Client);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("client udp port:");
            this.Settings.ClientPort=Convert.ToInt32(
                GUILayout.TextField(this.Settings.ClientPort.ToString()));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("tcp listen port:");
            this.Settings.ListenPort=Convert.ToInt32(
                GUILayout.TextField(this.Settings.ListenPort.ToString()));
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Save"))
                this.Settings.Save();
        }*/
    }
}
