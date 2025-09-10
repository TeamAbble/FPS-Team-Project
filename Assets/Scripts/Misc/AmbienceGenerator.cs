using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceGenerator : MonoBehaviour
{
    public AudioSource source; public Vector2 randomNoiseTimes; public AudioClip[] randomNoises;
    private void Start()
    {
        StartCoroutine(RandomSoundLoop());
    }
    IEnumerator RandomSoundLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(randomNoiseTimes.x, randomNoiseTimes.y));
            source.PlayOneShot(randomNoises[Random.Range(0, randomNoises.Length)]);
        }
    }
}
