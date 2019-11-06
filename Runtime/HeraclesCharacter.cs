using System;
using Unity.Mathematics;
using UnityEngine;

namespace Hirame.Heracles
{
    public enum OrientationMode { None, World, Gravity, Surface }
    
    public class HeraclesCharacter : MonoBehaviour
    {
        [Header ("Debug")]
        public float CurretVelocity;
        public PhysicMaterial SurfaceMaterial;

        [Header ("Settings")]
        public float Speed = 5f;
        public float Acceleration = 20f;
        public float JumpHeight;
        public float StepHeight = 0.2f;

        public bool RestoreGroundOnGroundCollision = true;
        public OrientationMode AlignTo = OrientationMode.Gravity;

        
        [Range (0, 1)]
        public float AirControl = 0.7f;
        
        [Header ("Motion")]
        [SerializeField] private SurfaceDetector groundCheck;
        [SerializeField] private SurfaceDetector wallCheckLeft;
        [SerializeField] private SurfaceDetector wallCheckRight;
        
        [SerializeField] private KineticMover mover;
        [SerializeField] private JumpController jump;
        
        [SerializeField] private Rigidbody attachedRigidbody;

        private float drag = 1f;

        private bool jumpFlag;
        
        private SurfaceInfo groundInfo;
        private SurfaceInfo wallInfoLeft;
        private SurfaceInfo wallInfoRight;

        private bool hasMadeFullSurfaceContact;
        
        private void Awake ()
        {
            attachedRigidbody = GetComponent<Rigidbody> ();
        }

        private void Update ()
        {
            if (!jumpFlag)
                jumpFlag = Input.GetKeyDown (KeyCode.Space);
        }

        private float control = 1;
        
        public void FixedUpdate ()
        {
            var velocity = attachedRigidbody.velocity;

            var wasOnGround = groundInfo.InContact;
            
            groundInfo = groundCheck.GetSurfaceInfo (attachedRigidbody.position, StepHeight);
            
            if (RestoreGroundOnGroundCollision)
                groundInfo.InContact &= hasMadeFullSurfaceContact;
            
            SurfaceMaterial = groundInfo.GetPhysicsMaterial ();

            wallInfoLeft = wallCheckLeft.GetSurfaceInfo (attachedRigidbody.position, 0.1f);
            wallInfoRight = wallCheckRight.GetSurfaceInfo (attachedRigidbody.position, 0.1f);

            if (wallInfoLeft.InContact || wallInfoRight.InContact)
            {
                Debug.Log ($"{wallInfoLeft.ContactCollider} | {wallInfoRight.ContactCollider}");
            }

            if (groundInfo.InContact)
            {
                if (!wasOnGround)
                    jump.OnReturnedToGround ();
            }
            else
            {
                if (wasOnGround)
                {
                    hasMadeFullSurfaceContact = false;
                    jump.OnDetachedFromGround ();
                }
            }
            
            var input = Input.GetAxis ("Horizontal");
            
            Orientate ();

            ResolveJump (ref velocity);

            var deltaTime = GetDeltaTime (groundInfo.InContact);

            UpdateDrag ();

            var acceleration = input * Acceleration;


            var velocityMagnitude = velocity.x;
            var newVelocity = Kinetics.AccelerateTowards (velocityMagnitude, acceleration, drag, deltaTime);

            if (SurfaceMaterial)
                newVelocity -= (newVelocity - velocityMagnitude) * (1 - SurfaceMaterial.dynamicFriction);

            CurretVelocity = velocityMagnitude;
            attachedRigidbody.velocity = new Vector3(newVelocity, velocity.y, velocity.z);

            attachedRigidbody.drag = math.abs (input) < 0.1f ? 1 : 0;
        }

        private void ResolveJump (ref Vector3 velocity)
        {
            if (jumpFlag && jump.CanJump (in groundInfo, in wallInfoLeft))
            {
                var jumpVelocity = jump.GetJumpVelocity (JumpHeight, in groundInfo, in wallInfoLeft);
                velocity.y = jumpVelocity.y;
            }

            jumpFlag = false;
        }

        private float GetDeltaTime (bool onGround)
        {
            var deltaTime = Time.fixedDeltaTime;
            
            if (onGround)
            {
                control = math.clamp (control + deltaTime, 0, 1);
            }
            else
            {
                control = 0;
            }
            
            return math.lerp (deltaTime * AirControl * AirControl, deltaTime, control);
        }

        private void UpdateDrag ()
        {
            drag = Kinetics.CalculateOptimalDrag (Acceleration, Speed);
        }

        private void Orientate ()
        {
            switch (AlignTo)
            {
                case OrientationMode.World:
                    attachedRigidbody.MoveRotation (Quaternion.identity);
                    break;
                case OrientationMode.Gravity:
                    attachedRigidbody.MoveRotation (Quaternion.Euler (-Physics.gravity.normalized));
                    break;
                case OrientationMode.Surface:
                    break;
                default:
                    break;
            }
        }

        private void OnCollisionEnter (Collision collision)
        {
            if (!RestoreGroundOnGroundCollision)
                return;

            var contacts = collision.contactCount;
            for (var i = 0; i < contacts; i++)
            {
                var contact = collision.GetContact (i);
                if (contact.point.y > attachedRigidbody.position.y + StepHeight)
                    continue;

                hasMadeFullSurfaceContact = true;
                break;
            }
        }

        private void OnDrawGizmos ()
        {
            groundCheck.OnDrawGizmos (attachedRigidbody.position, StepHeight);
            wallCheckLeft.OnDrawGizmos (attachedRigidbody.position, 0.05f);
            wallCheckRight.OnDrawGizmos (attachedRigidbody.position, 0.05f);
        }
    }

}
