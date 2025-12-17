using System.Collections.Generic;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Models.Boons;

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
	[SerializeField] private Button pickButton;
	[SerializeField] private GameObject cardPrefab;
	[SerializeField] private Transform cardParent;
	
	private Card _selectedCard;
	
	private PlayerStatHandler _stats;

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
			cardComponent.Initialize(boon.CardName, boon.Description, boon.Icon, boon.BoonId);
		}
	}

	private void SelectBoon(int boonId)
	{
		// wait out timer for other players 
		
		_stats.CmdSelectBoon(boonId);
		
		boonPanel.SetActive(false);
		PlayerUIHandler.instance.rootPanel.gameObject.SetActive(true);
	}
   
   

	#endregion
}
