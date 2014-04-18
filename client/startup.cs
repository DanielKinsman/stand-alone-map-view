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

namespace StandAloneMapView.client
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class Startup : utils.MonoBehaviourExtended
    {
        public const string SAVEFILE = "persistent";
        public const string SAVEFILENAME = "persistent.sfs";
        public const string SAVEDIRECTORY = "stand_alone_map_viewer_dont_touch";

        public static string SavePath
        {
            get
            {
                // KSP is naughty and stores savegames in the application directory.
                var path = Path.Combine(
                                Path.Combine(KSPUtil.ApplicationRootPath, "saves"),
                                SAVEDIRECTORY);

                Directory.CreateDirectory(path); //safe if already exists

                return Path.Combine(path, SAVEFILENAME);
            }
        }

        public Settings Settings;

        public const string TOOLBAR_NAMESPACE = "standalonemapview";
        public const string TOOLBAR_ICON = "samv_client/toolbaricon";
        public const string TOOLBAR_ID = "samv_settings";
        protected utils.IButton showGUIButton;

        public bool start = false;
        public bool firstLoadDone = false;
        public TextButton3D startButton;

        public Startup()
        {
            this.LogPrefix = "samv";
        }

        public override void Awake()
        {
            var menu = FindObjectOfType<MainMenu>();
            menu.startBtn.GetComponent<TextMesh>().text = "";
            this.startButton = (TextButton3D)TextButton3D.Instantiate(menu.startBtn);
            this.startButton.GetComponent<TextMesh>().text = "Waiting for server sync...";
            this.startButton.onPressed = new Callback(StartButtonPressed);
            Destroy(menu.startBtn);

            this.WindowCaption = "Stand alone map view settings";
            this.ToolbarSetup();
        }

        public void ToolbarSetup()
        {
            if(utils.ToolbarManager.ToolbarAvailable)
            {
                this.ShowGUI = true; // set to false when toolbar is working
                this.showGUIButton = utils.ToolbarManager.Instance.add(
                    TOOLBAR_NAMESPACE, TOOLBAR_ID);
                this.showGUIButton.ToolTip = this.WindowCaption;
                this.showGUIButton.TexturePath = TOOLBAR_ICON;
                this.showGUIButton.OnClick += (e) => this.ShowGUI = !this.ShowGUI;
            }
            else
            {
                this.ShowGUI = true;
            }
        }

        public void StartButtonPressed()
        {
            this.start = true;
        }

        public override void Start()
        {
            try
            {
                TcpWorker.Instance.Start();
                CheatOptions.NoCrashDamage = true;
                CheatOptions.UnbreakableJoints = true;

                this.Settings = Settings.Load();
                this.Reset();
            }
            catch(Exception e)
            {
                LogException(e);
                throw;
            }
        }

        public override void OnDestroy()
        {
            // Delete these lines and the toobar button will show up in
            // the tracking station, but not on the main menu.
            // Looks like a bug in the toolbar.
            if(this.showGUIButton != null)
                this.showGUIButton.Destroy();
        }

        public override void Update()
        {
            if(this.firstLoadDone)
                this.startButton.GetComponent<TextMesh>().text = "Start map view";

            if(TcpWorker.Instance.AtLeastOneSaveReceived.WaitOne(0))
                this.firstLoadDone = true;

            if(this.firstLoadDone && this.start)
            {
                this.start = false;
                LoadSave();
            }
        }

        public void Reset()
        {
            TcpWorker.Instance.Stop();
            TcpWorker.Instance.Start();
            this.start = this.Settings.StartAutomatically;
            this.firstLoadDone = false;
            this.startButton.GetComponent<TextMesh>().text = "Waiting for server sync...";
        }

        public override void DrawWindow(int id)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("server ip/hostname:");
            this.Settings.Server = GUILayout.TextField(this.Settings.Server);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("server tcp port:");
            this.Settings.ServerPort=Convert.ToInt32(
                GUILayout.TextField(this.Settings.ServerPort.ToString()));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("udp receiving port:");
            this.Settings.RecievePort=Convert.ToInt32(
                GUILayout.TextField(this.Settings.RecievePort.ToString()));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Start automatically:");
            this.Settings.StartAutomatically=Convert.ToBoolean(
                GUILayout.Toggle(this.Settings.StartAutomatically, string.Empty));
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Set and save"))
            {
                this.Settings.Save();
                this.Reset();
            }

            GUI.DragWindow();
        }

        public static void LoadSave()
        {
            HighLogic.SaveFolder = SAVEDIRECTORY;

            Game game;
            lock(TcpWorker.Instance.SaveFileLock)
            {
                game = GamePersistence.LoadGame(SAVEFILE, HighLogic.SaveFolder, true, false);
            }

            game.startScene = GameScenes.TRACKSTATION;
            game.Start();
        }
    }
}