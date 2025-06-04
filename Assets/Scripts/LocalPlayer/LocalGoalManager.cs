using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Manages post-goal game events, such as updating UI, resetting player and ball positions.
/// </summary>
public class LocalGoalManager : MonoBehaviour
{
    [SerializeField] private LocalGameUI gameUI;
    [SerializeField] private AudioClip refereeWhistle;
    [SerializeField] private LocalBallAndPlayerMethods localBallAndPlayerMethods;
    private string scoreboardText;
    [SerializeField] private string goalText = "Goal!";
    [SerializeField] private int goalLimit = 10;
    private const float BallResetDelaySeconds = 2f;

    /// <summary>
    /// Called at the start of the game. Plays initial whistle.
    /// </summary>
    void Start()
    {
        PlayReffereeWhistle();
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
        gameUI.UpdateScoreboardText(goalText);

        yield return new WaitForSeconds(BallResetDelaySeconds);

        ResetPlayerPositions();
        ResetBallPosition(scoringPlayer);
        scoreboardText = localBallAndPlayerMethods.Player2Goals + "|" + localBallAndPlayerMethods.Player1Goals;
        gameUI.UpdateScoreboardText(scoreboardText);
        if (localBallAndPlayerMethods.Player1Goals >= goalLimit || localBallAndPlayerMethods.Player2Goals >= goalLimit)
        {
            gameUI.ShowWinner(localBallAndPlayerMethods.Player1Goals >= goalLimit ? "Player 1" : "Player 2");
            yield return new WaitForSeconds(5f);
            gameUI.QuitButton();
        }
        PlayReffereeWhistle();

        onComplete?.Invoke();
    }

    /// <summary>
    /// Plays the referee whistle sound effect.
    /// </summary>
    void PlayReffereeWhistle()
    {
        SoundFXManager.Instance.PlaySoundFX(refereeWhistle, transform);
    }

    /// <summary>
    /// Resets player positions.
    /// </summary>
    private void ResetPlayerPositions()
    {
        localBallAndPlayerMethods.SetStandartPosition();
    }
    /// <summary>
    /// Resets the ball position.
    /// </summary>
    private void ResetBallPosition(string scoringPlayer)
    {
        localBallAndPlayerMethods.ResetBall(scoringPlayer);
    }
}
