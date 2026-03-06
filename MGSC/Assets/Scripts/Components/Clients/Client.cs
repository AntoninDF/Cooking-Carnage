using Features;
using GameSystem;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This part of the client class handles the client's behaviour.
/// Movement is handled externally : See ClientMovement.cs
/// The client generation is defined in ClientGenerator.cs. It is joined to the Client through partial class
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public partial class Client : MonoBehaviour, IFeatureCollection
{
    public FeatureCollection Collection { get; set; } = new();

    public ClientBehaviour Behaviour { get; set; }
    


    private void Awake()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        Behaviour = new(this, agent);
        
    }

    private void Start()
    {
        Behaviour.Perform(Behaviour.DefaultAction);
    }

    // Find ClientTargets


    
    /*

    The client looks for seats.
    If none are found, they stay in place.
    If there are seats available, the client asks themselves : Am I hungry ?
    If so, they look for a seat associated to a OrderingCounter
    Otherwise they select another seat.


    */
}