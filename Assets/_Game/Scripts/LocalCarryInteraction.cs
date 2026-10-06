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
        { if (sample.Interact && input.GameplayActive) carry.Toggle(); }
    }
}
