using Cooking;
using System;
using UnityEngine;

namespace Controller
{

    /// <summary>
    /// Every interaction with the player should go through this interface.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerInteractor))]
    public class PlayerInterface : MonoBehaviour
    {
        #region Properties
        // Interactor
        public PlayerController Controller => _controller;
        public PlayerInteractor Interactor => _interactor;
        public PlayerInputHandler InputHandler => _inputHandler;
        public RaycastHit LookInfo => Interactor.LookInfo;
        public GameObject LookTarget => Interactor.LookTarget;
        public IContainer LookContainer => Interactor.LookContainer;
        public IGrabbable HeldItem => Interactor.HeldItem;
        #endregion

        #region Fields

        private PlayerController _controller;
        private PlayerInteractor _interactor;
        private PlayerInputHandler _inputHandler;
        #endregion


        #region Methods
        // Player controller
        void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _interactor = GetComponent<PlayerInteractor>();
            _inputHandler = GetComponent<PlayerInputHandler>();

            _controller.Player = this;
            _interactor.Player = this;
            _inputHandler.Player = this;
        }
        #endregion
    }

}