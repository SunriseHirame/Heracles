using System.Runtime.CompilerServices;
using UnityEngine;

namespace Hirame.Heracles
{
    [System.Flags]
    public enum SurfaceFlags { Below = 1, Above = 2, Left = 4, Right = 8}
    
    [System.Serializable]
    public struct SurfaceInfo
    {
        public bool InContact;

        public Collider ContactCollider;
        public Vector3 Normal;
        
        public bool IsOnLayer (LayerMask layer)
        {
            return (1 << ContactCollider.gameObject.layer) == layer;
        }
        
        private static readonly SurfaceInfo none = new SurfaceInfo ();
        public static ref readonly SurfaceInfo Default => ref none;
    }

    public static class SurfaceInfoExtensions
    {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static PhysicMaterial GetPhysicsMaterial (this in SurfaceInfo surfaceInfo)
        {
            return surfaceInfo.ContactCollider ? surfaceInfo.ContactCollider.sharedMaterial : null;
        }
    }

}
