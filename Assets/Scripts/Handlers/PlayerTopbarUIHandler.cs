using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTopbarUIHandler : MonoBehaviour
{
    public static PlayerTopbarUIHandler Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    
    [SerializeField] private GameObject _topBarRoot;

    [Header("Round timer")]
    [SerializeField] private TextMeshProUGUI _roundTimeLeftText;

    [SerializeField] private Image _roundTimeBarFill;
    
    [Header("Kill Feed")]
    [SerializeField] private Transform _killFeedContentRoot;
    [SerializeField] private TextMeshProUGUI _killFeedText;
    private Queue<string> _killFeedMessages = new Queue<string>();
    private const int MaxKillFeedMessages = 5;
    
    [Header("Scores")]
    public TextMeshProUGUI _playerScoresText;
    
    
    [Header("Player Died Message")]
    [SerializeField] private GameObject _playerDiedMessageRoot;
    [SerializeField] private TextMeshProUGUI _playerDiedMessageText;
    
    
    public void SetRoundTimeLeft(int timeLeft, float timeFraction)
    {
        _roundTimeBarFill.fillAmount = timeFraction;
        _roundTimeLeftText.text = $"{timeLeft / 60:D2}:{timeLeft % 60:D2}";
    }

    public void AddKillToFeed(string killerName, string victimName)
    {
        string message = $"{killerName} → {victimName}";
        _killFeedMessages.Enqueue(message);
        if (_killFeedMessages.Count > MaxKillFeedMessages)
        {
            _killFeedMessages.Dequeue();
        }

        _killFeedText.text = string.Join("\n", _killFeedMessages);
    }
    
    public void ShowPlayerDiedMessage(string message)
    {
        _playerDiedMessageText.text = message;
        _playerDiedMessageRoot.SetActive(true);
    }
    
    public void HidePlayerDiedMessage()
    {
        _playerDiedMessageRoot.SetActive(false);
    }
}
