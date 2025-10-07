using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(BoxCollider))]
public class ContainerSurface : MonoBehaviour
{
    public List<IContainable> Containables { get; set; }


}
