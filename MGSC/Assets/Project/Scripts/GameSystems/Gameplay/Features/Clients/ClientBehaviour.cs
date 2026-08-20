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

        public Action      OnSitting      { get; set; }
        public Action      OnSplineUpdate { get; set; }
        public Action      OnEatStart     { get; set; }
        public Action      OnEatEnd       { get; set; }
        public Action      OnLeaving      { get; set; }
        public Action      OnMealServed     { get; set; }
        
        public Action GoodReaction { get; set;  }
        public Action BadReaction   { get; set; }
        
        public Action PatienceReaction { get; set; }
        
        public Action OnExitQueue { get; set; }
        public Action OnQueuePointReached { get; set; }
        public Action<ClientSeat>      OnLeavingSeat      { get; set; }
        public Action OnReactionStart { get; set; }
        public Action OnReactionEnd      { get; set; }
        

        #endregion

        #region Fields

        private Client _client;
        private IFeatureCollection _owner;
        private Transform          _clientTransform;
        private SplineContainer    _splineComponent;
        private float              _speed;
        private float              _eatingTime = 3;
        private float _reactionTime = 2;
        private float _seatDownTime = 1.25f;
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
            if (GameLoop.IsPaused) return;
            if (ClientMovement == null) return;
            Collection.Update();
            ClientMovement.Update();
            OnSplineUpdate?.Invoke();
        }

        #region Behaviour Logic

        public void Begin()
        {
            _client = _owner as Client;
            ClientMovement = new ClientMovement(this, _clientTransform, Agent, _client.SplineComp, _client.Speed);         
            ClientMovement.SwitchToSpline(0f);
            
            _client.InitFeatures(ClientMovement);

            Perform(BuildSelectSeat());
        }
        
        private void EnqueueAndWait()
        {
            _waitAction = new WaitInQueue(this);
            
            _waitAction.OnQueuePointReached += () =>
            {
                _client.ClientLook.SetWaiting(true);
                OnQueuePointReached?.Invoke();
            };

            _waitAction.OnExitQueue += () =>
            {
                _client.ClientLook.SetWaiting(false);
                OnExitQueue?.Invoke();
            };
            
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
            if (_client.IsCooked == false && !_client.IsAngry ) Perform(BuildEat());
            else if (_client.IsAngry)
            {
                OnEatEnd?.Invoke();
                Perform(BuildReact());
                //TimerManager.StartTimer($"{_client.UniqueID} Wait For finishing the order ", _reactionTime, WaitForFinishingDownAnim);
                WaitForFinishingDownAnim(); //PlayUnique instant because they just left, they had no food Madge
            }
        }

        private Eat BuildEat()
        {

            Eat eat = new Eat(this, _eatingTime);
            eat.StartEating += OnEatStart;
            eat.StopEating += () => 
            { 
                OnEatEnd?.Invoke();
                WaitForFinishingEat();
            };
            return eat ;
        }

        private Leave BuildLeave()
        {
            var leave = new Leave(this);
            leave.OnExited += () =>
                              {
                                  UnityEngine.Object.Destroy(_client.gameObject);
                              };
            OnLeaving?.Invoke();
            return leave;
        }

        private React BuildReact()
        {
            var react = new React(this, _client.OrderLog);
            react.BadReaction += BadReaction;
            react.GoodReaction += GoodReaction;
            return react;
        }
        
        #endregion
        
        #region Timers

        private void WaitForFinishingEat()
        {
            OnReactionStart?.Invoke();
            Perform(BuildReact());
            TimerManager.StartGameplayTimer($"{_client.UniqueID} Wait For finishing the order ", _reactionTime, WaitForFinishingDownAnim);
        }

        private void WaitForFinishingDownAnim()
        {
            OnReactionEnd?.Invoke();
            TimerManager.StartTimer($"{_client.UniqueID} Wait finishing the react animation", _seatDownTime, () =>
            {
                Perform(BuildLeave());
                _client.QuitSeat();
            });
        }

        #endregion
        
        public void SetWaitPosition(float t)
        {
            ClientMovement.SetSplinePos(t);
        }
        
        
        
    }
}
