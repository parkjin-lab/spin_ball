using System.Globalization;
using UnityEngine;

namespace MCPForUnity.Runtime.Helpers
{
    /// <summary>
    /// Unity 6.5 object-identity helpers. EntityId is 64-bit; JSON/API fields
    /// that stay named instanceID store EntityId.ToULong so lookups round-trip.
    /// </summary>
    public static class UnityObjectIdentity
    {
        public static ulong ToSerializedId(this UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return 0UL;
            }

            return EntityId.ToULong(obj.GetEntityId());
        }

        public static EntityId FromSerializedId(ulong raw)
        {
            return EntityId.FromULong(raw);
        }

        public static bool TryParseSerializedId(string text, out ulong raw)
        {
            raw = 0UL;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return ulong.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out raw);
        }

        /// <summary>
        /// Deterministic int for System.Random seeds only. XOR-folds
        /// EntityId.ToULong so both 32-bit halves contribute without an
        /// EntityId-to-int cast (Unity treats that as identifier truncation).
        /// Do not use this value for object lookup.
        /// </summary>
        public static int ToDeterministicSeed(this UnityEngine.Object obj)
        {
            if (obj == null)
            {
                return 0;
            }

            ulong raw = EntityId.ToULong(obj.GetEntityId());
            return unchecked((int)(raw ^ (raw >> 32)));
        }
    }
}
