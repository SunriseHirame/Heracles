using System;
using Unity.Mathematics;
using UnityEngine;

namespace Hirame.Heracles
{
    public enum OrientationMode { World, Gravity, Surface }
    
    public class HeraclesCharacter : MonoBehaviour
    {
        [Header ("Debug")]
        public float CurretVelocity;
        public PhysicMaterial SurfaceMaterial;

        [Header ("Settings")]
        public float Speed = 5f;
        public float Acceleration = 20f;
        public float JumpHeight;

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
        
        private Rigidbody attachedRigidbody;

        private float drag = 1f;

        private bool jumpFlag;
        
        private SurfaceInfo groundInfo;
        private SurfaceInfo wallInfo;

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
            
            groundInfo = groundCheck.Scan (attachedRigidbody.position);
            groundInfo.InContact &= hasMadeFullSurfaceContact;
            SurfaceMaterial = groundInfo.GetPhysicsMaterial ();

            if (groundInfo.InContact)
            {
                if (!wasOnGround)
                    jump.OnReturnedToGround ();
            }
            else
            {
                hasMadeFullSurfaceContact = false;
                
                if (wasOnGround)
                    jump.OnDetachedFromGround ();
            }
            
            var input = Input.GetAxis ("Horizontal");

            Orientate ();

            ResolveJump (ref velocity);

            var deltaTime = GetDeltaTime (groundInfo.InContact);

            UpdateDrag ();

            var acceleration = input * Acceleration;
            
            var velocityMagnitude = velocity.x;
            velocityMagnitude = Kinetics.AccelerateTowards (velocityMagnitude, acceleration, drag, deltaTime);

            CurretVelocity = velocityMagnitude;
            attachedRigidbody.velocity = new Vector3(velocityMagnitude, velocity.y, velocity.z);
        }

        private void ResolveJump (ref Vector3 velocity)
        {
            if (jumpFlag && jump.CanJump (in groundInfo, in wallInfo))
            {
                var jumpVelocity = jump.GetJumpVelocity (JumpHeight, in groundInfo, in wallInfo);
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
                    throw new ArgumentOutOfRangeException ();
            }
        }

        private void OnCollisionEnter (Collision collision)
        {
            if (!RestoreGroundOnGroundCollision || groundInfo.InContact)
                return;

            var contacts = collision.contactCount;
            for (var i = 0; i < contacts; i++)
            {
                var contact = collision.GetContact (i);
                if (contact.point.y > attachedRigidbody.position.y + 0.1f)
                    continue;

                hasMadeFullSurfaceContact = true;
                break;
            }
        }
    }

}
