using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] pref;
    public Transform[] pos; 
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Spawn(pref[0], pos[0]);
        }
    }

    public void Spawn(GameObject prefab,Transform pos)
    {
        GameObject spawn = Instantiate(prefab);
        spawn.transform.position = pos.position;
        
    }
    
}
