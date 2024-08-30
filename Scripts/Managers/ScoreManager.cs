using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : StaticInstance<ScoreManager>
{
    internal void CalculateTurnScore(Player winner, List<Player> players)
    {
        var score = 0;

        foreach (var card in GameManager.Instance.GetPlacedCards())
        {
            if (card.GetSuit() == Suit.Hearts)
            {
                score++;
            }
            else if (card.GetSuit() == Suit.Spades && card.GetRank() == 12)
            {
                score += 12;
            }
        }
        winner.IncreaseTurnScore(score);
        UpdateTurnScoreText(players);
    }

    internal void CalculateRoundScore(List<Player> players)
    {
        var maxScore = 25;
        Player hasMaxScore = null;
        foreach (var player in players)
        {
            if (player.GetTurnScore() == maxScore)
            {
                hasMaxScore = player;
            }
        }

        if (hasMaxScore != null)
        {
            foreach (var player in players)
            {
                if (hasMaxScore != null && hasMaxScore != player)
                {
                    player.IncreaseTotalScore(25);
                }
            }
        }
        else
        {
            foreach (var player in players)
            {
                player.IncreaseTotalScore(player.GetTurnScore());
            }
        }
        UpdateTotalScoreText(players);
    }

    private void UpdateTotalScoreText(List<Player> players)
    {
        foreach (var player in players)
        {
            player.SetTotalScoreText(player.GetName() + ": " + player.GetTotalScore());
        }
    }

    private void UpdateTurnScoreText(List<Player> players)
    {
        foreach (var player in players)
        {
            player.SetTurnScoreText(player.GetName() + ": " + player.GetTurnScore());
        }
    }

    internal void ResetTurnScores(List<Player> players)
    {
        foreach (var player in players)
        {
            player.ResetTurnScore();
        }
        UpdateTurnScoreText(players);
    }
}
