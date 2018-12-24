using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameModeBasis : MonoBehaviour {

    protected ParticipantManager pm;
    protected ScoreboardSystem ss;
    protected WaitForSecondsRealtime gameTime;
    protected WaitForSecondsRealtime startupTime;

    public abstract bool SetGame(int gameTimeInSeconds, int teamSize, bool withBots);
    public abstract void startGame();
    public abstract void endGame();

}
