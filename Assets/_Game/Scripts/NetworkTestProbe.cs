#if UNITY_EDITOR || DEVELOPMENT_BUILD
// Development-only, opt-in local verification bridge. Never enabled by normal game launch.
using System;
using System.IO;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace SalvageCrew
{
    public sealed class NetworkTestProbe : MonoBehaviour
    {
        [Serializable] public class Command
        {
            public int seq;
            public string action;
            public string address = "127.0.0.1";
            public int port = 7777;
            public float x, z, mass;
            public float duration = 4;
            public long atUtcMillis;
        }
        [Serializable] private class PlayerState
        { public ulong client; public bool owner, motor, input, camera, grounded, supported, driving; public Vector3 position, forward, boatLocal; public ulong held; public float speedMultiplier; public bool sprint; public string feedback; public int corrections; }
        [Serializable] private class ItemState
        { public ulong id, holder, owner; public float mass; public Vector3 position, velocity; public bool kinematic, recovery; }
        [Serializable] private class Snapshot
        {
            public int frame, sequence, errors;
            public float time;
            public string role, status, action, result, lastError;
            public long toggleUtcMillis;
            public bool panel, focused;
            public PlayerState[] players;
            public ItemState[] items;
            public BoatState boat;
        }
        [Serializable] private class BoatState
        { public ulong driver; public bool departed, kinematic; public Vector3 position, velocity, rotation; public Vector2 drive; }
        private string directory, result = "Ready", lastError = "";
        private Command command;
        private int sequence = -1, errors;
        private float nextRead, nextSnapshot, walkDeadline;
        private bool walking;
        private Vector3 previousWalkPosition;
        private long scheduledToggle, lastToggle;
        private float driveUntil;
        private Vector2 automatedDrive;
        private bool boatWalk;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "--salvage-probe");
            if (index >= 0 && index + 1 < args.Length)
                new GameObject("DevelopmentNetworkProbe").AddComponent<NetworkTestProbe>().Configure(args[index + 1]);
        }
        public void Configure(string path)
        { directory = path; Directory.CreateDirectory(path); Application.logMessageReceived += Log; }
        private void Log(string message, string trace, LogType type)
        { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) { errors++; lastError = message; } }
        private void Update()
        {
            if (directory == null || HarborSession.Instance == null) return;
            if (Time.unscaledTime >= nextRead)
            {
                nextRead = Time.unscaledTime + .1f;
                string path = Path.Combine(directory, "command.json");
                try
                {
                    if (File.Exists(path))
                    {
                        var incoming = JsonUtility.FromJson<Command>(File.ReadAllText(path));
                        if (incoming.seq != sequence) { sequence = incoming.seq; command = incoming; Execute(); }
                    }
                }
                catch (IOException) { /* Writer may still be replacing a test command. */ }
            }
            if (walking) Walk();
            var driver = HarborSession.Instance.LocalPlayer;
            if (driveUntil > Time.time && driver != null)
            { driver.Motor.Step(default, Time.deltaTime); driver.SubmitDriving(automatedDrive); }
            else if (driveUntil > 0) { driver?.SubmitDriving(Vector2.zero); driveUntil = 0; if (driver != null) driver.Motor.enabled = true; }
            if (scheduledToggle > 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= scheduledToggle)
            {
                scheduledToggle = 0; lastToggle = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                HarborSession.Instance.LocalPlayer?.RequestToggle(); result = "Scheduled RPC submitted";
            }
            if (Time.unscaledTime >= nextSnapshot) { nextSnapshot = Time.unscaledTime + .2f; SaveSnapshot(); }
        }
        private void Execute()
        {
            var session = HarborSession.Instance;
            var player = session.LocalPlayer;
            walking = false; result = "OK";
            scheduledToggle = 0;
            driveUntil = 0;
            if (player != null) player.Motor.enabled = true;
            switch (command.action)
            {
                case "host": session.ConfigureEndpoint(command.address, (ushort)command.port); session.StartHost(); break;
                case "client": session.ConfigureEndpoint(command.address, (ushort)command.port); session.StartClient(); break;
                case "disconnect": session.Disconnect(); break;
                case "panel": session.SetPanelOpen(true); break;
                case "resume": session.SetPanelOpen(false); break;
                case "walk":
                case "walkBoat":
                    if (player == null) { result = "No local player"; break; }
                    boatWalk = command.action == "walkBoat";
                    player.Motor.enabled = false; walking = true; walkDeadline = Time.time + 20;
                    previousWalkPosition = player.transform.position; break;
                case "look":
                    var item = FindObjectsByType<NetworkScrap>().FirstOrDefault(i => i.Item.Mass == command.mass);
                    if (player == null || item == null) { result = "No player/item"; break; }
                    Vector3 direction = item.Item.Body.worldCenterOfMass - player.Carry.View.position;
                    player.transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    float pitch = Mathf.Asin(-direction.normalized.y) * Mathf.Rad2Deg;
                    float oldPitch = Mathf.DeltaAngle(0, player.Carry.View.parent.localEulerAngles.x);
                    player.Motor.Step(new LocalPlayerInput.Sample { Look = new Vector2(0, (oldPitch - pitch) / .12f) }, .001f);
                    break;
                case "toggle": if (player != null) { lastToggle = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); player.RequestToggle(); } break;
                case "lookHelm":
                    if (player != null && NetworkBoat.Instance != null) Aim(player, NetworkBoat.Instance.Helm.position); break;
                case "drive":
                    if (player == null || !player.Driving) { result = "Not driving"; break; }
                    player.Motor.enabled = false; automatedDrive = new Vector2(command.x, command.z); driveUntil = Time.time + command.duration; break;
                case "jump":
                    if (player != null) player.Motor.Step(new LocalPlayerInput.Sample { Jump = true }, .02f); break;
                case "toggleAt": scheduledToggle = command.atUtcMillis; result = "Scheduled"; break;
                case "reset": if (player != null) player.Motor.ReturnToSpawn(); break;
                case "status": break;
                default: result = "Unknown action"; break;
            }
        }
        private void Walk()
        {
            var player = HarborSession.Instance.LocalPlayer;
            if (player == null) { walking = false; result = "Player disconnected"; return; }
            if (Vector3.Distance(previousWalkPosition, player.transform.position) > 3)
            { walking = false; player.Motor.enabled = true; result = "Rescued or corrected; inspect spawn state"; return; }
            Vector3 target = boatWalk && NetworkBoat.Instance != null ? NetworkBoat.Instance.transform.TransformPoint(new Vector3(command.x, 1.5f, command.z))
                : new Vector3(command.x, player.transform.position.y, command.z);
            Vector3 delta = Vector3.ProjectOnPlane(target - player.transform.position, Vector3.up);
            if (delta.magnitude < .12f || Time.time > walkDeadline)
            {
                walking = false; result = delta.magnitude < .12f ? "Reached" : "Walk timeout";
                player.Motor.enabled = true; return;
            }
            player.transform.rotation = Quaternion.LookRotation(delta);
            float currentPitch = Mathf.DeltaAngle(0, player.Carry.View.parent.localEulerAngles.x);
            player.Motor.Step(new LocalPlayerInput.Sample { Move = Vector2.up, Look = new Vector2(0, currentPitch / .12f) }, Mathf.Min(Time.deltaTime, .033f));
            previousWalkPosition = player.transform.position;
        }
        private static void Aim(NetworkCrewPlayer player, Vector3 point)
        {
            Vector3 direction = point - player.Carry.View.position;
            player.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(direction, Vector3.up));
            float pitch = Mathf.Asin(-direction.normalized.y) * Mathf.Rad2Deg;
            float oldPitch = Mathf.DeltaAngle(0, player.Carry.View.parent.localEulerAngles.x);
            player.Motor.Step(new LocalPlayerInput.Sample { Look = new Vector2(0, (oldPitch - pitch) / .12f) }, .001f);
        }
        private void SaveSnapshot()
        {
            var session = HarborSession.Instance;
            var state = new Snapshot
            {
                frame = Time.frameCount, time = Time.time, sequence = sequence, errors = errors, lastError = lastError,
                role = session.Manager.IsHost ? "HOST" : session.Manager.IsConnectedClient ? "CLIENT" : "SOLO",
                status = session.Status, panel = session.PanelOpen, focused = Application.isFocused,
                action = walking ? "walking" : command?.action, result = result,
                toggleUtcMillis = lastToggle,
                players = FindObjectsByType<NetworkCrewPlayer>().Select(p => new PlayerState
                {
                    client = p.OwnerClientId, owner = p.IsOwner, motor = p.Motor.enabled, input = p.Input.enabled,
                    camera = p.GetComponentInChildren<Camera>().enabled, position = p.transform.position,
                    forward = p.Carry.View.forward, held = p.HeldId.Value, speedMultiplier = p.Motor.CarrySpeedMultiplier,
                    sprint = p.Motor.CarryAllowsSprint, feedback = p.Feedback, grounded = p.Motor.Grounded,
                    supported = p.Motor.Passenger != null && p.Motor.Passenger.Support != null, driving = p.Driving,
                    boatLocal = NetworkBoat.Instance != null ? NetworkBoat.Instance.transform.InverseTransformPoint(p.transform.position) : Vector3.zero,
                    corrections = p.Corrections
                }).ToArray(),
                items = FindObjectsByType<NetworkScrap>().Select(i => new ItemState
                {
                    id = i.NetworkObjectId, mass = i.Item.Mass, holder = i.Holder.Value, owner = i.OwnerClientId,
                    position = i.Item.Body.position, velocity = i.Item.Body.linearVelocity, kinematic = i.Item.Body.isKinematic,
                    recovery = i.Item.enabled
                }).ToArray()
            };
            var boat = NetworkBoat.Instance;
            if (boat != null) state.boat = new BoatState { driver = boat.Driver.Value, departed = boat.Departed.Value,
                position = boat.transform.position, rotation = boat.transform.eulerAngles, velocity = boat.Velocity, drive = boat.Drive, kinematic = boat.Body.isKinematic };
            File.WriteAllText(Path.Combine(directory, "state.json"), JsonUtility.ToJson(state, true));
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
    }
}
#endif
