using UnityEngine;
using System.Collections;

public abstract class Item : MonoBehaviour
{
    public AudioClip[] collectSounds; // Sounds to pick from when item is collected
    
    protected abstract void OnCollect(Player player);

    public void Collect(Player player)
    {
        SoundManager.instance.RandomizeSfx(collectSounds);
        OnCollect(player);
    }
}