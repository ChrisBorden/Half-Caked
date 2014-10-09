using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    class KeybindingDialog : ContentBoxScreen
    {
        Keybinding mNewKey = null;
        string mKeybinding = null;
        ReturnKeybindingInput mReturnMethod = null;

        public KeybindingDialog(string keybinding, ReturnKeybindingInput method)
            : base("Set the keybindings for: " + keybinding, "", new string[] { "Accept", "Cancel" }, -1)
        {
            this.mKeybinding = keybinding;            
            this.mReturnMethod = method;
            
            // Hook up menu event handlers.
            Buttons[0].Pressed += AcceptSelected;

            Buttons[0].State = UIState.Inactive;
            Buttons[1].State = UIState.Inactive;
        }

        public delegate void ReturnKeybindingInput(Keybinding input);

        public override void HandleInput(InputState input)
        {
            Keybinding tmpKey = input.GetNewestKeybindingPressed(this.ControllingPlayer);

            if (tmpKey != null && mNewKey == null) 
            {
                Content = "Key [" + tmpKey.ToString() + "] Pressed";
                mNewKey = tmpKey;
                
                this.mSelectedButton = 0;

                Buttons[0].State = UIState.Selected;
                Buttons[1].State = UIState.Active;
            }
            else
                base.HandleInput(input);

        }

        void AcceptSelected(object sender, PlayerIndexEventArgs e)
        {
            // Tell the Keybinding screen what was selected...
            this.mReturnMethod.Invoke(this.mNewKey);
        }
    }
}
