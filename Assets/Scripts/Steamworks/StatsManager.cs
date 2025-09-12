using HeathenEngineering.SteamworksIntegration.API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static bool cheatsWereEverUsed;
    void Awake()
    {
        Initialise();
    }
    public void Initialise()
    {
        if(Instance == null)
        {
            Instance = this;
            StartCoroutine(StoreStatsOnServer());
        }
    }
    public void StartNewGame()
    {
        elims = cash = waves = 0;
    }
    public void UpdateElims(int elimsToAdd)
    {
        if (cheatsWereEverUsed)
            return;
        Elims += elimsToAdd;
        StatsAndAchievements.Client.SetStat("Elims", Elims);
    }
    public void UpdateCash(int cashToAdd)
    {
        if (cheatsWereEverUsed)
            return;
        Cash += cashToAdd;
        StatsAndAchievements.Client.SetStat("Cash", Cash);
    }
    /// <summary>
    /// UpdateWaves works slightly differently. We want to directly set the number of waves instead.
    /// </summary>
    /// <param name="newWavesCount"></param>
    public void UpdateWaves(int newWavesCount)
    {
        if (cheatsWereEverUsed)
            return;
        Waves = newWavesCount;
        StatsAndAchievements.Client.SetStat("Waves", Waves);
    }
    public static StatsManager Instance;
    public int elims, cash, waves;
    public static int Elims { get  { return Instance.elims; } set { Instance.elims = value; } }
    public static int Cash { get { return Instance.cash; } set { Instance.cash = value; } }
    public static int Waves { get { return Instance.waves; } set { Instance.waves = value; } }

    public IEnumerator StoreStatsOnServer()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            StatsAndAchievements.Client.StoreStats();
        }
    }
    private void OnApplicationQuit()
    {
        StatsAndAchievements.Client.StoreStats();
    }

}
