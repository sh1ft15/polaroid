using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] Transform _targetDoor;
    PlayerScript _player;
    StealthScript _stealth;
    AreaScript _area;
    bool _onDoor, _isLocked;
    
    void Start() {
        // _player = GameObject.Find("/Player").GetComponent<PlayerScript>();
        _area = GameObject.Find("/Area").GetComponent<AreaScript>();
    }

    void Update() {
        if (_onDoor) { 
            if (Input.GetKeyUp(KeyCode.W)) { _area.GotoTarget(_targetDoor); }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && col.transform.root.tag.Equals("Player")) { _onDoor = true; }
    } 

    void OnTriggerExit2D(Collider2D col) {
        if (col != null && col.transform.root.tag.Equals("Player")) { _onDoor = false; }
    } 

    public void ToggleLock(bool status) { _isLocked = status; }
}
