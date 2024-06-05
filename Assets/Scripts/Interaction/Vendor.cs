using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vendor : Purchasable
{
    public override int Cost => base.Cost;
    public ParticleSystem particle;
    public AudioSource audioSource;
    public AudioClip clip;
}
