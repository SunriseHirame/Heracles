using System.Runtime.CompilerServices;
using Hirame.Pantheon;
using UnityEngine;

namespace Hirame.Heracles
{
    [System.Serializable]
    public class SurfaceDetector
    {
        [SerializeField] private LayerMask validLayers;
        [SerializeField] private Vector3 scanSize = new Vector3 (0.4f, 0.02f, 0.04f);
        [SerializeField] private Vector3 offset = new Vector3 (0, 0.03f, 0);
        [SerializeField] private AxialDirection direction = AxialDirection.Down;
        [SerializeField] private float maxDistance = 0.2f;

        private SurfaceInfo surfaceInfo;

        public ref readonly SurfaceInfo GetSurfaceInfo (in Vector3 origin, float skinWidth)
        {
            var startPosition = GetStartPosition (origin, skinWidth);
            var checkDistance = GetCheckDistance (skinWidth);
            var dir = direction.GetDirectionVector ();
            
            var hitSomething = Physics.BoxCast (startPosition, scanSize, dir, out var hitInfo,
                Quaternion.identity, checkDistance, validLayers);

            surfaceInfo = hitSomething
                ? new SurfaceInfo
                {
                    InContact = true,
                    Normal = hitInfo.normal,
                    ContactCollider = hitInfo.collider
                }
                : SurfaceInfo.Default;
            
            return ref surfaceInfo;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private Vector3 GetStartPosition (Vector3 origin, float skinWidth)
        {
            origin += offset + direction.GetDirectionVector () * -skinWidth;
            return origin;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        private float GetCheckDistance (float skinWidth)
        {
            return maxDistance + skinWidth;
        }

        public void OnDrawGizmos (in Vector3 origin, float skinWidth)
        {
            var start = GetStartPosition (origin, skinWidth);
            var dir = direction.GetDirectionVector () * GetCheckDistance (skinWidth);
            
            Gizmos.DrawRay (start, dir);
        }
    }

}