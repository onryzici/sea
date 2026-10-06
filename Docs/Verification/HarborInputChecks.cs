// Real Input System events consumed by the running player's normal Update loop.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalvageCrew;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class HarborInputChecks
{
    public static async Task<Dictionary<string, object>> Run()
    {
        var p = UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        var cc = p.GetComponent<CharacterController>();
        var result = new Dictionary<string, object>();
        void Assert(bool condition, string name, object detail)
        { if (!condition) throw new Exception(name + ": " + detail); result[name] = detail; }
        void Place(Vector3 pos)
        {
            p.ReturnToSpawn(); cc.enabled = false;
            p.transform.SetPositionAndRotation(pos, Quaternion.identity);
            cc.enabled = true; Physics.SyncTransforms();
        }
        async Task<Vector3> Hold(params Key[] keys)
        {
            Place(new Vector3(0, 1.25f, -8));
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(keys));
            int frame = Time.frameCount;
            while (Time.frameCount == frame) await Task.Delay(10);
            Vector3 start = p.transform.position;
            float started = Time.time;
            while (Time.time - started < .4f) await Task.Delay(10);
            Vector3 speed = (p.transform.position - start) / (Time.time - started);
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
            frame = Time.frameCount;
            while (Time.frameCount == frame) await Task.Delay(10);
            return speed;
        }
        try
        {
            Assert(Application.isPlaying && p.enabled, "LivePlayer", true);
            var w = await Hold(Key.W); Assert(w.z > .4f, "W", w.ToString());
            var s = await Hold(Key.S); Assert(s.z < -.4f, "S", s.ToString());
            var a = await Hold(Key.A); Assert(a.x < -.4f, "A", a.ToString());
            var d = await Hold(Key.D); Assert(d.x > .4f, "D", d.ToString());
            var run = await Hold(Key.W, Key.LeftShift); Assert(run.z > w.z * 1.3f, "LeftShift", "walk=" + w.z + ", sprint=" + run.z);
            Place(new Vector3(14, -3, 7));
            await Task.Delay(250);
            Assert(Vector3.Distance(p.transform.position, new Vector3(0, 1.23f, 4)) < .1f, "AutomaticFallRecovery", p.transform.position.ToString());
            return result;
        }
        finally
        {
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
            p.ReturnToSpawn();
        }
    }
}
