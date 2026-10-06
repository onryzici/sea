// Live Play verification: real FixedUpdate, Rigidbody and scene colliders.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalvageCrew;
using UnityEngine;

public static class CarryPlayChecks
{
    static FirstPersonMotor motor;
    static PhysicsCarry carry;
    static CharacterController cc;
    static Dictionary<string, object> results;
    static void Check(bool ok, string name, object detail)
    {
        results[name] = new { passed = ok, detail };
        if (!ok) throw new Exception(name + ": " + detail);
    }
    static async Task Wait(float seconds)
    {
        float deadline = Time.time + seconds;
        while (Time.time < deadline) await Task.Delay(20);
    }
    static void Place(Vector3 position, float yaw)
    {
        motor.ReturnToSpawn();
        cc.enabled = false;
        motor.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
        cc.enabled = true;
        carry.View.localRotation = Quaternion.identity;
        Physics.SyncTransforms();
    }
    static async Task WalkTo(Vector3 target)
    {
        float deadline = Time.time + 12;
        while (Vector2.Distance(new Vector2(motor.transform.position.x, motor.transform.position.z),
            new Vector2(target.x, target.z)) > .12f && Time.time < deadline)
        {
            Vector3 delta = target - motor.transform.position; delta.y = 0;
            motor.transform.rotation = Quaternion.LookRotation(delta);
            float dt = Mathf.Min(.03f, Time.deltaTime);
            motor.Step(new LocalPlayerInput.Sample { Move = Vector2.up }, dt);
            await Task.Delay(10);
        }
        Check(Time.time < deadline, "Walk_" + (carry.Held != null ? carry.Held.Mass : 0) + "_" + target, motor.transform.position.ToString());
    }
    public static async Task<Dictionary<string, object>> Run()
    {
        if (!Application.isPlaying) throw new Exception("Play required");
        Application.runInBackground = true;
        motor = UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        carry = motor.GetComponent<PhysicsCarry>(); cc = motor.GetComponent<CharacterController>();
        results = new Dictionary<string, object>();
        motor.enabled = false;
        try
        {
            foreach(var existing in UnityEngine.Object.FindObjectsByType<ScrapItem>())existing.Recover();
            var boat=UnityEngine.Object.FindAnyObjectByType<CalmWaterBuoyancy>().transform;
            foreach (var item in UnityEngine.Object.FindObjectsByType<ScrapItem>().OrderBy(i => i.Mass))
            {
                item.Recover(); await Wait(.25f);
                Place(new Vector3(.25f, 1.25f, item.transform.position.z), 90);
                carry.View.LookAt(item.Body.worldCenterOfMass);
                await Wait(.1f);
                Check(carry.TryPickup(item), "Pickup_" + item.Mass,
                    new { item.DisplayName, target = carry.FindTarget()?.name, position = item.Body.position.ToString(), held = item.IsHeld });
                Check(Mathf.Approximately(motor.CarrySpeedMultiplier, item.MovementMultiplier)
                    && motor.CarryAllowsSprint == item.AllowSprint, "MovementModifier_" + item.Mass, motor.CarrySpeedMultiplier);
                Check(Physics.GetIgnoreCollision(cc, item.GetComponentInChildren<Collider>()), "PlayerCollisionIgnored_" + item.Mass, true);
                carry.View.localRotation = Quaternion.identity;
                await Wait(.7f);
                await WalkTo(new Vector3(.25f, 0, 7.5f));
                await WalkTo(new Vector3(6.25f, 0, 7.5f));
                await Wait(.8f);
                Check(carry.Held == item && item.Body.position.x > 5.4f && item.Body.position.y > 1.5f,
                    "CarriedToDeck_" + item.Mass, item.Body.position.ToString());
                carry.Drop();
                Check(motor.CarrySpeedMultiplier == 1 && motor.CarryAllowsSprint && !item.IsHeld
                    && !Physics.GetIgnoreCollision(cc, item.GetComponentInChildren<Collider>()), "DropRestoration_" + item.Mass, true);
                await Wait(2);
                // Preserve the deck-relative threshold now that the hull has draft and heave.
                var onBoat=boat.InverseTransformPoint(item.Body.position);
                Check(Mathf.Abs(onBoat.x)<2 && Mathf.Abs(onBoat.z)<5 && onBoat.y > 1.6f
                    && item.Body.linearVelocity.magnitude < .15f, "SettledOnDeck_" + item.Mass,
                    new { position = item.Body.position.ToString(), speed = item.Body.linearVelocity.magnitude });
                item.Recover();
            }
        }
        finally { carry.Drop(); carry.View.localRotation = Quaternion.identity; motor.ReturnToSpawn(); motor.enabled = true; }
        return results;
    }

    // Leaves one legitimately carried box on deck for the requested Game view capture.
    // Stop Play afterwards; do not save this transient verification state to the scene.
    public static async Task<object> PrepareScreenshot()
    {
        motor = UnityEngine.Object.FindAnyObjectByType<FirstPersonMotor>();
        carry = motor.GetComponent<PhysicsCarry>(); cc = motor.GetComponent<CharacterController>();
        results = new Dictionary<string, object>(); motor.enabled = false;
        var item = UnityEngine.Object.FindObjectsByType<ScrapItem>().First(i => i.Mass == 3);
        item.Recover(); await Wait(.25f);
        Place(new Vector3(.25f, 1.25f, 2), 90);
        carry.View.LookAt(item.Body.worldCenterOfMass);
        if (!carry.TryPickup(item)) throw new Exception("Screenshot pickup failed");
        carry.View.localRotation = Quaternion.identity;
        await Wait(.6f);
        await WalkTo(new Vector3(.25f, 0, 7.5f));
        await WalkTo(new Vector3(6.25f, 0, 7.5f));
        motor.transform.rotation = Quaternion.identity;
        await Wait(1);
        return new { held = carry.Held?.DisplayName, player = motor.transform.position.ToString(), item = item.Body.position.ToString(), frame = Time.frameCount };
    }
}
