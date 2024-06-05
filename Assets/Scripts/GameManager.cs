using Cinemachine;
using Eflatun.SceneReference;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public int score = 0;
    public Vector3 spawnOffset;
    public GameObject pauseCanvas;
    public bool paused;
    public GameObject respawnScreen;
    public Vector3 spawnPosition;
    [SerializeField]
    List<EnemySpawner> spawners;
    public TextMeshProUGUI waveInfoDisplay;
    public TextMeshProUGUI breakTimeText, ammoDisplayText;
    public TextMeshProUGUI scoreText;
    public Slider healthbar;
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
    public Volume damageVolume;
    public Vector2 lookSpeed;
    public List<GameObject> unownedWeapons;
    public List<GameObject> defaultWeapons = new();
    public int weaponPrintCost;
    public int areaUnlockCost;
    public GameObject[] playerDependantObjects;
    public float maxSpawnDistance;
    public float minSpawnDistance;

    public TextMeshProUGUI interactText;
    public GameObject interactTextBG;
    public Slider dodgeBar;
    public List<string> heldKeysIDs;

    public Transform loadingScreenRoot;
    GameObject currentLoadingScreen;
    public CanvasGroup lsGroup;
    public List<GameObject> loadingScreens;
    public SceneReference gameScene;
    public SceneReference menuScene;
    public float loadScreenSpeed;
    bool loading;
    public void UseWeaponWheel(bool opening)
    {
        //If the player is dead, we don't want to allow the player to open the weapon wheel.
        //I don't think there's any real adverse effects of letting the player do this, because it hasn't been tried yet
        //But its stupid and pointless to let the player do this when the weapon wheel is hidden by the menus anyway :p

        if (!playerRef.IsAlive)
            return;

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
        //pauses the game
        //I don't much like the timescale method but there's not much else I can think to do other than disabling most components and that might have a wonky effect
        //this is but a humble game so its okay :)
        paused = newPause;
        Time.timeScale = paused ? 0 : 1;
        pauseCanvas.SetActive(paused);
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
        AudioListener.pause = paused;
    }
    public void Respawn()
    {


    }
    private void Awake()
    {
        //Initialise singleton if one doesn't already exist, or destroy this object if it does
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        scoreText.text = $"${score}";
        defaultWeapons = new(unownedWeapons);
        interactTextBG.SetActive(false);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //We need to try and find the player when a level is loaded, rather than just doing it on start. If there's no player, we will effectively ignore this scene as a game scene, and treat it as a menu scene.
        //Get the player 
        if (!playerRef)
            playerRef = FindFirstObjectByType<Player>();
        //Check the player again and then return this early if we have no player. If there's no player, this scene is effectively a menu scene, and we can ignore everything below this because we're not doing any gameplay stuff.
        if (!playerRef)
        {
            for (int i = 0; i < playerDependantObjects.Length; i++)
            {
                playerDependantObjects[i].SetActive(false);
            }
            return;
        }

        for (int i = 0; i < playerDependantObjects.Length; i++)
        {
            playerDependantObjects[i].SetActive(true);
        }

        //Check if the main camera in the scene does or doesn't have a Cinemachine Brain,
        //Allows it to work with the player
        if (!Camera.main.GetComponent<CinemachineBrain>())
        {
            Camera.main.gameObject.AddComponent<CinemachineBrain>();
        }

        //Force the game to be unpaused
        PauseGame(false);
        //Disable this menu
        respawnScreen.SetActive(false);
        //Gather all the spawners
        spawners.Clear();
        spawners.AddRange(FindObjectsOfType<EnemySpawner>(true));
        //Start the first wave delay
        StartCoroutine(WaveDelay());
        interactTextBG.SetActive(false);
        if(DamageRingManager.Instance)
            DamageRingManager.Instance.ClearRings();

        score = 0;
        scoreText.text = $"${score}";
        unownedWeapons = new(defaultWeapons);
        currentWave = 0;
        enemiesAlive = 0;
        enemiesRemaining = 0;
        SetEnemyDisplay();

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (playerRef)
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
            if (playerRef.weaponManager.CurrentWeapon)
            {
                (int max, int current) = playerRef.weaponManager.CurrentWeapon.Ammo;
                ammoDisplayText.text = $"{current}\n/{max}";
            }
        }
        scoreText.text = $"${score}";

    }
    public void FindSpawner()
    {
        if (playerRef == null || spawners.Count == 0)
            return;
        int spawnerIndex = 0;
        // int[] checkedRooms = new int[spawners.Count];
        // int checkedRoomCount = 0;
        // while (spawnerDistance >= 0 && spawnerDistance < maxSpawnDistance)
        // {
        //     spawnerIndex = Random.Range(0, spawners.Count);
        //     spawnerDistance = Vector3.Distance(playerRef.transform.position, spawners[spawnerIndex].transform.position);
        // }
        var spawnersInRange = spawners.FindAll(x => Vector3.Distance(x.transform.position, playerRef.transform.position) <= maxSpawnDistance && Vector3.Distance(x.transform.position, playerRef.transform.position) >= minSpawnDistance);
        spawnerIndex = Random.Range(0, spawnersInRange.Count);
        spawnTimer = 0;
        enemiesAlive++;
        enemiesRemaining--;
        spawnersInRange[spawnerIndex].Spawn();
        SetEnemyDisplay();
    }
    void SetEnemyDisplay()
    {
        waveInfoDisplay.text = WaveStringBuilder();
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
            breakTimeText.text = $"Break Time: {time:0}";
            time -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        WaveStart();
        yield break;
    }

    public void WaveStart()
    {
        currentWave++;
        breakTimeText.text = "";
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

    public string WaveStringBuilder()
    {
        int currentwaveHundreds = Mathf.FloorToInt( currentWave / 100);
        int currentwave = currentWave % 100;
        string printString = $"{currentwaveHundreds:00}:{currentwave:00}";
        return printString;
    }
    public void StartGame()
    {
        StartCoroutine(LoadingScreen(gameScene));
    }
    public void ReturnToMenu()
    {
        StartCoroutine(LoadingScreen(menuScene));
    }
    IEnumerator LoadingScreen(SceneReference targetScene)
    {
        if (!loading)
        {
            loading = true;
            print($"Attempting to load scene {targetScene.Name}");

            float t = 0;
            Time.timeScale = 1;
            lsGroup.alpha = 0;
            currentLoadingScreen = Instantiate(loadingScreens[Random.Range(0, loadingScreens.Count)], loadingScreenRoot);
            currentLoadingScreen.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            yield return null;
            while (t < 1)
            {
                t += Time.unscaledDeltaTime * loadScreenSpeed;
                lsGroup.alpha = t;
                yield return new WaitForEndOfFrame();
            }
            var lsa = SceneManager.LoadSceneAsync(targetScene.Name);
            while (!lsa.isDone)
            {
                yield return new WaitForFixedUpdate();
            }
            lsa.allowSceneActivation = true;
            while (t > 0)
            {
                t -= Time.unscaledDeltaTime * loadScreenSpeed;
                lsGroup.alpha = t;
                yield return new WaitForEndOfFrame();
            }
            Destroy(currentLoadingScreen);
            print($"Loaded scene {targetScene.Name}");
        }
        yield break;
    }
}
