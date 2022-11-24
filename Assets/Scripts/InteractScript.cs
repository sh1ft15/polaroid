using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractScript : MonoBehaviour
{
    [SerializeField] Canvas _canvas;
    [SerializeField] InteractObject _interactObject;
    CardScript _card;
    bool _isActive;

    void Start() {
        _card = GameObject.Find("Card").GetComponent<CardScript>();
        _canvas.enabled = false;

        if (_card != null && (_interactObject != null && _interactObject.playOnAwake)) { 
            _card.ToggleInteract(true, _interactObject);
        }
    }

    void Update() {
        if (!_isActive) { return; }

        if (Input.GetKeyUp(KeyCode.E)) { 
            _card.ToggleInteract(true, _interactObject);
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && _interactObject != null) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                _canvas.enabled = true;
                _isActive = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col != null && _interactObject != null) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                _canvas.enabled = false;
               _isActive = false;
            }
        }
    }
}
