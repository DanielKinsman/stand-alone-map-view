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
        public const string TOOLBAR_NAMESPACE = "standalonemapview";
        public const string TOOLBAR_ICON = "samv_server/toolbaricon";
        public const string TOOLBAR_ID = "samv_settings";
        protected utils.IButton showGUIButton;

        public Startup()
        {
            this.LogPrefix = "samv server";
        }

        public override void Awake()
        {
            this.WindowCaption = "Stand alone map view settings";
            ToolbarSetup();
        }

        public override void Start()
        {
            this.Settings = Settings.Load();
#if DEBUGfu
            // Automatically load default save for quicker testing
            HighLogic.SaveFolder = "default";
            var game = GamePersistence.LoadGame("persistent",
                                        HighLogic.SaveFolder, true, false);
            game.startScene = GameScenes.SPACECENTER;
            game.Start();
#endif
        }

        public override void OnDestroy()
        {
            // Delete these lines and the toobar button will show up in
            // the space center, but not on the main menu.
            // Looks like a bug in the toolbar.
            if(this.showGUIButton != null)
                this.showGUIButton.Destroy();
        }

        public void ToolbarSetup()
        {
            if(utils.ToolbarManager.ToolbarAvailable)
            {
                this.ShowGUI = true; // set to false when toolbar is working
                this.showGUIButton = utils.ToolbarManager.Instance.add(
                    TOOLBAR_NAMESPACE, TOOLBAR_ID);
                this.showGUIButton.Visible = true;
                this.showGUIButton.ToolTip = this.WindowCaption;
                this.showGUIButton.TexturePath = TOOLBAR_ICON;
                this.showGUIButton.OnClick += (e) => this.ShowGUI = !this.ShowGUI;
            }
            else
            {
                this.ShowGUI = true;
            }
        }

        public override void DrawWindow(int id)
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

            GUI.DragWindow();
        }
    }
}

