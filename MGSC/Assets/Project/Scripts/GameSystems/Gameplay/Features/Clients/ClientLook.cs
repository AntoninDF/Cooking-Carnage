using Clients;
using UnityEngine;

namespace Features
{
    /// <summary>
    /// Handle the look direction of the client 
    /// </summary>
    public class ClientLook : Feature
    {

        #region Properties

        #endregion
        
        #region Fields
        
        private Vector3 _lookDirection =  Vector3.forward;
        private Vector3 _previousDirection;
        private Transform _client;
        private bool isSitting;

        #endregion

        public ClientLook(IFeatureCollection owner, Transform clientTransform) : base(owner)
        {
            _client = clientTransform;
        }

        public override void Update()
        {
            if (isSitting)
            {
                Debug.Log("Is Not rotating");
                return;
            }
            Quaternion targetRot = Quaternion.LookRotation(Vector3.Lerp(_previousDirection, _lookDirection, Time.deltaTime * 5));
            Quaternion applyedRot = new Quaternion(0, targetRot.y, 0, targetRot.w);
            _client.rotation = Quaternion.Lerp(_client.rotation, applyedRot, Time.deltaTime * 5f);
        }
        
        
        public void SetTarget(Vector3 targetDir)
        {
            _previousDirection = _lookDirection;
            _lookDirection = targetDir;
        }

        public void SetClientSitting(Client client)
        {
            isSitting = true;
        }
        
        
    }
    
}