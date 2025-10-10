using Cooking;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IMultiContainer : IContainer
{
    public List<IGrabbable> HeldItems { get; set; }
}
