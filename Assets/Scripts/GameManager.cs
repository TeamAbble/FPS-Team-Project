using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Player playerRef;
    [Tooltip("The maximum number of enemies that can be alive at any point")]
    public int MaxEnemiesAllowed; //The remaining number of enemies to be spawned
    [Tooltip("The number of enemies remaining in this wave")]
    public int enemiesRemaining;//The remaining number of enemies that must be killed to complete the wave
    public float spawnRate = 3;
    [Tooltip("The number of enemies currently alive")]
    public int enemiesAlive = 0;//The Number of Enemies Currently Alive
    
    public float spawnTimer=0;
    public static GameManager instance;
    int score = 0;
    public Vector3 spawnOffset;
    public GameObject pauseCanvas;
    public bool paused;
    public GameObject respawnScreen;
    public Vector3 spawnPosition;
    [SerializeField]
    List<EnemySpawner> spawners;
    public TextMeshProUGUI waveInfoDisplay;
    [System.Serializable]public class Wave
    {
        public int waveNum;//The Wave Number
        public int EnemiesPerWave;//The Enemies to be spawned in this wave
        public float nextWaveDelay = 25;
        public AnimationCurve enemiesPerWaveRamp;
    }
    public Wave[] waves;
    public int waveContainerIndex;
    public int currentWave;
    public bool waveInProgress;
    [Header("Weapon Switching")]
    public GameObject weaponWheel;
    public float weaponWheelTimeMutliplier;
    /// <summary>
    /// The fixed timestep 
    /// </summary>
    float weaponWheelTimestep;
    public float defaultFixedTimestep = 0.02f;
    public bool weaponWheelOpen;
    public void UseWeaponWheel(bool opening)
    {
        if (opening)
        {
            weaponWheelTimestep = defaultFixedTimestep * weaponWheelTimeMutliplier;
            Time.timeScale = weaponWheelTimeMutliplier;
            Time.fixedDeltaTime = weaponWheelTimestep;
            weaponWheel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Time.timeScale = 1;
            Time.fixedDeltaTime = defaultFixedTimestep;
            weaponWheel.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        weaponWheelOpen = opening;
    }
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
        StartCoroutine(WaveDelay());

        spawners.AddRange(FindObjectsOfType<EnemySpawner>(true));
    }


    // Update is called once per frame
    private void FixedUpdate()
    {
        if (waveInProgress)
        {
            if (enemiesAlive < MaxEnemiesAllowed && enemiesRemaining > 0)
            {
                spawnTimer += Time.fixedDeltaTime;
                if (spawnTimer >= spawnRate)
                {
                    FindSpawner();
                }
            }
            else
            {
                spawnTimer = 0;
            }
        }
    }
    public void FindSpawner()
    {
        if (playerRef == null || spawners.Count == 0)
            return;
        spawnTimer = 0;
        enemiesAlive++;
        enemiesRemaining--;
        int spawnerIndex = Random.Range(0, spawners.Count);
        spawners[spawnerIndex].Spawn();
        SetEnemyDisplay();
    }
    void SetEnemyDisplay()
    {
        waveInfoDisplay.text = $"Enemies Left This Wave\n{enemiesRemaining}\nEnemies Still Working\n{enemiesAlive}";
    }
    public void EnemyDeath()
    {
        score++;
        //The number of enemies left decrements when an enemy dies
        enemiesAlive--;
        SetEnemyDisplay();
        if (enemiesRemaining == 0 && enemiesAlive == 0)
        {
            EndWave();
        }
    }
    void EndWave()
    {
        StartCoroutine(WaveDelay());
    }
    IEnumerator WaveDelay()
    {
        waveInProgress = false;
        float time = waves[waveContainerIndex].nextWaveDelay;
        while (time >= 0)
        {
            waveInfoDisplay.text = $"Break Time: {time:0}";
            time -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        WaveStart();
        yield break;
    }



    private void OnGUI()
    {
        GUI.skin.textField.fontSize = 40;

        GUILayout.TextField($"Score : {score}", GUILayout.Width(Screen.width / 10), GUILayout.Height(Screen.height / 20));
        GUILayout.TextField($"Health : {playerRef.Health}", GUILayout.Width(Screen.width / 8), GUILayout.Height(Screen.height / 20));
    }

    public void WaveStart()
    {
        currentWave++;
        for (int i = 0; i < waves.Length; i++)
        {
            if(currentWave >= waves[i].waveNum)
            {
                waveContainerIndex = i;
            }
        }

        if(waveContainerIndex < waves.Length - 1)
        {
            float ilerp = Mathf.InverseLerp(waves[waveContainerIndex].waveNum, waves[waveContainerIndex +1].waveNum, currentWave);
            enemiesRemaining = Mathf.CeilToInt(Mathf.Lerp(waves[waveContainerIndex].EnemiesPerWave, waves[waveContainerIndex + 1].EnemiesPerWave, waves[waveContainerIndex].enemiesPerWaveRamp.Evaluate(ilerp)));
        }
        else
        {
            enemiesRemaining = waves[waveContainerIndex].EnemiesPerWave;
        }
        SetEnemyDisplay();
        waveInProgress = true;
    }
}
