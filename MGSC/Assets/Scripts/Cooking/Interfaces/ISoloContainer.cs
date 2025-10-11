using System;
using UnityEngine;

namespace Cooking
{

    public interface ISoloContainer : IContainer
    {
        public IGrabbable HeldItem { get;}
        public Transform ItemSlot {  get;}
    }

}