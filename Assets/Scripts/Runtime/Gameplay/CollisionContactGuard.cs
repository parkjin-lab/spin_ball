using UnityEngine;
using Object = UnityEngine.Object;

namespace AlienCrusher.Gameplay
{
    /// <summary>
    /// Unity physics can still deliver OnCollision* after the other collider
    /// (or this host) was Destroy()'d earlier in the same tick. Accessing
    /// collision.gameObject / GetContact / isActiveAndEnabled on a fake-null
    /// Unity object throws MissingReferenceException.
    /// </summary>
    internal static class CollisionContactGuard
    {
        public static bool IsUnityAlive(Object obj)
        {
            return (Object)(object)obj != (Object)null;
        }

        public static bool IsBehaviourLive(Behaviour behaviour)
        {
            return IsUnityAlive(behaviour) && behaviour.isActiveAndEnabled;
        }

        public static bool TryGetLiveOther(Collision collision, out Collider otherCollider, out GameObject otherObject)
        {
            otherCollider = null;
            otherObject = null;
            if (collision == null)
            {
                return false;
            }

            try
            {
                otherCollider = collision.collider;
            }
            catch (MissingReferenceException)
            {
                otherCollider = null;
                return false;
            }

            if (!IsUnityAlive(otherCollider))
            {
                otherCollider = null;
                return false;
            }

            otherObject = otherCollider.gameObject;
            if (!IsUnityAlive(otherObject))
            {
                otherCollider = null;
                otherObject = null;
                return false;
            }

            return true;
        }

        public static Vector3 GetContactPointOrFallback(Collision collision, Vector3 fallback)
        {
            if (collision == null || collision.contactCount <= 0)
            {
                return fallback;
            }

            Collider otherCollider;
            try
            {
                otherCollider = collision.collider;
            }
            catch (MissingReferenceException)
            {
                return fallback;
            }

            if (!IsUnityAlive(otherCollider))
            {
                return fallback;
            }

            try
            {
                return collision.GetContact(0).point;
            }
            catch (MissingReferenceException)
            {
                return fallback;
            }
        }
    }
}
