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
        
        public bool CanJump (in SurfaceInfo groundInfo, in SurfaceInfo wallInfo)
        {
            if (jumpCount >= maxJumps)
                return false;

            if (airJump.Enabled)
                return true;

            if (wallInfo.InContact && wallJump.Enabled)
                return true;
            
            return groundInfo.InContact;
        }
        
        public Vector3 GetJumpVelocity (
            float jumpHeight, float gravity, in SurfaceInfo groundInfo, in SurfaceInfo wallInfo)
        {
            jumpCount++;
            
            if (groundInfo.InContact)
            {
                return new Vector3 (
                    0, 
                    Kinetics.GetVelocityToReachApex (jumpHeight, gravity), 
                    0);
            }

            if (wallJump.Enabled && wallInfo.InContact)
            {
                var yVelocity = Kinetics.GetVelocityToReachApex (
                    jumpHeight * wallJump.Strength, gravity);
                
                yVelocity = math.cos (yVelocity * wallJump.Verticality);
                var xVelocity = math.sin (yVelocity);
                
                return new Vector3 (xVelocity, yVelocity, 0f);
            }

            if (airJump.Enabled)
            {
                return new Vector3 (
                    0,
                    Kinetics.GetVelocityToReachApex (jumpHeight * airJump.Strength, gravity),
                    0);
            }
            
            return new Vector3 (
                0,
                0,
                0);
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
        
        [Range (0, 1)]
        public float Strength;
        
        [Range (0, 1)]
        public float Verticality;
    }

    [System.Serializable]
    public struct AirJump
    {
        public bool Enabled;
        
        [Range (0, 1)]
        public float Strength;
        
        [Range (0, 1)]
        public float Control;
    }

}