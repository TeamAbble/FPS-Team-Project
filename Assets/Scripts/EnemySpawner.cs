using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public void Spawn()
    {
        int enemyIndex = Random.Range(0, GameManager.instance.waves[GameManager.instance.waveContainerIndex].allowedEnemies.Count);
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            GameObject spawn = Instantiate(GameManager.instance.waves[GameManager.instance.waveContainerIndex].allowedEnemies[enemyIndex], hit.point + GameManager.instance.spawnOffset, transform.rotation);
            if (spawn.GetComponent<Enemy>() != null)
            {
                spawn.GetComponent<Enemy>().target = GameManager.instance.playerRef.gameObject;
            }
        }

    }
}
