using Hirame.Pantheon;
using Unity.Mathematics;
using UnityEngine;

namespace Hirame.Heracles
{
    [System.Serializable]
    public class JumpController
    {
        [SerializeField] private float graceTime = 0.1f;

        [SerializeField] private int maxJumps = 2;

        [SerializeField] private AirJump airJump;
        [SerializeField] private WallJump wallJump;

        private int jumpCount;

        public bool CanJump (in SurfaceInfo groundInfo, in SurfaceInfo leftWallInfo, in SurfaceInfo rightWallInfo)
        {
            if (jumpCount >= maxJumps)
                return false;

            if (airJump.Enabled)
                return true;

            if (wallJump.Enabled && leftWallInfo.InContact || rightWallInfo.InContact)
                return true;

            return groundInfo.InContact;
        }

        public Vector3 GetJumpVelocity (
            float jumpHeight, float gravity, in Vector2 directionalInput,
            in SurfaceInfo groundInfo, in SurfaceInfo leftWallInfo, in SurfaceInfo rightWallInfo)
        {
            jumpCount++;

            if (groundInfo.InContact)
            {
                return new Vector3 (
                    0,
                    Kinetics.GetVelocityToReachApex (jumpHeight, gravity),
                    0);
            }
            
            if (wallJump.Enabled)
            {
                if (leftWallInfo.InContact)
                {
                    var yVelocity = Kinetics.GetVelocityToReachApex (
                        jumpHeight * wallJump.Strength, gravity);

                    return GetWallJumpVelocity (yVelocity, in directionalInput, 1);
                }

                if (rightWallInfo.InContact)
                {
                    var yVelocity = Kinetics.GetVelocityToReachApex (
                        jumpHeight * wallJump.Strength, gravity);

                    return GetWallJumpVelocity (yVelocity, in directionalInput, -1);
                }
            }

            if (airJump.Enabled)
            {
                return GetAirJumpVelocity (
                    Kinetics.GetVelocityToReachApex (jumpHeight * airJump.Strength, gravity),
                    in directionalInput);
            }

            return new Vector3 (
                0,
                0,
                0);
        }

        private Vector3 GetWallJumpVelocity (float baseVelocity, in Vector2 directionalInput, float sign)
        {
            ref readonly var verticalControl = ref wallJump.Control;
            var remappedInput = Mathf.Abs (directionalInput.x);
            
            var control = verticalControl.Max - verticalControl.Remap (remappedInput);
            var rotate = Quaternion.Euler (0, 0, 90 * control);
            
            var jumpVector = rotate * Vector2.up;
            jumpVector *= baseVelocity;
            
            Debug.Log (jumpVector);
            
            return new Vector3 (jumpVector.x * -sign, jumpVector.y, 0f);
        }

        private Vector3 GetAirJumpVelocity (float baseVelocity, in Vector2 directionalInput)
        {
            var xScaled = directionalInput.x * airJump.Control;

            var jumpVector = new Vector3 (
                baseVelocity * xScaled,
                baseVelocity,
                0f
                );

            return jumpVector;
        }
        
        public void OnDetachedFromGround ()
        {
            Debug.Log ("Detached from ground");
            if (jumpCount == 0)
                jumpCount = 1;
        }

        public void OnReturnedToGround ()
        {
            Debug.Log ("Returned to ground");
            jumpCount = 0;
        }
    }

    [System.Serializable]
    public struct WallJump
    {
        public bool Enabled;

        [Range (0, 1)] public float Strength;
        [MinMax (0, 1)] public FloatMinMax Control;
    }

    [System.Serializable]
    public struct AirJump
    {
        public bool Enabled;

        [Range (0, 1)] public float Strength;
        [Range (0, 1)] public float Control;
    }
}