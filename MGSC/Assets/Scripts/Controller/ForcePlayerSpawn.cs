using Controller;
using GameSystem;
using UnityEngine;

namespace Player
{

    public class ForcePlayerSpawn : MonoBehaviour
    {
        [SerializeField] private SO_AssetReferences _assetReferences;
        private void Start()
        {
            Reference.AssetReferences = _assetReferences;
            PlayerSetup.TrySetupPlayer();
            Destroy(gameObject);
        }

    }

}