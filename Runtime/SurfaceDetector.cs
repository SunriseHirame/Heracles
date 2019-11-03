using UnityEngine;

namespace Hirame.Heracles
{
    [System.Serializable]
    public class SurfaceDetector
    {
        [SerializeField] private LayerMask validLayers;
        [SerializeField] private Vector3 scanSize = new Vector3 (0.4f, 0.02f, 0.04f);
        [SerializeField] private Vector3 offset = new Vector3 (0, 0.03f, 0);
        [SerializeField] private Vector3 direction = Vector3.down;
        [SerializeField] private float maxDistance = 0.2f;

        public SurfaceInfo Scan (in Vector3 origin)
        {
            var startPosition = GetStartPosition (origin);
            var checkDistance = maxDistance + scanSize.y * 1.1f + offset.y;
            
            var hitSomething = Physics.BoxCast (startPosition, scanSize, direction, out var hitInfo,
                Quaternion.identity, checkDistance, validLayers);

            if (!hitSomething)
                return SurfaceInfo.Default;
            
            return new SurfaceInfo
            {
                InContact = true,
                Normal = hitInfo.normal,
                Layer = hitInfo.collider.gameObject.layer
            };
        }

        private Vector3 GetStartPosition (Vector3 origin)
        {
            origin += offset;
            origin.y += scanSize.y * 1.1f;
            return origin;
        }
    }

}