using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Models.Stats;

public class PlayerUIHandler : MonoBehaviour
{
   public Canvas playerCanvas;

   public TextMeshProUGUI playerName;
   public TextMeshProUGUI playerHealth;
   public Image playerHealthBar;
   public TextMeshProUGUI playerStamina;
   public  Image playerStaminaBar;
   
   public TextMeshProUGUI playerAmmoCounter;
   public Image ReloadProgressBar;

   private PlayerStatHandler _stats;

   public void Initialize(PlayerStatHandler stats)
   {
      playerCanvas.gameObject.SetActive(true);

      _stats = stats;
   }

   private void Update()
   {
      playerHealth.text = $"{_stats.CurrentHealth + _stats.CurrentShield}/{_stats.GetStat(StatType.MaxHealth)}";
      playerAmmoCounter.text = $"{_stats.CurrentAmmo}/{_stats.GetStat(StatType.GunMagazineSize)}";
      playerStamina.text = $"{_stats.CurrentStamina}/{_stats.GetStat(StatType.MaxStamina)}";

      ReloadProgressBar.fillAmount = _stats.CurrentReloadNormalized;
      
   }
}
