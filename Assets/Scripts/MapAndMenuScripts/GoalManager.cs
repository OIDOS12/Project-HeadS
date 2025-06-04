using UnityEngine;
using Mirror;
using System;
using System.Collections;

/// <summary>
/// Manages post-goal game events, such as updating UI, resetting player and ball positions.
/// </summary>
public class GoalManager : NetworkBehaviour
{
    [SerializeField] private NetworkGameUI gameUI;
    [SerializeField] private BallAndPlayerMethods ballAndPlayerMethods;
    [SerializeField] private AudioClip refereeWhistle;
    private string scoreboardText;
    [SerializeField] private string goalText = "Goal!";
    [SerializeField] private int goalLimit = 10;
    private const float BallResetDelaySeconds = 2f;

    /// <summary>
    /// Called at the start of the game. Plays initial whistle.
    /// </summary>
    void Start()
    {
        SoundFXManager.Instance.PlaySoundFX(refereeWhistle, transform);
    }

    /// <summary>
    /// Initiates the goal handling sequence.
    /// </summary>
    public void HandleGoal(string scoringPlayer, Action onComplete)
    {
        StartCoroutine(GoalSequence(scoringPlayer, onComplete));
    }

    /// <summary>
    /// Coroutine that performs UI updates and resets after a goal.
    /// </summary>
    private IEnumerator GoalSequence(string scoringPlayer, Action onComplete)
    {
        if (!isServer) yield break;
        gameUI.RpcUpdateScoreboardText(goalText);

        yield return new WaitForSeconds(BallResetDelaySeconds);

        ResetPlayerPositions();
        ResetBallPosition(scoringPlayer);

        scoreboardText = ballAndPlayerMethods.Player2Goals + "|" + ballAndPlayerMethods.Player1Goals;
        gameUI.RpcUpdateScoreboardText(scoreboardText);

        if (ballAndPlayerMethods.Player1Goals >= goalLimit || ballAndPlayerMethods.Player2Goals >= goalLimit)
        {
            gameUI.RpcShowWinner(ballAndPlayerMethods.Player1Goals >= goalLimit ? "Player 1" : "Player 2");
            yield return new WaitForSeconds(5f);
            QuitMatch();
        }

        RpcPlayReffereeWhistle();

        onComplete?.Invoke();
    }

    /// <summary>
    /// Plays the referee whistle sound effect.
    /// </summary>
    [ClientRpc]
    void RpcPlayReffereeWhistle()
    {
        SoundFXManager.Instance.PlaySoundFX(refereeWhistle, transform);
    }

    /// <summary>
    /// Resets player positions.
    /// </summary>
    [Server]
    private void ResetPlayerPositions()
    {
        ballAndPlayerMethods.SetStandartPosition();
    }

    /// <summary>
    /// Resets the ball position.
    /// </summary>
    [Server]
    private void ResetBallPosition(string scoringPlayer)
    {
        ballAndPlayerMethods.ResetBall(scoringPlayer);
    }

    /// <summary>
    /// Quits the match and returns to the menu.
    /// This is called on the server and executed on all clients.
    /// </summary>
    [ClientRpc]
    private void QuitMatch()
    {
        gameUI.QuitButton();
    }
}
