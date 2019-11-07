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

        // readonly
        public PhysicMaterial GetPhysicsMaterial ()
        {
            return ContactCollider ? ContactCollider.sharedMaterial : null;
        }
        
        public bool IsOnLayer (LayerMask layer)
        {
            return (1 << ContactCollider.gameObject.layer) == layer;
        }
        
        private static readonly SurfaceInfo none = new SurfaceInfo ();
        public static ref readonly SurfaceInfo Default => ref none;
    }

}
