using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [SerializeField] List<CardObject> _cards;
    [SerializeField] Transform _inventoryUI, _tradeUI, _interactUI;
    InteractObject _curInteractObject;
    int _curInteractIndex = 0, _curTradeIndex = 0;
    bool _uiActive;

    void Start() {
        // ToggleInteract(true, _intro);
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Tab)) { ToggleInventory(!_uiActive); }
    }

    public void ToggleInventory(bool status) {
        if (_uiActive && status) { return; }

        if (_uiActive != status) {
            if (status) {
                List<CardObject> cards = _cards.FindAll(c => c.unlocked == true);
                Transform preview = _inventoryUI.Find("Preview/Cards");

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
            _uiActive = status;
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
        if (_uiActive && status) { ToggleInteract(false); }

        if (_uiActive != status) {
            if (status) {
                Debug.Log("here: " + _curInteractObject);
                CycleTrade(0);
            }

            ToggleCanvasGroup(_tradeUI, status);
            _uiActive = status;
        }
    }

    public void CycleTrade(int dir = 0) {
        if (_curInteractObject == null) { return; }

        List<CardObject> giveCards = _curInteractObject.giveCards, 
                         receiveCards = _curInteractObject.receiveCards;
        int index = _curTradeIndex + dir;

        
         Debug.Log("index: " + index);
         Debug.Log("giveCards: " + giveCards.Count);
         Debug.Log("receiveCards: " + receiveCards.Count);

        if (index >= 0 && index < giveCards.Count && index < receiveCards.Count) {

            foreach(string type in new string[]{"Give", "Receive"}) {
                CardObject card;

                if (type.Equals("Give")) { card = giveCards[index]; }
                else { card = receiveCards[index]; }

                if (card != null) {
                    Image image = _tradeUI.Find(type + "/Card").GetComponent<Image>();
                    Text title = _tradeUI.Find(type + "/Title").GetComponent<Text>(),
                        desc = _tradeUI.Find(type + "/Desc").GetComponent<Text>(),
                        num = _tradeUI.Find(type + "/InStore/Num").GetComponent<Text>();
                    
                    image.sprite = card.sprite;
                    title.text = card.title;
                    desc.text = card.description;
                    num.text = card.count.ToString("00");
                }
            }

            _curTradeIndex = index;
        }
    }

    public void PerformTrade() {
        Debug.Log("trade performed");
    }

    public void ToggleInteract(bool status, InteractObject interact = null) {
        if (_uiActive && status) { return; }

        if (_uiActive != status) {
            if (status) {
                if (interact != null) {
                    List<CardObject> giveCards = interact.giveCards,
                                     receiveCards = interact.receiveCards;
                    GameObject trade = _interactUI.Find("Dialog/Trade").gameObject;

                    trade.SetActive(giveCards.Count > 0 && receiveCards.Count == giveCards.Count);
                    _curInteractObject = interact;
                    ProgressInteract(0);
                }
                else { return; }
            }
            else { 
                _curInteractIndex = 0; 
                // _curInteractObject = null;
            }

            ToggleCanvasGroup(_interactUI, status);
            _uiActive = status;
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
