using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vendor : Purchasable
{
    public override int Cost => base.Cost;
    public ParticleSystem particle;
    public AudioSource audioSource;
    public AudioClip clip;
    public override void Purchase()
    {
        base.Purchase();
        if (particle)
            particle.Emit(1);
        if (audioSource && clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
