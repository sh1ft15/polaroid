using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchScript : MonoBehaviour
{
    [SerializeField] Transform _lights;
    [SerializeField] Canvas _canvas;
    bool _isActive, _turnedOn;
    
    void Start() { _canvas.enabled = false; }

    void Update() {
        if (!_isActive || _lights.childCount <= 0) { return; }

        if (Input.GetKeyUp(KeyCode.E)) { ToggleLight(!_turnedOn); }
    }

    public void ToggleLight(bool status) {
        _turnedOn = status;

        for(int i = 0; i < _lights.childCount; i++) {
            Transform light = _lights.GetChild(i);
            Animator anim = light?.GetComponent<Animator>();

            anim?.SetBool("turn_on", status);
        }
    }

    public bool IsTurnedOn() { return _turnedOn; }
    
    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && _lights.childCount > 0) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                _canvas.enabled = true;
                _isActive = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col != null && _lights.childCount > 0) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                _canvas.enabled = false;
               _isActive = false;
            }
        }
    }
}
