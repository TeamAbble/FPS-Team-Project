using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject[] pref;
    public Transform[] pos;
    public Player playerRef;
    public int MaxEnemiesAllowed = 15; //The remaining number of enemies to be spawned
    public int enemiesRemaining;//The remaining number of enemies that must be killed to complete the wave
    public float spawnRate = 3;
    public int enemiesAlive = 0;//The Number of Enemies Currently Alive
    
    int remaining;
    float timer=0;
    public static GameManager instance;
    int score = 0;
    public Vector3 spawnOffset;
    public GameObject pauseCanvas;
    public bool paused;
    public GameObject respawnScreen;
    public Vector3 spawnPosition;

    [System.Serializable]public struct Wave
    {
        public int waveNum;//The Wave Number
        public int EnemiesPerWave;//The Enemies to be spawned in this wave
    }
    public Wave[] waves;

    public void PauseGame(bool newPause)
    {
        paused = newPause;
        Time.timeScale = paused ? 0 : 1;
        pauseCanvas.SetActive(paused);
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }
    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        if (!Camera.main.GetComponent<CinemachineBrain>())
        {
            Camera.main.gameObject.AddComponent<CinemachineBrain>();
        }
        if (!playerRef)
            playerRef = FindFirstObjectByType<Player>();
        PauseGame(false);
        respawnScreen.SetActive(false);
    }


    // Update is called once per frame
    private void FixedUpdate()
    {
        
        timer += Time.fixedDeltaTime;
        if (timer >= spawnRate && MaxEnemiesAllowed > enemiesAlive)
        {

            Spawn(pref[Random.Range(0, pref.Length - 1)], pos[Random.Range(0, pos.Length - 1)]);
            timer = 0;
            enemiesAlive++;

        }
        
    }
    public void Spawn(GameObject prefab,Transform pos)
    {

        if(Physics.Raycast(pos.position, Vector3.down, out RaycastHit hit))
        {
            GameObject spawn = Instantiate(prefab, hit.point + spawnOffset, pos.rotation);
            if (spawn.GetComponent<Enemy>() != null)
            {
                spawn.GetComponent<Enemy>().target = playerRef.gameObject;
            }
        }

    }
    public void EnemyDeath()
    {
        MaxEnemiesAllowed++;
        score++;
        remaining--;
        if (remaining == 0)
        {
            WaveStart();
        }
    }




    private void OnGUI()
    {
        GUI.skin.textField.fontSize = 40;

        GUILayout.TextField($"Score : {score}", GUILayout.Width(Screen.width / 10), GUILayout.Height(Screen.height / 20));
        GUILayout.TextField($"Health : {playerRef.Health}", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 20));
    }

    public void WaveStart()
    {
    }
}
