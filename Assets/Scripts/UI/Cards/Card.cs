using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Image icon;

    private int boonId;
    
    public void Initialize(string cardTitle, string cardDescription, Sprite cardIcon, int boonId)
    {
        this.boonId = boonId;
        title.text = cardTitle;
        description.text = cardDescription;
        icon.sprite = cardIcon;
    }
}
