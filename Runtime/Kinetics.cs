using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Hirame.Heracles
{
    public static class Kinetics
    {
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float TimeToApex (float height, float gravity)
        {
            return math.sqrt (math.abs (2 * height / gravity));
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetVelocityToReachApex (float height, float gravity)
        {
            var timeToApex = TimeToApex (height, gravity);
            return math.abs (gravity) * timeToApex;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetImpulseToReachApex (float height, float gravity, float deltaTime)
        {
            var velocity = GetVelocityToReachApex (height, gravity);
            return GetRequiredAcceleration (velocity, 1f, deltaTime);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float CalculateOptimalDrag (float acceleration, float targetSpeed)
        {
            return math.abs (acceleration / targetSpeed);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float ApplySlipperiness (float frameAcceleration, PhysicMaterial material)
        {
            return frameAcceleration * (1 - material.dynamicFriction);
        }
        
        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float CalculateAcceleration (float time, float targetSpeed, float drag)
        {
            return targetSpeed / time + drag;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float TimeToAccelerate (float targetSpeed, float acceleration, float drag)
        {
            return targetSpeed / (acceleration - drag);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static Vector3 ApplyDrag (Vector3 velocity, float drag, float deltaTime)
        {
            return velocity - deltaTime * drag * velocity;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float AccelerateTowards (float velocity, float acceleration, float drag, float deltaTime)
        {
            return velocity + (acceleration - drag * velocity) * deltaTime;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetFinalVelocity (float velocityChange, float drag, float deltaTime)
        {
            return velocityChange * (1 / math.clamp (drag * deltaTime, 0, 1) - 1);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetFinalVelocityFromAcceleration (float acceleration, float drag, float deltaTime)
        {
            return GetFinalVelocity (acceleration * deltaTime, drag, deltaTime);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetDrag (float velocityChange, float finalVelocity, float deltaTime)
        {
            return velocityChange / ((finalVelocity + velocityChange) * deltaTime);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetRequiredVelocityChange (float finalSpeed, float drag, float deltaTime)
        {
            var m = math.clamp (drag * deltaTime, 0, 1);
            return finalSpeed * m / (1 - m);
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public static float GetRequiredAcceleration (float finalSpeed, float drag, float deltaTime)
        {
            return GetRequiredVelocityChange (finalSpeed, drag, deltaTime) / deltaTime;
        }
    }
}