using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class MessageFader : MonoBehaviour
{
    public Animation Animation;

    public TextMeshProUGUI Text;

    public AnimationClip ToPlay;

    public EventHandler OnMessageShown;

    public void Animate()
    {
        Animation.Play(ToPlay.name);
    }

    public bool IsPlaying()
    {
        return Animation.IsPlaying(ToPlay.name);
    }

    public void MessageShown()
    {
        OnMessageShown?.Invoke(this, EventArgs.Empty);
    }
}
