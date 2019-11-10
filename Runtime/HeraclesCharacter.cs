using System;
using Unity.Mathematics;
using UnityEngine;

namespace Hirame.Heracles
{
    public enum OrientationMode { None, World, Gravity, Surface }
    
    public class HeraclesCharacter : MonoBehaviour
    {
        [Header ("Settings")]
        public float Speed = 5f;
        public float Acceleration = 20f;
        public float JumpHeight;
        public float StepHeight = 0.2f;

        public bool GroundOnFullContact = true;
        public OrientationMode AlignTo = OrientationMode.Gravity;

        
        [Range (0, 1)]
        public float AirControl = 0.7f;
        public float GravityScale = 1f;

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
        private SurfaceInfo previousLeftWallInfo;
        private SurfaceInfo previousRightWallInfo;

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
            var directionalInput = GetDirectionalInput ();

            UpdateSurfaceInfo ();
            UpdateGroundStatus ();
            
            Orientate ();

            ResolveJump (in directionalInput, ref velocity);

            Move (directionalInput.x, in velocity);
        }

        private static Vector2 GetDirectionalInput ()
        {
            return new Vector2(
                Input.GetAxis ("Horizontal"),
                Input.GetAxis ("Vertical")
                );
        }

        private void UpdateSurfaceInfo ()
        {
            previousGroundInfo = groundCheck.SurfaceInfo;
            previousLeftWallInfo = wallCheckLeft.SurfaceInfo;
            previousRightWallInfo = wallCheckRight.SurfaceInfo;

            groundCheck.UpdateGroundInfo (attachedRigidbody.position, StepHeight);
            wallCheckLeft.UpdateGroundInfo (attachedRigidbody.position, 0.1f);
            wallCheckRight.UpdateGroundInfo (attachedRigidbody.position, 0.1f);
            
            //Debug.Log ($"{wallCheckLeft.SurfaceInfo.InContact} | {wallCheckRight.SurfaceInfo.InContact}");
        }

        private void UpdateGroundStatus ()
        {
            var wasOnGround = onGround;
            onGround =groundCheck.SurfaceInfo.InContact;
            
            if (GroundOnFullContact)
                onGround &= hasMadeFullSurfaceContact;
            
            if (previousLeftWallInfo.InContact || previousRightWallInfo.InContact)
            {
                //Debug.Log ($"{wallInfoLeft.ContactCollider} | {wallInfoRight.ContactCollider}");
            }

            //Debug.Log ($"{onGround} | {previousGroundInfo.InContact}");

            if (onGround)
            {
                if (!wasOnGround)
                    jump.OnReturnedToGround ();
            }
            else if (wasOnGround)
            {
                hasMadeFullSurfaceContact = false;
                jump.OnDetachedFromGround ();
            }
        }
        
        private void ResolveJump (in Vector2 directionalInput, ref Vector3 velocity)
        {
            ref readonly var groundInfo = ref groundCheck.SurfaceInfo;
            ref readonly var leftWallInfo = ref wallCheckLeft.SurfaceInfo;
            ref readonly var rightWalInfo = ref wallCheckRight.SurfaceInfo;
            
            if (jumpFlag && jump.CanJump (in groundInfo, in leftWallInfo, in rightWalInfo))
            {
                var gravity = Physics.gravity.y * GravityScale;
                
                var jumpVelocity = jump.GetJumpVelocity (
                    JumpHeight, gravity, in directionalInput,
                    in groundInfo, in leftWallInfo, in rightWalInfo);

                velocity.x += jumpVelocity.x;
                velocity.y = math.clamp (velocity.y + jumpVelocity.y, jumpVelocity.y, jumpVelocity.y * 1.2f);
                //onGround = false;
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

        private void Move (float input, in Vector3 currentVelocity)
        {
            ref readonly var groundInfo = ref groundCheck.SurfaceInfo;
            
            var deltaTime = GetDeltaTime (groundInfo.InContact);
            var acceleration = input * Acceleration;
            
            var newVelocity = mover.GetVelocity (
                acceleration, Speed, in currentVelocity, in groundInfo, deltaTime);
            
            attachedRigidbody.velocity = newVelocity;
            attachedRigidbody.drag = math.abs (input) < 0.1f ? 1 : 0;
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
            if (!GroundOnFullContact)
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
