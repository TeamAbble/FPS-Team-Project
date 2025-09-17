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

            StatsAndAchievements.Client.GetStat("total_cash", out totalCash);
            StatsAndAchievements.Client.GetStat("total_waves", out totalWaves);
            StatsAndAchievements.Client.GetStat("total_elims", out totalElims);
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
        totalElims += elimsToAdd;
    }
    public void UpdateCash(int cashToAdd)
    {
        if (cheatsWereEverUsed)
            return;
        Cash += cashToAdd;
        totalCash += cashToAdd;
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
        totalWaves += newWavesCount - waves;
    }
    public static StatsManager Instance;
    public int elims, cash, waves;
    public static int Elims { get  { return Instance.elims; } set { Instance.elims = value; } }
    public static int Cash { get { return Instance.cash; } set { Instance.cash = value; } }
    public static int Waves { get { return Instance.waves; } set { Instance.waves = value; } }
    public int totalElims, totalCash, totalWaves;
    public static int heals = 0;
    public IEnumerator StoreStatsOnServer()
    {
        while (true)
        {
            yield return new WaitForSeconds(60);
            StoreStats();
        }
    }
    
    private void OnApplicationQuit()
    {
        StoreStats();
    }
    void StoreStats()
    {
        StatsAndAchievements.Client.SetStat("cash", Cash);
        StatsAndAchievements.Client.SetStat("waves", Waves);
        StatsAndAchievements.Client.SetStat("elims", Elims);
        StatsAndAchievements.Client.SetStat("heals", heals);
        StatsAndAchievements.Client.SetStat("total_elims", totalElims);
        StatsAndAchievements.Client.SetStat("total_waves", totalWaves);
        StatsAndAchievements.Client.SetStat("total_cash", totalCash);

        StatsAndAchievements.Client.StoreStats();
    }

}
