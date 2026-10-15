
using UnityEngine;

namespace Game.UI
{
    public class BackpackPanelToggle : MonoBehaviour
    {
        [SerializeField] private GameObject backpackPanel;

    public void Toggle()
    {
        if (backpackPanel == null) return;

        bool willOpen = !backpackPanel.activeSelf;
        backpackPanel.SetActive(willOpen);

        if (willOpen && Game.UI.InventoryUI.Instance != null)
        {
            Game.UI.InventoryUI.Instance.RefreshFromInventory();
        }
    }

        public void Close()
        {
            if (backpackPanel != null)
                backpackPanel.SetActive(false);
        }
    }
}
