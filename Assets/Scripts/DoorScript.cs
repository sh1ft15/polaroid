using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] Transform _targetDoor;
    [SerializeField] Canvas _canvas;
    AreaScript _area;
    bool _isActive, _isLocked;
    
    void Start() {
        _area = GameObject.Find("/Area").GetComponent<AreaScript>();
        _canvas.enabled = false;
    }

    void Update() {
        if (_isActive) { if (Input.GetKeyUp(KeyCode.E)) { _area.GotoTarget(_targetDoor); } }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && _targetDoor != null) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                _canvas.enabled = true;
                _isActive = true;
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col != null && _targetDoor != null) {
            Transform root = col.transform.root;

            if (root.tag == "Player") { 
                _canvas.enabled = false;
                _isActive = false;
            }
        }
    }

    public void ToggleLock(bool status) { _isLocked = status; }
}
