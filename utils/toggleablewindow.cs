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

using UnityEngine;

namespace StandAloneMapView.utils
{
    public class ToggleableWindow
    {
        public bool IsOn = false;
        public Texture2D buttonTexture = null;
        public GUIStyle buttonStyle = null;

        protected bool _wasToggled = false;
        public bool WasToggled
        {
            get
            {
                bool was = this._wasToggled;
                this._wasToggled = false;
                return was;
            }
        }

        public int CompactedWidth
        {
            get
            {
                return this.buttonTexture.width + this.buttonStyle.padding.horizontal;
            }
        }

        public int CompactedHeight
        {
            get
            {
                return this.buttonTexture.height + this.buttonStyle.padding.vertical;
            }
        }

        public ToggleableWindow(string textureUrl)
        {
            this.buttonTexture = GameDatabase.Instance.GetTexture(
                textureUrl, false);
        }

        public void OnGUI()
        {
            if(this.buttonStyle != null)
                return;

            this.buttonStyle = new GUIStyle(GUI.skin.button);
            this.buttonStyle.margin = new RectOffset();
            this.buttonStyle.padding = new RectOffset(5, 5, 5, 5);
        }

        public void DrawWindow()
        {
            bool pressed = GUILayout.Button(this.buttonTexture, 
                             this.buttonStyle,
                             GUILayout.Width(this.CompactedWidth),
                             GUILayout.Height(this.CompactedHeight) );

            if(pressed)
            {
                this._wasToggled = true;
                this.IsOn = !this.IsOn;
            }
        }
    }
}