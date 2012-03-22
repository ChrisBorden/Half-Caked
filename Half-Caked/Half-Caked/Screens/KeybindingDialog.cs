using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Half_Caked
{
    class KeybindingDialog : MessageBoxScreen
    {
        InputState newKeybinding = null;
        String keybinding = null;
        String message = null;
        ReturnKeybindingInput return_method = null;
        private ReturnKeybindingInput returnKeybindingInput;
        public KeybindingDialog(string keybinding, ReturnKeybindingInput method)
            : base("", new string[] {"Accept (Primary)", "Accept (Secondary)", "Cancel"}, 0)
        {
            this.keybinding = keybinding;
            message += "Set the keybindings for: " + keybinding + "\n\n" 
                     + "Press any key to bind... ";
            
            IsPopup = true;
            this.return_method = method;
            
            // Hook up menu event handlers.
            Buttons[0].Pressed += AcceptPrimarySelected;
            Buttons[1].Pressed += AcceptSecondarySelected;
            Buttons[2].Pressed += CancelSelected;

            this.mMessage += message;

        }

        public delegate void ReturnKeybindingInput(InputState input, string whichBinding);

        public override void HandleInput(InputState input)
        {
            // Reset the message to be our "default" + the new keybinding
            this.mMessage = this.message + input.ToString();
            newKeybinding = input;
            base.HandleInput(input);
        }
        void AcceptSecondarySelected(object sender, PlayerIndexEventArgs e)
        {
            // Tell the Keybinding screen what was selected...
            this.return_method.Invoke(this.newKeybinding, "Secondary");
        }
        void AcceptPrimarySelected(object sender, PlayerIndexEventArgs e)
        {
            // Tell the Keybinding screen what was selected...
            this.return_method.Invoke(this.newKeybinding, "Primary");
        }

        void CancelSelected(object sender, PlayerIndexEventArgs e)
        {
            // Do nothing.
            return;
        }
        
    }
}
