using System.Runtime.CompilerServices;
using UnityEngine;

namespace Hirame.Heracles
{
    public enum AxisMask
    {
        X = 1, Y = 2, Z = 3,
        XY = 4, XZ = 5, YZ = 6,
        XYZ = 7
    }

    public static class AxisMaskExtensions
    {
        private static readonly Vector3Int[] Masks =
        {
            new Vector3Int (0, 0, 0),
            
            new Vector3Int (1, 0, 0), // X
            new Vector3Int (0, 1, 0), // Y
            new Vector3Int (0, 0, 1), // Z
            new Vector3Int (1, 1, 0), // XY
            new Vector3Int (1, 0, 1), // XZ
            new Vector3Int (0, 1, 1), // YZ
            new Vector3Int (1, 1, 1), // XYZ
        };
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static ref readonly Vector3Int GetMask (this AxisMask mask)
        {
            return ref Masks[(int) mask];
        }
    }
    
    [System.Serializable]
    public class KineticMover
    {
        [SerializeField] private AxisMask axisMask = AxisMask.XYZ;
        
        [Range (0, 90)]
        [SerializeField] private float maxSurfaceAngle = 60f;
        
        private float drag = 1f;
            
        public Vector3 GetVelocity (
            float acceleration, float targetSpeed, in Vector3 current, in SurfaceInfo surfaceInfo, float deltaTime)
        {
            UpdateDrag (acceleration, targetSpeed);
            var velocityMagnitude = current.x;
            var xVelocity = Kinetics.AccelerateTowards (velocityMagnitude, acceleration, drag, deltaTime);
            
            var surfaceMaterial = surfaceInfo.GetPhysicsMaterial ();
            
            if (surfaceMaterial)
                xVelocity -= Kinetics.ApplySlipperiness (xVelocity - velocityMagnitude, surfaceMaterial);

            var mask = axisMask.GetMask ();
            var newVelocity = new Vector3 (xVelocity, current.y, current.z);
            newVelocity.Scale (mask);
            
            return newVelocity;
        }
        
        private void UpdateDrag (float acceleration, float targetSpeed)
        {
            drag = Kinetics.CalculateOptimalDrag (acceleration, targetSpeed);
        }
        
    }

}
