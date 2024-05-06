using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] pref;
    public Transform[] pos;
    public GameObject playerRef; 
    public int spawnCount = 15;
    public float spawnRate = 3;
    float timer=0;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {

            Spawn(pref[Random.Range(0, pref.Length - 1)], pos[Random.Range(0, pos.Length - 1)]);
            timer = 0;
            spawnCount -= 1;

        }
        
    }
    public void Spawn(GameObject prefab,Transform pos)
    {
        GameObject spawn = Instantiate(prefab);
        spawn.transform.position = pos.position;
        if (spawn.GetComponent<Enemy>()!= null)
        {
            spawn.GetComponent<Enemy>().target = playerRef.transform;
        }
        
    
        
    }
    
}
