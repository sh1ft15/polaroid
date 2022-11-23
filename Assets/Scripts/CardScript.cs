using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [SerializeField] List<CardObject> _cards;
    [SerializeField] Transform _inventoryUI, _tradeUI, _interactUI;
    bool _inventoryActive, _tradeActive, _interactActive;

    void Start() {
        ToggleInventory(false);
        ToggleTrade(false);
        ToggleInteract(false);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Tab)) { ToggleInventory(!_inventoryActive); }
    }

    public void ToggleInventory(bool status) {
        if (_inventoryActive != status) {
            if (status) {
                if (_tradeActive) { ToggleTrade(false); }
                if (_interactActive) { ToggleInteract(false); }

                List<CardObject> cards = _cards.FindAll(c => c.unlocked == true);
                Transform preview = _inventoryUI.Find("Preview/Cards");

                // Debug.Log(preview);

                for(int i = 0; i < preview.childCount; i++) {
                    Image image = preview.GetChild(i)?.GetComponent<Image>();

                    if (i < cards.Count) {
                        CardObject card = cards[i];
                        
                        image.name = card.code;
                        image.enabled = true;
                        image.sprite = card.sprite;
                    }
                    else { 
                        image.name = "Card";
                        image.enabled = false; 
                    }
                }

                SelectCard(preview.GetChild(0));
            }

            ToggleCanvasGroup(_inventoryUI, status);
            _inventoryActive = status;
        }
    }

    public void SelectCard(Transform item) {
        CardObject card = _cards.Find(c => c.code.Equals(item.name));
        Transform hilite = _inventoryUI.Find("Preview/Hilite"),
                  dialog = _inventoryUI.Find("Dialog");

        if (dialog != null && card != null) {
            Text title = dialog.Find("Title").GetComponent<Text>(),
                 desc = dialog.Find("Desc").GetComponent<Text>(),
                 count = dialog.Find("Count/Num").GetComponent<Text>();
            
            title.text = card.title;
            count.text = card.count.ToString("00");
            desc.text = card.description;
            hilite.position = item.position;
        }
    }

    public void ToggleTrade(bool status) {
        if (_tradeActive != status) {
            if (status) {
                if (_inventoryActive) { ToggleInventory(false); }
                if (_interactActive) { ToggleInteract(false); }
            }

            ToggleCanvasGroup(_tradeUI, status);
            _tradeActive = status;
        }
    }

    public void ToggleInteract(bool status) {
        if (_interactActive != status) {
            if (status) {
                if (_inventoryActive) { ToggleInventory(false); }
                if (_tradeActive) { ToggleTrade(false); }
            }

            ToggleCanvasGroup(_interactUI, status);
            _interactActive = status;
        }
    }

    void ToggleCanvasGroup(Transform ui, bool status) {
        CanvasGroup canvasGroup = ui.GetComponent<CanvasGroup>();

        if (canvasGroup) {
            canvasGroup.alpha = status ? 1 : 0;
            canvasGroup.interactable = status;
            canvasGroup.blocksRaycasts = status;
        }
    }
}
