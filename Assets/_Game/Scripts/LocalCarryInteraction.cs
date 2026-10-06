using UnityEngine;

namespace SalvageCrew
{
    [RequireComponent(typeof(PhysicsCarry), typeof(FirstPersonMotor), typeof(LocalPlayerInput))]
    public sealed class LocalCarryInteraction : MonoBehaviour
    {
        private PhysicsCarry carry;
        private LocalPlayerInput input;
        private void Awake() { carry = GetComponent<PhysicsCarry>(); input = GetComponent<LocalPlayerInput>(); }
        private void OnEnable() { GetComponent<FirstPersonMotor>().InputSampled += Process; }
        private void OnDisable() { GetComponent<FirstPersonMotor>().InputSampled -= Process; }
        private void Process(LocalPlayerInput.Sample sample)
        {
            if(sample.Scan && input.GameplayActive) WreckExpedition.Instance?.RequestScan(transform);
            var boat=OfflineBoatController.Instance;
            if(boat!=null&&boat.HandleInput(carry,sample))return;
            if(sample.Interact&&input.GameplayActive)carry.Toggle();
        }
    }
}
