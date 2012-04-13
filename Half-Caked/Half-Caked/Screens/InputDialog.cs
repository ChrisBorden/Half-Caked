using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Half_Caked
{
    class InputDialog : ContentBoxScreen
    {
        int cursorPosition = 0;

        public InputDialog(string msg, string def)
            : base(msg, def)
        {
            cursorPosition = def.Length;
        }

        public override void HandleInput(InputState input)
        {
            string text = input.GetTextSinceUpdate(ControllingPlayer);
            text = text.Replace("\n", "");

            PlayerIndex outs;

            if(text.Length > 0)
            {
                int index = text.IndexOf('\b');
                text = text.Replace("\b", "");

                int displacement = (cursorPosition > 0 && index != -1 ? -1 : 0);

                Content = Content.Substring(0, cursorPosition + displacement) + text + Content.Substring(cursorPosition);
                cursorPosition += text.Length + displacement;
            }
            else if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Left, null, out outs))
                cursorPosition--;
            else if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.Right, null, out outs))
                cursorPosition++;
            else
                base.HandleInput(input);            

            cursorPosition = (int)MathHelper.Clamp(cursorPosition, 0, Content.Length);
        }
    }
}
