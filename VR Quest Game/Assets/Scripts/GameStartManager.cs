using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GameStartManager : NetworkBehaviour {

    private static GameMode mode;
    private static GameModeBasis modeScript;
    private static GameStartManager gms;

    public static GameStartManager GSM { get { return gms; } }

    private void Awake()
    {
        gms = this;
    }

    [Server]
    public bool SetGame(int gameTimeInSeconds, int teamSize, bool withBots, GameMode gMode)
    {
        removeGameMode();
        return addGameModeAndSetGame( gameTimeInSeconds,  teamSize,  withBots, gMode);
    }
    [Server]
    private void removeGameMode()
    {
        if(mode != GameMode.None) //remove current game mode
        {
            if(modeScript != null)
            {
                Destroy(modeScript);
            }
            else
            {
                Debug.LogWarning("Mode has been set, but there is no modeScript");
            }
        }
    }
    [Server]
    private bool addGameModeAndSetGame(int gameTimeInSeconds, int teamSize, bool withBots, GameMode gMode)
    {
        if(gMode == GameMode.None)
        {
            Debug.LogWarning("Game mode none is not allowed");
            return false;
        }
        else if(gMode == GameMode.Team_deathmatch)
        {
            modeScript = this.gameObject.AddComponent<TeamDeathMatch>();
            mode = GameMode.Team_deathmatch;
        }
        else
        {
            Debug.LogWarning("Unknown game mode");
            return false;
        }

        return modeScript.SetGame(gameTimeInSeconds, teamSize, withBots);
    }


}
