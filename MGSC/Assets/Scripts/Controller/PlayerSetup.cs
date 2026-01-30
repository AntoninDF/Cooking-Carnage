using GameSystem;
using UnityEngine;

namespace Controller
{

    public static class PlayerSetup
    {

        /// <summary>
        /// Creates a player if none exists, then resets it's position, and adds it to the reference manager
        /// </summary>
        /// <returns></returns>
        public static PlayerInterface TrySetupPlayer()
        {
            PlayerInterface player = null;
            if (Reference.GetAll<PlayerInterface>().Count == 0)
            {
                TrySpawnPlayer(out player);
            }
            TryResetPlayer(player);
            Reference.Instance.TryAddElement(player);

            return player;
        }


        public static bool TrySpawnPlayer(out PlayerInterface player)
        {
            player = null;
            if (!CheckReference(Reference.AssetReferences,
                "[PlayerSetup] : Cannot spawn the player because no Asset References SO was found."))
                return false;
            if (!CheckReference(Reference.AssetReferences.Player,
                "[PlayerSetup] : Cannot spawn the player because no Player prefab was found in the Asset References SO."))
                return false;

            GameObject playerPrefab = Reference.AssetReferences.Player;
            if (!playerPrefab.HasComponent<PlayerInterface>())
            {
                Debug.LogError("[PlayerSetup] : The player prefab referenced in the asset references SO doesn't have the player interface component.");
                return false;
            }

            player = SpawnPlayer(playerPrefab);

            return true;
        }

        /// <summary>
        /// Instantiate the player prefab in the scene
        /// </summary>
        private static PlayerInterface SpawnPlayer(GameObject playerPrefab)
        {
            GameObject playerGo = GameObject.Instantiate(playerPrefab);
            return playerGo.GetComponent<PlayerInterface>();
        }

        public static bool TryResetPlayer(PlayerInterface player)
        {
            PlayerStart playerStart = Reference.GetObject<PlayerStart>();
            Vector3 pos;
            Quaternion rot;
            if (playerStart == null)
            {
                pos = Vector3.zero;
                rot = Quaternion.identity;
            }
            else
            {
                pos = playerStart.transform.position;
                rot = playerStart.transform.rotation;
            }
            if (!EnforceReference(player,
                "[Player Setup] : The player to be reset was null"))
                return false;

            ResetPlayer(player, pos, rot);
            return true;
        }

        private static void ResetPlayer(PlayerInterface player, Vector3 position, Quaternion rotation)
        {
            player.transform.position = position;
            player.transform.rotation = rotation;
            SetPlayerReferences(player);
        }


        /// <summary>
        /// Assign the input action from the scene preset to the player
        /// </summary>
        private static void SetPlayerReferences(PlayerInterface player)
        {
            if (Reference.AssetReferences.InputActions == null)
                Debug.LogWarning("[PlayerSetup] : Please assign the input action asset in the asset references SO");
            player.InputHandler.PlayerControls = Reference.AssetReferences.InputActions;
        }

        private static bool EnforceReference(Object obj, string message="Object not set to an instance of an object")
        {
            if (obj == null) Debug.LogError(message);
            return obj != null;
        }

        private static bool CheckReference(Object obj, string message="Object not set to an instance of an object")
        {
            if (obj == null) Debug.LogWarning(message);
            return obj != null;
        }
    }
}