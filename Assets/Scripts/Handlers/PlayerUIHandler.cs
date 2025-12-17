using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Models.Stats;

public class PlayerUIHandler : MonoBehaviour
{
   public static  PlayerUIHandler instance;

   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      else
      {
         Destroy(gameObject);
      }
   }

   public Canvas playerCanvas;
   public GameObject rootPanel;
   
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
      if(_stats == null) return;
      
      playerHealth.text = $"{_stats.CurrentHealth + _stats.CurrentShield}/{_stats.GetStatValue<int>(StatType.MaxHealth)}";
      playerAmmoCounter.text = $"{_stats.CurrentAmmo}/{_stats.GetStatValue<int>(StatType.GunMagazineSize)}";
      playerStamina.text = $"{_stats.CurrentStamina}/{_stats.GetStatValue<int>(StatType.MaxStamina)}";

      if (_stats.CurrentReloadNormalized == 1)
      {
         ReloadProgressBar.fillAmount = 0f;
         return;
      } 
      
      ReloadProgressBar.fillAmount = _stats.CurrentReloadNormalized;
      
   }
}
