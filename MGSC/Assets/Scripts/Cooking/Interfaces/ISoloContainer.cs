using System;
using UnityEngine;

namespace Cooking
{

    public interface ISoloContainer : IContainer
    {
        public Transform ItemSlot {  get;}
    }

}