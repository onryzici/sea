using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalvageCrew;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class CarrySafetyChecks
{
    static FirstPersonMotor motor;
    static PhysicsCarry carry;
    static CharacterController cc;
    static Dictionary<string, object> results;
    static void Check(bool ok, string name, object detail) { results[name] = new { passed = ok, detail }; }
    static async Task Wait(float seconds)
    { float end = Time.time + seconds; while (Time.time < end) await Task.Delay(15); }
    static async Task KeyPress(Key key)
    {
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(key));
        await Wait(.09f);
        InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        await Wait(.09f);
    }
    static async Task MouseClick()
    {
        InputSystem.QueueStateEvent(Mouse.current, new MouseState().WithButton(MouseButton.Left));
        await Wait(.09f);
        InputSystem.QueueStateEvent(Mouse.current, new MouseState()); await Wait(.09f);
    }
    static async Task Aim(ScrapItem item, Vector3? playerPosition = null, Vector3? itemPosition = null)
    {
        carry.Drop(); motor.ReturnToSpawn();
        if (itemPosition.HasValue) { item.Body.position = itemPosition.Value; item.Body.linearVelocity = Vector3.zero; }
        else item.Recover();
        cc.enabled = false;
        motor.transform.SetPositionAndRotation(playerPosition ?? new Vector3(.25f, 1.25f, item.Body.position.z), Quaternion.Euler(0, 90, 0));
        cc.enabled = true; Physics.SyncTransforms();
        await Wait(.15f);
        carry.View.LookAt(item.Body.worldCenterOfMass);
        await Wait(.09f);
    }
    public static async Task<Dictionary<string, object>> Run()
    {
        Application.runInBackground = true;
        motor = UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        carry = motor.GetComponent<PhysicsCarry>(); cc = motor.GetComponent<CharacterController>();
        results = new Dictionary<string, object>();
        var items = UnityEngine.Object.FindObjectsByType<ScrapItem>().OrderBy(i => i.Mass).ToArray();
        motor.enabled = true;
        GameObject wall = null;
        try
        {
            await MouseClick();
            await Aim(items[0]);
            await KeyPress(Key.E);
            Check(carry.Held == items[0], "ActualE_Pickup", carry.Held?.name);
            var prompt = GameObject.Find("InteractionPrompt").GetComponent<TMPro.TextMeshProUGUI>();
            Check(prompt.text.Contains("Bırak") && prompt.text.Contains("Hafif"), "HeldHUD", prompt.text);
            await KeyPress(Key.E);
            Check(carry.Held == null && motor.CarrySpeedMultiplier == 1, "ActualE_Drop", true);
            await Aim(items[0]);
            for (int i = 0; i < 8; i++) await KeyPress(Key.E);
            Check(carry.Held == null && !items[0].IsHeld && motor.CarrySpeedMultiplier == 1, "RapidEToggles", true);
            await Aim(items[0]); await KeyPress(Key.E); await KeyPress(Key.Escape);
            Check(Cursor.lockState == CursorLockMode.None && carry.Held == items[0], "EscapeWhileHeld", Cursor.lockState.ToString());
            await KeyPress(Key.E);
            Check(carry.Held == items[0], "UnlockedEIgnored", carry.Held?.name);
            await MouseClick();
            Check(Cursor.lockState == CursorLockMode.Locked && carry.Held == items[0], "RelockClickNoAction", true);
            await KeyPress(Key.R);
            Check(carry.Held == null && motor.CarrySpeedMultiplier == 1 && motor.transform.position.x < .05f,
                "RReleasesAndResets", motor.transform.position.ToString());
            await Aim(items[2]); await KeyPress(Key.E);
            Check(carry.Held == items[2] && !motor.CarryAllowsSprint, "HeavySprintDisabled", motor.CarrySpeedMultiplier);
            float sprintStartTime = Time.time;
            Vector3 sprintStartPosition = motor.transform.position;
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.W, Key.LeftShift));
            await Wait(.3f);
            float heavySpeed = Vector3.Distance(motor.transform.position, sprintStartPosition) / (Time.time - sprintStartTime);
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
            await Wait(.09f);
            Check(Mathf.Abs(heavySpeed - 2f) < .3f, "HeavyActualSprintSpeed", heavySpeed);
            cc.enabled = false; motor.transform.position = new Vector3(0, -4, 4); cc.enabled = true;
            await Wait(.2f);
            Check(carry.Held == null && motor.CarrySpeedMultiplier == 1 && motor.CarryAllowsSprint
                && motor.transform.position.y > 1, "PlayerFallReleases", motor.transform.position.ToString());

            motor.enabled = false;
            var response = new List<float>();
            foreach (var item in items)
            {
                await Aim(item);
                carry.TryPickup(item); carry.View.localRotation = Quaternion.identity;
                await Wait(1.2f);
                Vector3 start = item.Body.position;
                cc.enabled = false; motor.transform.position += Vector3.forward * .7f; cc.enabled = true;
                Physics.SyncTransforms(); await Wait(.2f);
                response.Add(item.Body.position.z - start.z);
                carry.Drop();
            }
            Check(response[0] > response[1] && response[1] > response[2], "WeightResponse", response.ToArray());
            var crate = items[1];
            await Aim(crate, new Vector3(7, 1.55f, 9.7f), new Vector3(7, 2.1f, 10.7f));
            crate.Body.interpolation = RigidbodyInterpolation.None;
            crate.Body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            Check(carry.TryPickup(crate), "WallFixturePickup", carry.Held?.name);
            motor.transform.rotation = Quaternion.identity; carry.View.localRotation = Quaternion.identity;
            Physics.SyncTransforms();
            float maxSpeed = 0, maxZ = 0;
            float end = Time.time + 2;
            while (Time.time < end)
            {
                maxSpeed = Mathf.Max(maxSpeed, crate.Body.linearVelocity.magnitude);
                maxZ = Mathf.Max(maxZ, crate.Body.position.z);
                await Wait(.04f);
            }
            Check(maxZ < 11 && maxSpeed < 4.5f && motor.transform.position.y < 1.7f,
                "CabinPushStable", new { maxZ, maxSpeed, player = motor.transform.position.ToString() });
            carry.Drop();
            Check(crate.Body.interpolation == RigidbodyInterpolation.None
                && crate.Body.collisionDetectionMode == CollisionDetectionMode.Discrete
                && crate.Body.useGravity && !crate.Body.isKinematic, "PhysicsSettingsRestored", true);
            crate.Body.interpolation = RigidbodyInterpolation.Interpolate;
            crate.Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            await Aim(items[0]); carry.TryPickup(items[0]);
            items[0].Body.position += Vector3.right * 5; await Wait(.1f);
            Check(carry.Held == null && motor.CarrySpeedMultiplier == 1, "DistanceSafeDrop", true);
            await Aim(items[0]); carry.TryPickup(items[0]); await Wait(.3f);
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "VerificationOnlyOccluder";
            wall.transform.position = (carry.View.position + items[0].Body.worldCenterOfMass) * .5f;
            wall.transform.localScale = new Vector3(.12f, 2, 2);
            Physics.SyncTransforms(); await Wait(.7f);
            Check(carry.Held == null, "ObstructionSafeDrop", true);
            UnityEngine.Object.Destroy(wall); wall = null;
            items[2].Body.position = new Vector3(20, -2, 20);
            items[2].Body.linearVelocity = Vector3.down * 4;
            items[2].Body.angularVelocity = Vector3.one * 3;
            await Wait(1.8f);
            Check(items[2].Body.position.y > 1 && items[2].Body.position.x < 2
                && items[2].Body.linearVelocity.magnitude < .2f && items[2].Body.angularVelocity.magnitude < .2f,
                "ScrapSeaRecovery", new { position = items[2].Body.position.ToString(), speed = items[2].Body.linearVelocity.magnitude });
        }
        finally
        {
            if (wall != null) UnityEngine.Object.Destroy(wall);
            carry.Drop(); foreach (var item in items) item.Recover();
            carry.View.localRotation = Quaternion.identity; motor.ReturnToSpawn(); motor.enabled = true;
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
        }
        return results;
    }
}
