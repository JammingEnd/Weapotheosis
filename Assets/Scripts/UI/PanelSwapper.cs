using System.Collections.Generic;
using UnityEngine;

namespace SteamLobbyUI
{
    public class PanelSwapper : MonoBehaviour
    {
        public List<Panel> panels = new List<Panel>();

        public void SwapPanel(string panelName)
        {
            foreach (var panel in panels)
            {
                if (panel.panelName == panelName)
                {
                    panel.gameObject.SetActive(true);       
                }
                else
                {
                    panel.gameObject.SetActive(false);       
                }
            }
        }
    }
}
