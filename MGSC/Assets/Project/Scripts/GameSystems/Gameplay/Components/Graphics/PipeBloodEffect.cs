using Clients;
using GameSystem;
using System;
using UnityEngine;

public class PipeBloodEffect : MonoBehaviour
{
    [SerializeField] private MeshRenderer _pipeA;
    [SerializeField] private MeshRenderer _pipeB;
    [SerializeField] private MeshRenderer _pipeC;
    [SerializeField] private EventVariable _onClientBlended;
    [Space]
    [SerializeField] private float _bloodAnimationDuration = 2f;

    private void OnEnable() => _onClientBlended.AddListener(Play);
    private void OnDisable() => _onClientBlended.RemoveListener(Play);
    public void Play()
    {
        var client = (_onClientBlended.Parameter as Client);
        EClientType clientType;
        if (client == null)
            clientType = EClientType.B;
        else
            clientType = client.Type;
        switch (clientType)
        {
            case EClientType.A:
                Animate(_pipeA);
                break;
            case EClientType.B:
                Animate(_pipeB);
                break;
            case EClientType.C:
                Animate(_pipeC);
                break;
        }
    }

    private void Animate(MeshRenderer pipe)
    {
        if (pipe == null) return;
        TimerManager.StartGameplayTimer("PipeAnimation " + pipe.name, _bloodAnimationDuration, null, UpdatePipe(pipe));
    }

    private Action<float> UpdatePipe(MeshRenderer pipe)
    {
        return (float t) =>
        {
            if (pipe == null) return;
            float bloodAmount = Mathf.Lerp(-1, 1, t);
            pipe.material.SetFloat("_LiquidAnimation", bloodAmount);
        };
    }
}
