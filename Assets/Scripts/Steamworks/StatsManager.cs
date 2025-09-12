using HeathenEngineering.SteamworksIntegration.API;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager
{
    public static bool cheatsWereEverUsed;
    public static void Initialise()
    {
        if(Instance == null)
        {
            Instance = new StatsManager();
            Instance.QueryStats();
        }
    }
    void QueryStats()
    {
        StatsAndAchievements.Client.GetStat("elims", out elims);
        StatsAndAchievements.Client.GetStat("cash", out cash);
        StatsAndAchievements.Client.GetStat("waves", out waves);

    }
    public void UpdateElims(int elimsToAdd)
    {
        if (cheatsWereEverUsed)
            return;
        elims += elimsToAdd;
        StatsAndAchievements.Client.SetStat("elims", elims);
    }
    public void UpdateCash(int cashToAdd)
    {
        if (cheatsWereEverUsed)
            return;
        cash += cashToAdd;
        StatsAndAchievements.Client.SetStat("cash", cash);
    }
    /// <summary>
    /// UpdateWaves works slightly differently. We want to directly set the number of waves instead.
    /// </summary>
    /// <param name="newWavesCount"></param>
    public void UpdateWaves(int newWavesCount)
    {
        if (cheatsWereEverUsed)
            return;
        waves = newWavesCount;
        StatsAndAchievements.Client.SetStat("waves", waves);
    }
    public static StatsManager Instance;
    public static int elims;
    public static int cash;
    public static int waves;
}
