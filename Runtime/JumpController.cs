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
            if (groundInfo.InContact)
                return true;

            if (jumpCount < maxJumps)
                return airJump.Enabled;
            
            return false;
        }
        
        public Vector3 GetJumpVelocity (float jumpHeight, in SurfaceInfo groundInfo, in SurfaceInfo wallInfo)
        {
            jumpCount++;

            var multiplier = 1f;
            if (!groundInfo.InContact)
                multiplier = airJump.Strength;
            
            return new Vector3(
                0, 
                Kinetics.GetVelocityToReachApex (jumpHeight * multiplier, Physics.gravity.y), 
                0);   
        }

        public void OnDetachedFromGround ()
        {
            if (jumpCount == 0)
                jumpCount = 1;
        }

        public void OnReturnedToGround ()
        {
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