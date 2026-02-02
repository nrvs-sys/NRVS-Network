using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public interface IServerStopper
    {
        /// <summary>Final step that ends the lifetime of this server (deployment/process).</summary>
        IEnumerator StopAsync(string reason);
    }

    /// <summary>
    /// Edgegap self-stop: DELETE ARBITRIUM_DELETE_URL with ARBITRIUM_DELETE_TOKEN.
    /// If env vars are missing, this is considered unavailable.
    /// </summary>
    public sealed class EdgegapServerStopper : IServerStopper
    {
        const int RequestTimeoutSeconds = 10;
        readonly string _deleteUrl = Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_URL");
        readonly string _deleteToken = Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_TOKEN");

        public static bool CanUseEdgegapEnv()
            => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_URL"));

        public IEnumerator StopAsync(string reason)
        {
            if (string.IsNullOrWhiteSpace(_deleteUrl))
            {
                Debug.Log("[EdgegapStopper] Missing ARBITRIUM_DELETE_URL. Skipping.");
                yield break;
            }

            Debug.Log($"[EdgegapStopper] Stopping deployment via {_deleteUrl} (reason={reason}).");
            using (var req = UnityWebRequest.Delete(_deleteUrl))
            {
                if (!string.IsNullOrWhiteSpace(_deleteToken))
                    req.SetRequestHeader("Authorization", _deleteToken);

                req.timeout = RequestTimeoutSeconds;
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                    Debug.LogError($"[EdgegapStopper] HTTP {(int)req.responseCode}: {req.error}");
                else
                    Debug.Log($"[EdgegapStopper] Stop request accepted (HTTP {(int)req.responseCode}).");
            }

            // Give the orchestrator a moment to send SIGTERM
            yield return new WaitForSeconds(1f);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(0);
#endif
        }
    }

    /// <summary>
    /// Local/dev fallback: stop the Unity server (if provided) and optionally quit the process.
    /// </summary>
    public sealed class LocalServerStopper : IServerStopper
    {
        readonly ConnectionToggleBehaviour _toggle;
        readonly bool _quitProcess;

        public LocalServerStopper(ConnectionToggleBehaviour toggle, bool quitProcess = true)
        {
            _toggle = toggle;
            _quitProcess = quitProcess;
        }

        public IEnumerator StopAsync(string reason)
        {
            Debug.Log($"[LocalStopper] Stopping local server (reason={reason}).");
            _toggle?.GoOffline();

            yield return null; // a frame to flush logs

            if (_quitProcess)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit(0);
#endif
            }
        }
    }
}
