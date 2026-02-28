using System;
using UnityEngine;

namespace Features
{
    public abstract class BasePlayerInputs : Feature
    {
        public Action OnPause { get; set; }
        public Action OnPrimaryAction { get; set; }
        public Action OnMovement { get; set; }
        public Action OnLook { get; set; }

        public Vector2 MovementDirection { get; set; }
        public Vector2 LookOffset { get; set; } // X is Yaw, Y is Pitch


        protected BasePlayerInputs(IFeatureCollection owner) : base(owner)
        {
        }
    }
}