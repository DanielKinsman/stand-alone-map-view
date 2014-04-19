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

        public utils.ToggleableWindow toggleWindow;

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

            this.ShowGUI = true;
            this.toggleWindow = new utils.ToggleableWindow("samv_client/icon");
            this.WindowCaption = "samv";
            this.WindowBounds.width = 1;
            this.WindowBounds.height = 1;
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

        public override void DrawGUI()
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