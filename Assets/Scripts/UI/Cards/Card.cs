using Models.Boons;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image icon;
    [SerializeField] private Image background;

    private int boonId;
    
    public void Initialize(string cardTitle, string cardDescription, Sprite cardIcon, BoonRarity rarity, int boonId)
    {
        this.boonId = boonId;
        title.text = cardTitle;
        description.text = cardDescription;
        icon.sprite = cardIcon;
        
        switch (rarity)
        {
            case BoonRarity.Common:
                background.color = Color.gray;
                break;
            case BoonRarity.Rare:
                background.color = Color.mediumBlue;
                break;
            case BoonRarity.Epic:
                background.color = Color.purple;
                break;
            case BoonRarity.Legendary:
                background.color = Color.yellowNice;
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // highlight card
        this.transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // disable highlight
        this.transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlayerBoonUIHandler.Instance.SelectBoon(boonId, this.gameObject);
        }
    }
}
