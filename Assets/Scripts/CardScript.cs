using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [SerializeField] List<CardObject> _cards;
    [SerializeField] Transform _inventoryUI, _tradeUI, _interactUI, _settingUI;
    InteractObject _curInteractObject;
    PlayerScript _player;
    SceneLoaderScript _sceneLoader;
    GhostScript _ghost;
    StealthScript _stealth;
    int _curInteractIndex = 0, _curTradeIndex = 0;
    string _curUIType;
    Coroutine _animateTradeCoroutine;
    bool _uiActive;

    void Start() {
        _player = GameObject.Find("/Player").GetComponent<PlayerScript>();
        _sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoaderScript>();
        _stealth = GameObject.Find("/Stealth").GetComponent<StealthScript>();
        _ghost = GameObject.Find("/Ghost").GetComponent<GhostScript>();
    }

    void Update() {
        if (_stealth.IsAlarmed()) {
            if (_uiActive) {
                if (_curUIType.Equals("interact")) { ToggleInteract(false); }
                else if (_curUIType.Equals("inventory")) { ToggleInventory(false); }
                else if (_curUIType.Equals("trade")) { ToggleTrade(false); }
                else if (_curUIType.Equals("setting")) { ToggleSetting(false); }
            }
            return;
        }

        if (Input.GetKeyUp(KeyCode.Tab)) { 
            if (_curUIType.Equals("interact")) { ToggleInteract(false); }
            if (_curUIType.Equals("inventory") || _curUIType.Equals("")) { ToggleInventory(!_uiActive); }
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (_curUIType.Equals("setting") || _curUIType.Equals("")) { ToggleSetting(!_uiActive); }
        }
    }

    public bool IsActive() { return _uiActive; }

    public bool HasCard(string code, int count = 1) {
        return _cards.FindIndex(c => c.code.Equals(code) && c.count >= count) != -1;
    }

    public CardObject GetCard(string code) { return _cards.Find(c => c.code.Equals(code)); }

    public void UpdateCardCount(string code, int num = 0) {
        CardObject card = GetCard(code);

        if (card != null) { 
            card.count = Mathf.Min(Mathf.Max(card.count + num, 0), card.limit);
            card.unlocked = card.count > 0;
        }
    }

    public void ResetCards() {
        foreach(CardObject card in _cards) {
            switch(card.code) {
                case "blank": break;
                case "newGame": 
                    card.count = 1;
                    card.limit = 1;
                break;
                case "plotArmor":
                    card.count = 3;
                    card.limit = 5;
                break;
                case "masterKey":
                    card.count = 0;
                    card.limit = 3;
                break;
                default:
                    card.count = 0;
                    card.limit = 1;
                break;
            }

            card.unlocked = card.count > 0;
        }
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
            _curUIType = status ? "inventory" : "";
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
            count.text = card.count.ToString("00") + " / " + card.limit.ToString("00");
            desc.text = card.description;
            hilite.position = item.position;
        }
    }

    public void ToggleTrade(bool status) {
        if (_uiActive && status) { ToggleInteract(false); }

        if (_uiActive != status) {
            _curTradeIndex = 0;
            
            if (status) { CycleTrade(0); }

            ToggleCanvasGroup(_tradeUI, status);
            _uiActive = status;
            _curUIType = status ? "trade" : "";
        }
    }

    public void ToggleSetting(bool status) {
        if (_uiActive && status) { return; }

        if (_uiActive != status) {
            ToggleCanvasGroup(_settingUI, status);
            _uiActive = status;
            _curUIType = status ? "setting" : "";
        }
    }

    public void CycleTrade(int dir = 0) {
        if (_curInteractObject == null) { return; }

        List<CardObject> giveCards = _curInteractObject.giveCards, 
                         receiveCards = _curInteractObject.receiveCards;
        int index = _curTradeIndex + dir;

        if (index >= 0 && index < giveCards.Count && index < receiveCards.Count && giveCards.Count == receiveCards.Count) {
            Text dialogTitle = _tradeUI.Find("Title/Text").GetComponent<Text>();

            dialogTitle.text = "TRADE [" + (index + 1) + "/" + giveCards.Count + "]";

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
                    num.text = card.count.ToString("00") + " / " + card.limit.ToString("00");
                }
            }

            _curTradeIndex = index;
        }
    }

    public void PerformTrade() {
        CardObject giveCard = _curInteractObject.giveCards[_curTradeIndex],
                   receiveCard = _curInteractObject.receiveCards[_curTradeIndex];
        Text giveCount = _tradeUI.Find("Give/InStore/Num").GetComponent<Text>(),
             receiveCount = _tradeUI.Find("Receive/InStore/Num").GetComponent<Text>();
        Animator giveAnim = giveCount.transform.parent.GetComponent<Animator>(),
                 receiveAnim = receiveCount.transform.parent.GetComponent<Animator>();

        if ((giveCard?.count ?? 0) > 0 && (receiveCard?.count ?? 0) < (receiveCard?.limit ?? 0)) {
            giveCard.count = Mathf.Max(giveCard.count - 1, 0);
            giveCard.unlocked = giveCard.count > 0;

            receiveCard.count = Mathf.Max(receiveCard.count + 1, 0);
            receiveCard.unlocked = receiveCard.count > 0;

            giveCount.text = giveCard.count.ToString("00") + " / " + giveCard.limit.ToString("00");
            giveAnim.Play("decrease", 0);

            receiveCount.text = receiveCard.count.ToString("00") + " / " + receiveCard.limit.ToString("00");
            receiveAnim.Play("increase", 0);

            UpdateCardEffect(giveCard);
            UpdateCardEffect(receiveCard);
        }
        else {
            giveAnim.Play("decrease", 0);
            receiveAnim.Play("decrease", 0);
        }
    }

    public void UpdateCardEffect(CardObject card) {
        switch(card.code){
            case "plotArmor": _player.UpdateHealth(card.count); break;
            case "quit": _sceneLoader.LoadScene("EndSceneC"); break;
            case "gameOver": _sceneLoader.LoadScene("EndSceneA"); break;
            case "dream": _sceneLoader.LoadScene("EndSceneB"); break;
        }
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
                    CycleInteract(0);

                    // Debug.Log(_curInteractObject);
                }
                else { return; }
            }
            else { 
                _curInteractIndex = 0; 

                if (_curInteractObject.toggleEnemy) { 
                    _ghost.SetDisabled(!_ghost.IsDisabled()); 
                }
            }

            ToggleCanvasGroup(_interactUI, status);
            _uiActive = status;
            _curUIType = status ? "interact" : "";
        }
    }

    public void CycleInteract(int dir = 0) {
        Image image = _interactUI.Find("Image/Sprite").GetComponent<Image>();
        Text label = _interactUI.Find("Dialog/Text").GetComponent<Text>(),
             nextText = _interactUI.Find("Dialog/Next/Text").GetComponent<Text>();
        GameObject backBtn = _interactUI.Find("Dialog/Back").gameObject;
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

            backBtn.SetActive(index > 0);
            nextText.text = index >= (dialogs.Count - 1) ? "CLOSE X" : "NEXT >";
            _curInteractIndex = index;
        }
        // clicking the last next
        else if (index >= dialogs.Count) { ToggleInteract(false);  }
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
