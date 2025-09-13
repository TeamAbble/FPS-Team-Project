using Cinemachine;
using Eflatun.SceneReference;
using InputSystemActionPrompts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
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
    public CanvasGroup pauseCanvas;
    public bool paused;
    public CanvasGroup respawnScreen;
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
        public float healthAtWave = 10;
        public AnimationCurve healthRamp;
        public List<GameObject> allowedEnemies;
    }
    public Wave[] waves;
    public static float currentEnemyHealth;
    public int waveContainerIndex;
    public int currentWave;
    public bool waveInProgress;
    [Header("Weapon Switching")]
    public GameObject weaponWheel;
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
    public InputSystemActionPrompts.PromptText interactPromptText;
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
    public int baseWeaponCost, baseAreaCost;
    public float areaCostMultiplier;

    public GameObject quitPrompt;

    public Selectable firstSelected;

    public bool onlyAllowUIToggleInDebug;

    public GameObject frameCounter;
    public TMP_Text frameCounterText;
    public bool frameCounterTicking;

    public AudioSource breaktimeBuzzer;


    public static float ch_damageMultEnemy = 1, ch_damageMultPlayer = 1, ch_spawnRateMult = 1;
    public static bool ch_playerNoHurt = false, ch_playerNoAmmo = false, ch_playerDodgeNoCool = false, ch_babyNoMoney;


    public static bool cheatsEnabled;
    public CanvasGroup debugUI;
    public CanvasGroup gameUI;
    public delegate void DoorCheat();
    public delegate void WaveCheat(int wavesToSkip);
    public static WaveCheat onWaveSkipped;
    public static DoorCheat onDoorsUnlocked;
    public TMP_Text waveSkipCounter;
    public Button waveSkipButton, unlockDoorButton;
    public Slider enemyHurtSlide, playerHurtSlide, spawnRateSlide;
    public Toggle invTog, noAmmoTog, dodgeCoolTog, noMoneyTog;
    public TMP_Text enemyDmgMultDisplay, playerDmgMultDisplay, spawnRateMultDisplay, waveSkipDisplay;
    public int wavesToSkip = 1;

    public PromptIcon[] promptIconsToChange;

    public AudioSource calmMusic, battleMusic;
    public float musicFadeTime;
    public AudioClip calmClip, battleClip;

    public void SetFrameCounter(bool value)
    {
        frameCounter.SetActive(value);
        if (!frameCounterTicking)
        {
            StartCoroutine(FrameCounterTick());
        }
    }
    public IEnumerator FrameCounterTick()
    {
        frameCounterTicking = true;
        while (frameCounter.activeInHierarchy)
        {
            frameCounterText.text = $"FPS:{(1/Time.smoothDeltaTime):00.0}";
            yield return new WaitForSeconds(0.25f);
        }
        frameCounterTicking = false;
        yield break;
    }
    public void PauseGame(bool newPause)
    {
        //pauses the game
        //I don't much like the timescale method but there's not much else I can think to do other than disabling most components and that might have a wonky effect
        //this is but a humble game so its okay :)
        paused = newPause;
        Time.timeScale = paused ? 0 : 1;
        pauseCanvas.SetGroupActive(paused);
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
        AudioListener.pause = paused;
        if (!paused)
            quitPrompt.SetActive(false);
    }
    public void Respawn()
    {
        StartCoroutine(LoadingScreen(gameScene));
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

        
        SetFrameCounter(SettingsController.settings.showFrames);
        InitCheats();
    }
    public void FadeMusic(bool calm)
    {
        StartCoroutine(_FadeMusic(calm));
    }
    IEnumerator _FadeMusic(bool calm)
    {
        float t = 0;
        float inc = (1 / musicFadeTime) * Time.fixedDeltaTime;
        while (t < 1)
        {
            if (calm)
            {
                calmMusic.volume = t;
                battleMusic.volume = 1 - t;
            }
            else
            {
                calmMusic.volume = 1 - t;
                battleMusic.volume = t;
            }
            t += inc;
            yield return new WaitForFixedUpdate();
        }
    }

    void InitCheats()
    {
        cheatsEnabled = false;
        unlockDoorButton.onClick.AddListener(UnlockAllDoors);
        waveSkipButton.onClick.AddListener(SkipWaves);
        invTog.onValueChanged.AddListener(GodMode);
        dodgeCoolTog.onValueChanged.AddListener(InfDodge);
        noAmmoTog.onValueChanged.AddListener(InfAmmo);
        noMoneyTog.onValueChanged.AddListener(InfCash);
    }
    /// <summary>
    /// Multiplies damage done by enemies
    /// </summary>
    public void EnemyDamage(float value)
    {
        ch_damageMultEnemy = value;
        enemyDmgMultDisplay.text = $"x{ch_damageMultEnemy}";
    }
    public void SpawnMultiply(float value)
    {
        ch_spawnRateMult = value;
        spawnRateMultDisplay.text = $"x{ch_spawnRateMult}";
    }
    /// <summary>
    /// Multiplies damage done by player
    /// </summary>
    public void PlayerDamage(float value)
    {
        ch_damageMultPlayer = value;
        playerDmgMultDisplay.text = $"x{ch_damageMultPlayer}";
    }
    public void SetWavesSkip(float value)
    {
        wavesToSkip = Mathf.CeilToInt(value);
        waveSkipDisplay.text = wavesToSkip.ToString();
    }
    public void ToggleDebugUI(bool state)
    {
        if (!cheatsEnabled)
            state = false;

        //If we've paused the game and tried to open this menu
        if (paused && !debugUI.isActiveAndEnabled)
            state = false;

        paused = state;
        Time.timeScale = state ? 0 : 1;
        debugUI.SetGroupActive(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }
    public void SetSkipWaves(int value)
    {
        wavesToSkip = value;
    }
    public void SkipWaves()
    {
        enemiesRemaining = 0;
        onWaveSkipped?.Invoke(wavesToSkip);
        currentWave += wavesToSkip;
        EndWave();
    }
    public void UnlockAllDoors()
    {
        onDoorsUnlocked?.Invoke();
    }
    public void ToggleUI()
    {
        if (gameUI != null)
        {
            gameUI.SetGroupActive(!gameUI.gameObject.activeSelf);
        }
    }

    public void InfCash(bool value)
    {
        ch_babyNoMoney = value;
    }
    public void InfDodge(bool value)
    {
        ch_playerDodgeNoCool = value;
    }
    public void GodMode(bool value)
    {
        ch_playerNoHurt = value;
    }
    public void InfAmmo(bool value)
    {
        ch_playerNoAmmo = value;
    }


    public void UpdatePromptIcons()
    {
        for (int i = 0; i < promptIconsToChange.Length; i++)
        {
            promptIconsToChange[i].Refresh();
        }
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
        respawnScreen.SetGroupActive(false);
        //Gather all the spawners
        spawners.Clear();
        spawners.AddRange(FindObjectsOfType<EnemySpawner>(true));
        //Start the first wave delay
        StartCoroutine(WaveDelay());
        interactTextBG.SetActive(false);
        if(DamageRingManager.Instance)
            DamageRingManager.Instance.ClearRings();
        //Force recompile :|
        //score = Debug.isDebugBuild ? 1000 : 0;
        score = 0;
        scoreText.text = $"${score}";
        unownedWeapons = new(defaultWeapons);
        currentWave = 0;
        enemiesAlive = 0;
        enemiesRemaining = 0;
        SetEnemyDisplay();
        areaUnlockCost = baseAreaCost;
        weaponPrintCost = baseWeaponCost;
        waveInProgress = false;
        waveContainerIndex = 0;

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
                    spawnTimer += Time.fixedDeltaTime * (cheatsEnabled ? ch_spawnRateMult : 1);
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
                var w = playerRef.weaponManager.CurrentWeapon;
                var (max, current, reserve) = w.Ammo;
                if (w.meleeWeapon)
                    ammoDisplayText.text = $"{w.displayName}\nMelee Weapon";
                else
                    ammoDisplayText.text = $"{w.displayName}\n{current}/{max}\nReserve:{reserve}";
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
        if(spawnersInRange.Count > 0)
        {
            spawnerIndex = Random.Range(0, spawnersInRange.Count);
            enemiesAlive++;
            enemiesRemaining--;
            spawnersInRange[spawnerIndex].Spawn();
            SetEnemyDisplay();
        }
        spawnTimer = 0;
    }
    void SetEnemyDisplay()
    {
        waveInfoDisplay.text = WaveStringBuilder();
    }
    public void EnemyDeath(int score, bool ignorecash = false)
    {
        if (!ignorecash)
        {
            this.score+=score;
            StatsManager.Instance.UpdateCash(score);
        }
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
        StatsManager.Instance.UpdateWaves(currentWave);
        //Can we make it recompile pls?
        waveInProgress = false;
        float time = waves[waveContainerIndex].nextWaveDelay;
        breakTimeText.gameObject.SetActive(true);
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
        breakTimeText.gameObject.SetActive(false);
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
            currentEnemyHealth = Mathf.Lerp(waves[waveContainerIndex].healthAtWave, waves[waveContainerIndex + 1].healthAtWave, waves[waveContainerIndex].healthRamp.Evaluate(ilerp));
        }
        else
        {
            currentEnemyHealth = waves[waveContainerIndex].healthAtWave;
            enemiesRemaining = waves[waveContainerIndex].EnemiesPerWave;
        }
        if(breaktimeBuzzer != null)
        {
            breaktimeBuzzer.Play();
        }
        SetEnemyDisplay();
        waveInProgress = true;
    }

    public string WaveStringBuilder()
    {
        return $"{currentWave}";
    }
    public void StartGame()
    {
        StartCoroutine(LoadingScreen(gameScene));
        StatsManager.Instance.StartNewGame();
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
            yield return new WaitForSeconds(1);
            var lsa = SceneManager.LoadSceneAsync(targetScene.Name);
            while (!lsa.isDone)
            {
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(1);
            lsa.allowSceneActivation = true;
            while (t > 0)
            {
                t -= Time.unscaledDeltaTime * loadScreenSpeed;
                lsGroup.alpha = t;
                yield return new WaitForEndOfFrame();
            }
            Destroy(currentLoadingScreen);
            print($"Loaded scene {targetScene.Name}");
            loading = false;
        }
        yield break;
    }
}
