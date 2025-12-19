using System;
using System.Collections.Generic;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Models.Boons;
using TMPro;
using Random = UnityEngine.Random;

public class PlayerBoonUIHandler : MonoBehaviour
{
	public static PlayerBoonUIHandler Instance;
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

	[SerializeField] private GameObject boonPanel;
	[SerializeField] private GameObject cardPrefab;
	[SerializeField] private Transform cardParent;
	[SerializeField] private TextMeshProUGUI timerText;
	
	private PlayerStatHandler _stats;
	
	private int _selectedBoonId = 1;
	private GameObject _selectedCardObject;

	public void Initialize(PlayerStatHandler stats)
	{
		_stats = stats;
	}
	
	#region Boons
	
	public void ShowBoons(int[] boonIds)
	{
		CursorHelper.UnlockCursor();
		
		PlayerUIHandler.instance.rootPanel.gameObject.SetActive(false);
		boonPanel.SetActive(true);
		
		for (int i = 0; i < boonIds.Length; i++)
		{
			BoonCardSC boon = BoonDatabase.GetBoonById(boonIds[i]);
			
			GameObject card = Instantiate(cardPrefab, cardParent);
			Card cardComponent = card.GetComponent<Card>();
			cardComponent.Initialize(boon.CardName, boon.Description, boon.Icon, boon.Rarity, boon.BoonId);
		}
		
		_selectedBoonId = boonIds[Random.Range(0, boonIds.Length)];
	}

	// called from GameRoundHandler when the timer runs out
	[Client]
	public void ActivateBoon()
	{
		_stats.CmdSelectBoon(_selectedBoonId);
		
		boonPanel.SetActive(false);
		PlayerUIHandler.instance.rootPanel.gameObject.SetActive(true);
		
		CursorHelper.LockCursor();
	}

	public void SelectBoon(int boonId, GameObject card)
	{
		_selectedBoonId = boonId;
		if (_selectedCardObject != null)
			_selectedCardObject.transform.localScale = Vector3.one;
		_selectedCardObject = card;
	}

	public void UpdateTimer(float newValue)
	{
		timerText .text = "Time Left: " + Mathf.CeilToInt(newValue).ToString();
	}
	
	#endregion

	private void Update()
	{
		if (_selectedCardObject != null)
		{
			// some sin scaling effect
			float scale = 1f + 0.05f * Mathf.Sin(Time.time * 5f);
			_selectedCardObject.transform.localScale = new Vector3(scale, scale, scale);
		}
	}
}
