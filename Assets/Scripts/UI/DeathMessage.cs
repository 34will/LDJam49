using System;
using UnityEngine;

[RequireComponent(typeof(Animation))]
public class DeathMessage : MonoBehaviour
{
    private Animation animationComponent;

    public AnimationClip ToPlay;

    public EventHandler OnMessageShown;

    public void Awake()
    {
        animationComponent = GetComponent<Animation>();
    }

    public void Animate()
    {
        animationComponent.Play(ToPlay.name);
    }

    public void MessageShown()
    {
        OnMessageShown?.Invoke(this, EventArgs.Empty);
    }
}
