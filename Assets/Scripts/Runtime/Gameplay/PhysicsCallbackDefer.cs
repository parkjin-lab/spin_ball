using System;
using System.Collections.Generic;
using UnityEngine;

namespace AlienCrusher.Gameplay
{
    /// <summary>
    /// PhysX can abort the editor/player (Unity Bug Reporter, type Crash) when
    /// collision callbacks Destroy, disable, reparent, or add Rigidbodies on
    /// objects still in the current contact graph. Queue those mutations until
    /// after the physics step. Durability, score, and feedback stay immediate.
    /// </summary>
    [DefaultExecutionOrder(32000)]
    internal sealed class PhysicsCallbackDefer : MonoBehaviour
    {
        private static readonly List<Action> pending = new List<Action>(32);
        private static PhysicsCallbackDefer instance;
        private static bool flushing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            pending.Clear();
            instance = null;
            flushing = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureInstance();
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            var go = new GameObject("PhysicsCallbackDefer");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            instance = go.AddComponent<PhysicsCallbackDefer>();
        }

        public static void RunAfterPhysics(Action action)
        {
            if (action == null)
            {
                return;
            }

            pending.Add(action);
            if (instance == null)
            {
                EnsureInstance();
            }
        }

        private void LateUpdate()
        {
            Flush();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private static void Flush()
        {
            if (flushing || pending.Count == 0)
            {
                return;
            }

            flushing = true;
            try
            {
                while (pending.Count > 0)
                {
                    var action = pending[0];
                    pending.RemoveAt(0);
                    try
                    {
                        action?.Invoke();
                    }
                    catch (MissingReferenceException)
                    {
                    }
                }
            }
            finally
            {
                flushing = false;
            }
        }
    }
}
