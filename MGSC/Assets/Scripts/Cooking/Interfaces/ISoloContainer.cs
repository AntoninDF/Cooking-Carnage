using System;
using UnityEngine;

namespace Cooking
{

    public interface ISoloContainer : IContainer
    {
        public IGrabbable HeldItem { get; set; }
        public Transform ItemSlot {  get;}
    }

}