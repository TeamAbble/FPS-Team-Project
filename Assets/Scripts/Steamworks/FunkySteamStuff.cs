using HeathenEngineering.SteamworksIntegration;
using HeathenEngineering.SteamworksIntegration.API;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunkySteamStuff : MonoBehaviour
{

    public static FunkySteamStuff Instance { get; private set; }

    [SerializeField] SteamSettings steamSettings;

    private void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SteamAPI.Init();
        steamSettings.Initialize();
    }
}
