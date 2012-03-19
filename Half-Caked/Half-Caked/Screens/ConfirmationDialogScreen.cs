#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;
#endregion

namespace Half_Caked
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    class ConfirmationDialogScreen : GameScreen
    {
        private GuiManager mGUI;
        private Nuclex.Input.InputManager mInput;
        public event EventHandler<PlayerIndexEventArgs> Confirmed;
        public event EventHandler<PlayerIndexEventArgs> Cancelled;

        public ConfirmationDialogScreen(string msg, ScreenManager sm)
        {
            IsPopup = true;

            mGUI = new GuiManager(sm.Game.Services);
            WindowControl confirmationDialog = new WindowControl();

            LabelControl msgLabel = new LabelControl(msg);
            msgLabel.Bounds = new UniRectangle(10.0f, 15.0f, 110.0f, 30.0f);

            ButtonControl okButton = new ButtonControl();
            okButton.Text = "Yes";
            okButton.Pressed += new EventHandler(ConfirmedHandler);
            okButton.Bounds = new UniRectangle(
              new UniScalar(1.0f, -180.0f), new UniScalar(1.0f, -40.0f), 80, 24
            );

            ButtonControl cancelButton = new ButtonControl();
            cancelButton.Text = "No";
            cancelButton.Pressed += new EventHandler(CancelledHandler);
            cancelButton.Bounds = new UniRectangle(
              new UniScalar(1.0f, -90.0f), new UniScalar(1.0f, -40.0f), 80, 24
            );

            confirmationDialog.Bounds = new UniRectangle(100.0f, 100.0f, 512.0f, 384.0f);
            confirmationDialog.Children.Add(msgLabel);
            confirmationDialog.Children.Add(okButton);
            confirmationDialog.Children.Add(cancelButton);

            Screen dialogScreen = new Screen(sm.GraphicsDevice.Viewport.Width * .75f, sm.GraphicsDevice.Viewport.Height * .75f);
            dialogScreen.Desktop.Bounds = new UniRectangle(     new UniScalar(0.1f, 0.0f), new UniScalar(0.1f, 0.0f),       // x and y
                                                                new UniScalar(0.8f, 0.0f), new UniScalar(0.8f, 0.0f)  );    // width and height
            mGUI.Screen = dialogScreen;
            dialogScreen.Desktop.Children.Add(confirmationDialog);

            mInput = new Nuclex.Input.InputManager(sm.Game.Services, sm.Game.Window.Handle);
            sm.Game.Components.Add(mGUI);
            sm.Game.Components.Add(mInput);
            mGUI.Initialize();
        }

        private void ConfirmedHandler(object sender, EventArgs eArgs)
        {
            if (Confirmed != null)
                Confirmed(this, new PlayerIndexEventArgs(PlayerIndex.One));

            ScreenManager.Game.Components.Remove(mGUI);
            ScreenManager.Game.Components.Remove(mInput);
            ScreenManager.Game.Services.RemoveService(typeof(Nuclex.Input.IInputService));
            ScreenManager.Game.Services.RemoveService(typeof(IGuiService));
            ExitScreen();
        }

        private void CancelledHandler(object sender, EventArgs eArgs)
        {
            if (Cancelled != null)
                Cancelled(this, new PlayerIndexEventArgs(PlayerIndex.One));

            ScreenManager.Game.Components.Remove(mGUI);
            ScreenManager.Game.Components.Remove(mInput);
            ScreenManager.Game.Services.RemoveService(typeof(Nuclex.Input.IInputService));
            ScreenManager.Game.Services.RemoveService(typeof(IGuiService));
            ExitScreen();
        }

    }
}
