using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [SerializeField] List<CardObject> _cards;
    [SerializeField] Transform _inventoryUI, _tradeUI, _interactUI;
    InteractObject _curInteractObject;
    int _curInteractIndex = 0;
    bool _inventoryActive, _tradeActive, _interactActive;

    void Start() {
        // ToggleInteract(true, _intro);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Tab)) { ToggleInventory(!_inventoryActive); }
    }

    public void ToggleInventory(bool status) {
        if (_inventoryActive != status) {
            if (status) {

                if (_tradeActive || _interactActive) { return; }

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
                if (_inventoryActive || _interactActive) { return; }
            }

            ToggleCanvasGroup(_tradeUI, status);
            _tradeActive = status;
        }
    }

    public void ToggleInteract(bool status, InteractObject interact = null) {
        if (_interactActive != status) {
            if (status) {
                if (_inventoryActive || _tradeActive) { return; }

                if (interact != null) {
                    _curInteractObject = interact;
                    ProgressInteract(0);
                }
                else { return; }
            }
            else { 
                _curInteractIndex = 0; 
                _curInteractObject = null;
            }

            ToggleCanvasGroup(_interactUI, status);
            _interactActive = status;
        }
    }

    public void ProgressInteract(int dir = 0) {
        Image image = _interactUI.Find("Image/Sprite").GetComponent<Image>();
        Text label = _interactUI.Find("Dialog/Text").GetComponent<Text>();
        List<string> dialogs = _curInteractObject.dialogs;
        List<Sprite> sprites = _curInteractObject.sprites;
        int index = _curInteractIndex + dir;

        if (index >= 0 && index < dialogs.Count) {
            string text = dialogs[index];

            if (text != "") { label.text = text; }
            
            if (index < sprites.Count) { 
                Sprite sprite = sprites[index]; 

                if (!image.enabled) { image.enabled = true; }

                if (sprite != null) { image.sprite = sprite; }
            }

            _curInteractIndex = index;
        }
        // clicking the last next
        else if (index >= dialogs.Count) { ToggleInteract(false); }
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
