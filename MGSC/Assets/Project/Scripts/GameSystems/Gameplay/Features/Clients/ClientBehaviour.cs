using System;
using Clients;
using GameSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

namespace Features
{
    public class ClientBehaviour : Feature, IFeatureCollection
    {
        
        #region Properties

        public FeatureCollection Collection     { get; set; } = new();
        public ClientMovement    ClientMovement { get; set; }
        public NavMeshAgent      Agent          { get; set; }

        public BaseClientAction CurrentAction { get; private set; }

        public Action             OnSitting        { get; set; }
        public Action      OnSplineUpdate { get; set; }
        public WaitInQueue WaitInQueue    { get; private set; }
        

        #endregion

        #region Fields

        private IFeatureCollection _owner;
        private Transform          _clientTransform;
        private SplineContainer    _splineComponent;
        private float              _speed;
        private float              _eatingTime = 3;
        private WaitInQueue        _waitAction;
        
        #endregion

        public ClientBehaviour(IFeatureCollection owner, Transform clientTransform, NavMeshAgent agent, SplineContainer splineComponent, float speed) : base(owner)
        {
            _owner           = owner;
            _clientTransform = clientTransform;
            _splineComponent = splineComponent;
            _speed           = speed;
            Agent            = agent;
        }

        public override void Update()
        {
            if (ClientMovement == null) return;
            Collection.Update();
            ClientMovement.Update();
            OnSplineUpdate?.Invoke();
        }

        #region Behavior Logic

        public void Begin()
        {
            var client = _owner as Client;
            ClientMovement = new ClientMovement(this, _clientTransform, Agent, client.SplineComp, client.Speed);         
            ClientMovement.SwitchToSpline(0f);
            
            client.InitFeatures(ClientMovement);

            Perform(BuildSelectSeat());
        }
        
        private void EnqueueAndWait()
        {
            WaitInQueue = new WaitInQueue(this);
            _waitAction = WaitInQueue;
            Perform(_waitAction);

            var queue = Reference.GetManager<ClientManager>().WaitingQueue;
            queue.OnSeatAvailable += OnQueueSeatAvailable;
        }
        
        public void OnQueueSeatAvailablePublic(Client client, ClientSeat seat) => OnQueueSeatAvailable(client, seat);

        private void OnQueueSeatAvailable(Client client, ClientSeat seat)
        {
            if (client != Owner as Client) return;

            var queue = Reference.GetManager<ClientManager>().WaitingQueue;
            queue.OnSeatAvailable -= OnQueueSeatAvailable;
            
            Perform(BuildSelectSeat());
    
            _waitAction?.StopWaiting();
            _waitAction = null;

        }
        
        private void Perform(BaseClientAction action)
        {
            CurrentAction = action;
            action.Perform();
        }

        #endregion

        #region BaseClientAction Builds

        private SelectSeat BuildSelectSeat()
        {
            var select = new SelectSeat(this);
            select.OnSeatFound    += seat => Perform(BuildMoveToSeat(seat));
            select.OnSeatNotFound += EnqueueAndWait;
            return select;
        }

        private MoveToSeat BuildMoveToSeat(ClientSeat seat)
        {
            var move            = new MoveToSeat(this, seat);
            move.OnSeatReached  += s => Perform(BuildSitAndWait(s));
            return move;
        }

        private SitAndWaitForServing BuildSitAndWait(ClientSeat seat)
        {
            var sit        = new SitAndWaitForServing(this, seat);
            sit.OnServed   += () => CanLeave();
            return sit;
        }

        private void CanLeave()
        {
            if ((_owner as Client)?.IsCooked == false) Perform(BuildEat());
        }

        private Eat BuildEat()
        {

            var eat = new Eat(this, _eatingTime);
            eat.StopEating += () =>
            {

                Perform(BuildLeave());
            };
            return eat ;
        }

        private Leave BuildLeave()
        {
            var leave = new Leave(this);
            leave.OnExited += () =>
                              {
                                  UnityEngine.Object.Destroy((Owner as Client)?.gameObject);
                              };
            return leave;
        }
        #endregion
        
        public void SetWaitPosition(float t)
        {
            ClientMovement.SetSplinePos(t);
        }
        
        
        
    }
}
