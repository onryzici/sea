using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalvageCrew;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Live host helm + real Input System events. Focus callback is SIMULATED, not native OS focus.
public static class BoatInputChecks
{
    public static async Task<Dictionary<string, object>> Run()
    {
        var session = HarborSession.Instance;
        var player = session.LocalPlayer;
        var boat = NetworkBoat.Instance;
        if (player == null || !player.Driving || !boat.IsServer) throw new Exception("Host must hold helm first.");
        var results = new Dictionary<string, object>();
        void Check(bool ok, string name) { results[name] = ok; if (!ok) throw new Exception(name); }
        try
        {
            player.Motor.enabled = true;
            player.Input.SendMessage("OnApplicationFocus", true); session.SetPanelOpen(false);
            var local = boat.transform.InverseTransformPoint(player.transform.position);
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W, Key.Space));
            await Task.Delay(500);
            Check(boat.Drive.y > .9f, "WDrivesServerBoat");
            Check(Vector3.Distance(local, boat.transform.InverseTransformPoint(player.transform.position)) < .04f, "HelmDisablesWalkingAndJump");
            int frame = Time.frameCount;
            player.Input.SendMessage("OnApplicationFocus", false);
            await Task.Delay(500);
            Check(boat.Drive == Vector2.zero && player.Input.Read().Move == Vector2.zero, "SimulatedFocusLossZerosThrottleAndMovement");
            Check(Time.frameCount > frame + 5 && Application.runInBackground, "BackgroundFramesAndSimulationContinue");
            player.Input.SendMessage("OnApplicationFocus", true); session.SetPanelOpen(true);
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W));
            await Task.Delay(350);
            Check(boat.Drive == Vector2.zero && Cursor.lockState == CursorLockMode.None, "PanelBlocksDriveAndReleasesCursor");
            return results;
        }
        finally
        {
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
            player.Input.SendMessage("OnApplicationFocus", true); session.SetPanelOpen(false);
            player.RequestToggle();
        }
    }
}
