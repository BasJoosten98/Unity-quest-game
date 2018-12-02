﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamDeathMatch : MonoBehaviour {

    private ParticipantManager pm;
    private ScoreboardSystem ss;
    private WaitForSecondsRealtime gameTime;
    private WaitForSecondsRealtime startupTime;
    
	void Awake () {
        if(!SetGame(240, 2, false)) { Debug.Log("SetGame FAILED"); }
	}

    private bool SetGame(int gameTimeInSeconds, int teamSize, bool withBots)
    {
        if (this.GetComponent<ParticipantManager>()) { pm = this.GetComponent<ParticipantManager>(); } else { return false; }
        if (this.GetComponent<ScoreboardSystem>()) { ss = this.GetComponent<ScoreboardSystem>(); } else { return false; }

        if(gameTimeInSeconds >= 60) { gameTime = new WaitForSecondsRealtime(gameTimeInSeconds); } else { return false; }
        if(teamSize >= 1 && teamSize <= 3) { ss.SetScoreboardSystem(true, teamSize); } else { return false; }
        pm.SetParticipantManager(withBots, true);
        StartCoroutine("Timer");
        return true;
    }
    private IEnumerator Timer()
    {
        startupTime = new WaitForSecondsRealtime(2f);
        yield return startupTime; //let everyone join the game
        startGame();
        yield return gameTime; //actual game time
        endGame();
    }
    private void startGame()
    {
        pm.StartGame();
        Debug.Log("Game has started");
    }
    private void endGame()
    {
        pm.AllowMovement(false);
        pm.AllowGrabbingArrowsAndShooting(false);
        Debug.Log("Game had ended");
        ss = null;
        pm = null;
    }
}
