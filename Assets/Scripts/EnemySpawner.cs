using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public void Spawn()
    {
        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            GameObject spawn = Instantiate(enemyPrefabs[enemyIndex], hit.point + GameManager.instance.spawnOffset, transform.rotation);
            if (spawn.GetComponent<Enemy>() != null)
            {
                spawn.GetComponent<Enemy>().target = GameManager.instance.playerRef.gameObject;
            }
        }

    }
}
