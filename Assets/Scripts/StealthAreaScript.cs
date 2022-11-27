using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealthAreaScript : MonoBehaviour
{
    StealthScript _stealth;
    GhostScript _ghost;

    void Start() {
        _stealth = GameObject.Find("Stealth")?.GetComponent<StealthScript>();
        _ghost = GameObject.Find("Ghost")?.GetComponent<GhostScript>();
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && _stealth != null && _ghost != null) {
            if (_ghost.IsDisabled() || _stealth.IsAlarmed()) { return; }

            Transform root = col.transform.root;

            if (root.tag == "Player" && !col.transform.tag.Equals("Hit")) { 
                _stealth.TriggerIndicator();
                _stealth.ResetRecoverStealth(1, .1f);
            }
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col != null && _stealth != null && _ghost != null) {
            if (_ghost.IsDisabled() || _stealth.IsAlarmed()) { return; }

            Transform root = col.transform.root;

            if (root.tag == "Player" && !col.transform.tag.Equals("Hit")) { 
                _stealth.ResetRecoverStealth(-1, 1f);
            }
        }
    }
}
