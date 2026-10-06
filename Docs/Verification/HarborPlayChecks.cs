// Run via Unity Pipeline run_script while HarborPrototype is in Play mode.
// Uses the actual CharacterController, scene colliders and motor; no mock physics.
using System;
using System.Collections.Generic;
using SalvageCrew;
using UnityEngine;

public static class HarborPlayChecks
{
    public static Dictionary<string, object> Run()
    {
        if (!Application.isPlaying) throw new InvalidOperationException("Play mode required.");
        var p = UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        var cc = p.GetComponent<CharacterController>();
        var results = new Dictionary<string, object>();
        p.enabled = false;
        void Assert(bool value, string name, object detail)
        {
            if (!value) throw new Exception(name + " failed: " + detail);
            results[name] = detail;
        }
        void Place(Vector3 pos, float yaw)
        {
            p.ReturnToSpawn(); cc.enabled = false;
            p.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
            cc.enabled = true; Physics.SyncTransforms();
            for (int i = 0; i < 5; i++) p.Step(default, 1f / 60f);
        }
        float Travel(Vector2 move, int frames)
        {
            Place(new Vector3(0, 1.25f, 3), 0);
            Vector3 start = p.transform.position;
            for (int i = 0; i < frames; i++) p.Step(new LocalPlayerInput.Sample { Move = move }, 1f / frames);
            return Vector2.Distance(new Vector2(start.x, start.z), new Vector2(p.transform.position.x, p.transform.position.z));
        }
        try
        {
            float at30 = Travel(Vector2.up, 30), at120 = Travel(Vector2.up, 120);
            Assert(Mathf.Abs(at30 - at120) < .03f && Mathf.Abs(at30 - 4) < .05f, "FrameRateIndependent", new[] { at30, at120 });
            float straight = Travel(Vector2.up, 60);
            // Diagonal travels on the wide shore so it cannot fall off the narrow pier.
            Place(new Vector3(0, 1.25f, -8), 0);
            Vector3 diagonalStart = p.transform.position;
            for (int i = 0; i < 60; i++) p.Step(new LocalPlayerInput.Sample { Move = Vector2.one }, 1f / 60f);
            float diagonal = Vector2.Distance(new Vector2(diagonalStart.x, diagonalStart.z), new Vector2(p.transform.position.x, p.transform.position.z));
            Assert(Mathf.Abs(diagonal - straight) < .05f, "DiagonalSpeed", new[] { straight, diagonal });
            Place(new Vector3(0, 1.25f, -8), 0);
            Vector3 sprintStart = p.transform.position;
            for (int i = 0; i < 60; i++) p.Step(new LocalPlayerInput.Sample { Move = Vector2.up, Sprint = true }, 1f / 60f);
            Assert(Mathf.Abs(p.transform.position.z - sprintStart.z - 6.5f) < .05f, "Sprint", p.transform.position.z - sprintStart.z);

            Place(new Vector3(0, 1.25f, 7.5f), 90);
            for (int i = 0; i < 100; i++) p.Step(new LocalPlayerInput.Sample { Move = Vector2.up }, 1f / 60f);
            Assert(p.transform.position.x > 6.3f && p.transform.position.y > 1.45f && p.Grounded, "PierRampDeck", p.transform.position.ToString());
            for (int i = 0; i < 120; i++) p.Step(new LocalPlayerInput.Sample { Move = Vector2.up }, 1f / 60f);
            Assert(p.transform.position.x < 8.65f && p.transform.position.x > 8.3f, "RailCollision", p.transform.position.ToString());
            for (int i = 0; i < 120; i++) p.Step(default, 1f / 60f);
            Assert(p.transform.position.y > 1.45f && p.Grounded, "DeckGrounding", p.transform.position.ToString());
            Place(new Vector3(7, 1.55f, 10), 0);
            for (int i = 0; i < 60; i++) p.Step(new LocalPlayerInput.Sample { Move = Vector2.up }, 1f / 60f);
            Assert(p.transform.position.z < 10.95f, "CabinCollision", p.transform.position.ToString());

            Place(new Vector3(0, 1.25f, 4), 0);
            float floor = p.transform.position.y;
            p.Step(new LocalPlayerInput.Sample { Jump = true }, 1f / 60f);
            float firstVelocity = p.VerticalSpeed;
            p.Step(new LocalPlayerInput.Sample { Jump = true }, 1f / 60f);
            Assert(firstVelocity > 5 && p.VerticalSpeed < firstVelocity && p.transform.position.y > floor, "GroundOnlyJump", new[] { firstVelocity, p.VerticalSpeed });
            float peak = p.transform.position.y;
            for (int i = 0; i < 100; i++) { p.Step(default, 1f / 60f); peak = Mathf.Max(peak, p.transform.position.y); }
            Assert(peak - floor > .85f && peak - floor < 1.1f && p.Grounded, "JumpHeightAndLanding", peak - floor);
            p.Step(new LocalPlayerInput.Sample { Look = new Vector2(100, 10000) }, 1f / 60f);
            float pitch = p.GetComponentInChildren<Camera>().transform.parent.localEulerAngles.x;
            Assert(Mathf.Abs(Mathf.DeltaAngle(0, pitch)) <= 80.01f, "PitchClamp", pitch);
            p.ReturnToSpawn();
            Assert(p.VerticalSpeed == 0 && Vector3.Distance(p.transform.position, new Vector3(0, 1.25f, 4)) < .01f, "ResetState", p.transform.position.ToString());
            cc.enabled = false; p.transform.position = new Vector3(15, -5, 15); cc.enabled = true;
            p.Step(default, 1f / 60f);
            Assert(p.VerticalSpeed == 0 && Vector3.Distance(p.transform.position, new Vector3(0, 1.25f, 4)) < .01f, "FallRecovery", p.transform.position.ToString());
            return results;
        }
        finally { p.ReturnToSpawn(); p.enabled = true; }
    }
}
