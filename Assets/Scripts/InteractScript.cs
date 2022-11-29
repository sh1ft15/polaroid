using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractScript : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] List<InteractObject> _interacts;
    [SerializeField] int _curInteractIndex = 0;    
    [SerializeField] bool _hideOnAlarmed, _playOnAwake, _playOnTrigger;
    CardScript _card;
    StealthScript _stealth;
    bool _isActive, _isTriggered;

    void Start() {
        _stealth = GameObject.Find("/Stealth").GetComponent<StealthScript>();
        _card = GameObject.Find("/Card").GetComponent<CardScript>();
        _canvas.enabled = false;

        if (_interacts.Count > 0) {
            InteractObject interact = _interacts[_curInteractIndex];

            if (interact != null && _playOnAwake) { 
                _card.ToggleInteract(true, interact);
                _curInteractIndex = Mathf.Min(_curInteractIndex + 1, _interacts.Count - 1);
            }
        }
    }

    bool CheckCondition(string condition) {
        if (condition.Equals("")) { return true; }
        else { 
            string[] conds = condition.Split('|');
            bool status = false;

            foreach(string cond in conds) {
                string[] parts = cond.Split('-');
                int count = int.Parse(parts[0]);
                string code = parts[1];

                if (_card.HasCard(code, count)) { status = true; }
                else { return false; }
            }

            return status; 
        }
    }

    void Update() {
        if (!_isActive || _interacts.Count <= 0 || _playOnTrigger) { return; }

        if (Input.GetKeyUp(KeyCode.E)) { ToggleInteract(); }
    }

    void ToggleInteract() {
        int index = _curInteractIndex;
        InteractObject interact = _interacts[index];

        if (interact != null && !CheckCondition(interact.condition)) {
            index = Mathf.Max(index - 1, 0);
            interact = _interacts[index];
        }

        if (interact != null) {
            _card.ToggleInteract(true, interact);
            _curInteractIndex = Mathf.Min(index + 1, _interacts.Count - 1);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && _interacts.Count > 0) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                // playOnTrigger trigger once only
                if (_playOnTrigger) { 
                    if (!_isTriggered) { 
                        ToggleInteract(); 
                        _isTriggered = true;
                    }
                }
                else { 
                    _canvas.enabled = true;
                    _isActive = true;
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col != null && _interacts.Count > 0) {
            Transform root = col.transform.root;

            if (root.tag == "Player") {
                _canvas.enabled = false;
                _isActive = false;
            }
        }
    }

    public bool HideOnAlarmed() { return _hideOnAlarmed; }
}
