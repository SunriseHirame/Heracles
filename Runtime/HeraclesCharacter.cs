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
        [SerializeField] private KineticMover mover;
        [SerializeField] private JumpController jump;

        [Header ("Surface Info")]
        [SerializeField] private SurfaceDetector groundCheck;
        [SerializeField] private SurfaceDetector wallCheckLeft;
        [SerializeField] private SurfaceDetector wallCheckRight;
        

        [SerializeField] private Rigidbody attachedRigidbody;


        private bool jumpFlag;
        
        private SurfaceInfo previousGroundInfo;
        private SurfaceInfo wallInfoLeft;
        private SurfaceInfo wallInfoRight;

        private bool onGround;
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
            var input = Input.GetAxis ("Horizontal");

            PushSurfaceInfo ();
            
            ref readonly var groundInfo = ref groundCheck.UpdateGroundInfo (attachedRigidbody.position, StepHeight);
            wallInfoLeft = wallCheckLeft.UpdateGroundInfo (attachedRigidbody.position, 0.1f);
            wallInfoRight = wallCheckRight.UpdateGroundInfo (attachedRigidbody.position, 0.1f);
            
            UpdateGroundStatus (in groundInfo);
            
            SurfaceMaterial = groundInfo.GetPhysicsMaterial ();

            Orientate ();

            ResolveJump (ref velocity);

            var deltaTime = GetDeltaTime (previousGroundInfo.InContact);
            var acceleration = input * Acceleration;
            
            var newVelocity = mover.Move (acceleration, Speed, velocity, in previousGroundInfo, deltaTime);
            
            CurretVelocity = newVelocity.x;
            
            attachedRigidbody.velocity = newVelocity;
            attachedRigidbody.drag = math.abs (input) < 0.1f ? 1 : 0;
        }

        private void PushSurfaceInfo ()
        {
            previousGroundInfo = groundCheck.SurfaceInfo;
            wallInfoLeft = wallCheckLeft.SurfaceInfo;
            wallInfoRight = wallCheckRight.SurfaceInfo;
        }

        private void UpdateGroundStatus (in SurfaceInfo groundInfo)
        {
            onGround = groundInfo.InContact;

            if (RestoreGroundOnGroundCollision)
                onGround &= hasMadeFullSurfaceContact;
            
            if (wallInfoLeft.InContact || wallInfoRight.InContact)
            {
                //Debug.Log ($"{wallInfoLeft.ContactCollider} | {wallInfoRight.ContactCollider}");
            }

            if (onGround)
            {
                if (!previousGroundInfo.InContact)
                    jump.OnReturnedToGround ();
            }
            else
            {
                if (previousGroundInfo.InContact)
                {
                    hasMadeFullSurfaceContact = false;
                    jump.OnDetachedFromGround ();
                }
            }
        }
        
        private void ResolveJump (ref Vector3 velocity)
        {
            if (jumpFlag && jump.CanJump (in previousGroundInfo, in wallInfoLeft))
            {
                var jumpVelocity = jump.GetJumpVelocity (JumpHeight, in previousGroundInfo, in wallInfoLeft);
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
