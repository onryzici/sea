using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalvageCrew;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Actual Input System events; focus callback is explicitly simulated (not a native window test).
public static class SessionInputChecks
{
    public static async Task<Dictionary<string, object>> Run()
    {
        var session = HarborSession.Instance;
        var motor = UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        var input = motor.GetComponent<LocalPlayerInput>();
        var result = new Dictionary<string, object>();
        void Check(bool ok, string name) { result[name] = ok; if (!ok) throw new Exception(name); }
        try
        {
            input.SendMessage("OnApplicationFocus", true); session.SetPanelOpen(false);
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W));
            await Task.Delay(100);
            Check(input.Read().Move.y > .9f, "FocusedW");
            input.SendMessage("OnApplicationFocus", false);
            Vector3 before = motor.transform.position;
            await Task.Delay(300);
            Check(input.Read().Move == Vector2.zero && Vector3.Distance(before, motor.transform.position) < .02f, "FocusLossStopsHeldW");
            input.SendMessage("OnApplicationFocus", true); session.SetPanelOpen(true);
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W, Key.E, Key.R));
            await Task.Delay(100);
            var sample = input.Read();
            Check(!input.GameplayActive && sample.Move == Vector2.zero && !sample.Interact && !sample.Reset, "PanelBlocksGameplay");
            Check(Cursor.lockState == CursorLockMode.None, "PanelReleasesCursor");
            Check(Application.runInBackground, "BackgroundSimulationEnabled");
            return result;
        }
        finally
        {
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
            input.SendMessage("OnApplicationFocus", true); session.SetPanelOpen(true); motor.ReturnToSpawn();
        }
    }
}
