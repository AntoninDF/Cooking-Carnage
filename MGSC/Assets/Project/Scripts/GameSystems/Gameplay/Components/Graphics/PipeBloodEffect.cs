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
    [SerializeField, ColorUsage(false, true)] private Color _colorA;
    [SerializeField, ColorUsage(false, true)] private Color _colorB;
    [SerializeField, ColorUsage(false, true)] private Color _colorC;
    [Space]
    [SerializeField] private float _bloodAnimationDuration = 2f;

    private int _lastBlendedPipe = -1;

    private void OnEnable() => _onClientBlended.AddListener(Play);
    private void OnDisable() => _onClientBlended.RemoveListener(Play);
    public void Play()
    {
        var client = (_onClientBlended.Parameter as Client);

        EClientType clientType;
        if (client == null) clientType = EClientType.B;
        else clientType = client.Type;
        
        Color color = clientType switch
        {
            EClientType.A => _colorA,
            EClientType.B => _colorB,
            EClientType.C => _colorC,
            _ => Color.white
        };
        MeshRenderer pipe = _lastBlendedPipe switch
        {
            0 => _pipeA,
            1 => _pipeB,
            2 => _pipeC,
            _ => null
        };
        Animate(pipe, color);
    }
    /// <summary>Called by UnityEvents/// </summary>
    public void SetLastBlendedPipe(int number) => _lastBlendedPipe = number;

    private void Animate(MeshRenderer pipe, Color color)
    {
        if (pipe == null) return;
        TimerManager.StartGameplayTimer("PipeAnimation " + pipe.name, _bloodAnimationDuration, null, UpdatePipe(pipe, color));
        Debug.Log("Animating pipe: " + pipe.name + " for duration: " + _bloodAnimationDuration);
    }

    private Action<float> UpdatePipe(MeshRenderer pipe, Color color)
    {
        return (float t) =>
        {
            if (pipe == null) return;
            t = t / _bloodAnimationDuration;
            Debug.Log("Animation progress for pipe " + pipe.name + ": " + t);
            float bloodAmount = Mathf.Lerp(1, -1, t);
            MaterialPropertyBlock mpb = new();
            pipe.GetPropertyBlock(mpb);
            mpb.SetFloat("_LiquidAnimation", bloodAmount);
            mpb.SetColor("_Liquid_Color", color);
            mpb.SetColor("_Liquid_Color_2", color + new Color(0.05f, 0.05f, 0.05f));
            pipe.SetPropertyBlock(mpb);
        };
    }
}
