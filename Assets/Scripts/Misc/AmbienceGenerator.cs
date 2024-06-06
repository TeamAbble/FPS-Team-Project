using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceGenerator : MonoBehaviour
{
    public AudioSource source; public Vector2 randomNoiseTimes; public AudioClip[] randomNoises;
    void PlayRandomSound()
    {
        source.PlayOneShot(randomNoises[Random.Range(0, randomNoises.Length)]);
        Invoke(nameof(PlayRandomSound), Random.Range(randomNoiseTimes.x, randomNoiseTimes.y));
    }
}
