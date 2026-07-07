using System;
using Clients;
using GameSystem;
using UnityEngine;

namespace Features
{
	public class ClientCooked : Feature
	{
		public Action OnCustomerCooked { get; set; }

		private float  _destroyDelay;
		private float  _timer;
		private bool   _isRunning;
		private Client _client;
		
		public ClientCooked(IFeatureCollection owner, float destroyDelay = 2f) : base(owner)
		{
			_client       = owner as Client;
			_destroyDelay = destroyDelay;
			
		}
		
		public void Trigger()
		{
			if (_isRunning) return;
			_isRunning       = true;
			_timer           = 0f;
			_client.IsCooked = true;
			_client.CurrentOrder.Cancel();
			OnCustomerCooked?.Invoke();
			_client.CurrentSeat?.TryUnregister(_client);

		}

		public override void Update()
		{
			if (!_isRunning) return;

			_timer += Time.deltaTime;
			if (_timer >= _destroyDelay)
				DestroyClient();
		}

		private void DestroyClient()
		{
			_isRunning = false;

			if (Reference.TryGetManager(out ClientManager cm))
			{
				
				//cm.AddClientData(_client, isDone: true, killed: true);
				cm.WaitingQueue?.Remove(_client);
			}

			var queue = cm?.WaitingQueue;
			if (queue != null) queue.OnSeatAvailable -= _client.Behaviour.OnQueueSeatAvailablePublic;

			GameObject.Destroy(_client.gameObject);
		}
	}
}