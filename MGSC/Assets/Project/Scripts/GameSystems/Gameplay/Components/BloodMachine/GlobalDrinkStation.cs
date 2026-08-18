using Clients;
using Features;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Upgrades;

namespace Project.Scripts.BloodMachine
{
	public class GlobalDrinkStation : MonoBehaviour, IFeatureCollection, IUpgradable<UnlockDrinkMachine>
	{
		#region Properties
		public FeatureCollection Collection      { get; set; } = new();
		public List<DrinkStation> DrinkStations { get => _drinkStations; set => _drinkStations = value; }

        #endregion

        #region Fields
		[SerializeField] private List<DrinkStation> _drinkStations;
		[SerializeField] private EventVariable EventInvokedUponBlending;
        #endregion


        private void Start() => InitializeStations();

		public void InitializeStations()
		{
			foreach(var station in _drinkStations)
			{
				station.Initialize(this);
				station.reserve.BloodQuantity = 0;
            }
			EventInvokedUponBlending.AddListener(UponClientBlend);

        }

		void UponClientBlend() => UponClientBlend(EventInvokedUponBlending.Parameter as Client);
		void UponClientBlend(Client client)
        {
            Debug.Log("GlobalDrinkStation.UponClientBlend invoked");
            if (client == null) return;
            Debug.Log("GlobalDrinkStation.UponClientBlend client not null");
			var station = GetStation(client.Type);
			if (station == null) return;
			float bloodGivenByClient = UnityEngine.Random.Range(client.BloodDropRate.x, client.BloodDropRate.y);
            station.reserve.BloodQuantity += bloodGivenByClient;
            Debug.Log("GlobalDrinkStation.UponClientBlend passed");
		}
		/// <summary>Get the drink station for a given client type. Can be null if the station wasn't assigned in the editor</summary>
		public DrinkStation GetStation(EClientType clientType)
			=> DrinkStations.Where(station => station.Type == clientType).FirstOrDefault();


        public void OnUpgradeInit(UnlockDrinkMachine upgrade) { }

        public void OnUpgraded(UnlockDrinkMachine upgrade) { }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

			// Setup default values and helpers to display labels in the scene
            float baseHandleHeight = 1f;
            float increment = 0.2f;
            int handleCount = 0;
            float GetY() => baseHandleHeight + handleCount++ * increment;

			// Make the spacing between labels proportional the the camera distance (and thus have a relatively screen-static spacing)
            var cam = SceneView.lastActiveSceneView.camera;
            if (cam != null) increment = Vector3.Distance(cam.transform.position, transform.position) / 50f;

			// This displays info about the stations in the scene, to make real-time debugging easier in editor
			void DisplayInfoForDrinkStation(DrinkStation drinkStation)
			{
				Vector3 GetPos() => drinkStation.DrinkSpawnPoint.transform.position + Vector3.up * GetY();
                handleCount = 0;
                Gizmos.color = Color.white;
                UnityEditor.Handles.Label(GetPos(), "Station type : " + drinkStation.Type);
                UnityEditor.Handles.Label(GetPos(), "Reserve : " + Mathf.Round(drinkStation.reserve.BloodQuantity*100)/100 + "cL (ratio is " 
																					+ Mathf.Round(drinkStation.ReserveFill01 * 100) / 100 + "/1)");
				UnityEditor.Handles.Label(GetPos(), "Is serving ? : " + (drinkStation.valve.IsServing ? "Yup" : "Nope"));
				UnityEditor.Handles.Label(GetPos(), "Serving timer : " + 
					(drinkStation.valve.ServingTimer != null ? (Mathf.Round(drinkStation.valve.ServingTimer.Elapsed * 100) / 100) + "/" + drinkStation.valve.ServingTimer.Duration + "s" : "0/0s"));
				UnityEditor.Handles.Label(GetPos(), "Overflowing timer : " + 
					(drinkStation.valve.OverflowingTimer != null ? (Mathf.Round(drinkStation.valve.OverflowingTimer.Elapsed * 100) / 100) + "/" + drinkStation.valve.OverflowingTimer.Duration + "s" : "0/0s"));

			}

			foreach (var station in DrinkStations)
				DisplayInfoForDrinkStation(station);
        }
#endif
    }
}