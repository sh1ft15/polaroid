using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class AreaScript : MonoBehaviour
{
    [SerializeField] Text _locationLabel;
    Transform _activeArea, _westBound, _eastBound;    
    PlayerScript _player; 
    StealthScript _stealth;
    SwitchScript _switch;
    SceneLoaderScript _sceneLoader;    
    Coroutine _changeAreaCoroutine;    
    bool _changingArea;                                                                                                                                                                                                            

    void Awake() { UpdateActiveArea(transform.GetChild(0)); }

    void Start(){
        _sceneLoader = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();
        _stealth = GameObject.Find("/Stealth").GetComponent<StealthScript>();
        _player = GameObject.Find("/Player").GetComponent<PlayerScript>();

        if (_switch != null) { _switch.ToggleLight(true); }
    }

    public void UpdateArea(string areaName){
        Transform targetArea = transform.Find(areaName);

        if (targetArea != null) {
            for(int i = 0; i < transform.childCount; i++) {
                Transform child = transform.GetChild(i);
                child.Find("Sprite").gameObject.SetActive(child.name.Equals(areaName)); 
            }
        }
    }

    void UpdateActiveArea(Transform area) {
        if (area != null) {
            _activeArea = area;
            _westBound = _activeArea.Find("WestBound");
            _eastBound = _activeArea.Find("EastBound");
            _switch = _activeArea.Find("Switch")?.GetComponent<SwitchScript>();
            _locationLabel.text = SplitCamelCase(area.name);
        }
    }

    public void UpdateInteracts(bool status) {
        if (_activeArea == null) { return; }

        Transform props = _activeArea.Find("Props");

        for(int i = 0; i < props.childCount; i++) {
            GameObject child = props.GetChild(i)?.gameObject;

            if (status) {
                InteractScript script = child.GetComponent<InteractScript>();

                if (script && script.HideOnAlarmed()) { script.gameObject.SetActive(false); }
            }
            else { if (!child.activeSelf) { child.SetActive(true); } }
        }
    }

    public Vector2 GetWestBound() { return (Vector2) _westBound?.position; }

    public Vector2 GetEastBound() { return (Vector2) _eastBound?.position; }

    public SwitchScript GetSwitch() { return _switch; }

    public void GotoTarget(Transform target){
        if (_changeAreaCoroutine == null) { 
            _changeAreaCoroutine = StartCoroutine(ChangeArea(target));
        }
    }

    string SplitCamelCase(string value) {
        return string.Join(" ", Regex.Split(value, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
    }

    public bool IsChanginArea() { return _changingArea; }

    public bool HasLight() { return _switch?.IsTurnedOn() ?? false; }

    IEnumerator ChangeArea(Transform target){
        if (target != null && !_stealth.IsAlarmed() && !_changingArea) {
            Vector2 playerPost = _player.transform.position,
                    targetPost = target.transform.position;
            string areaName = target.parent.name;

            _changingArea = true;

            yield return _sceneLoader.StartCoroutine(_sceneLoader.TriggerFadeIn());

            playerPost.x = targetPost.x;
            _player.transform.position = playerPost;

            UpdateArea(areaName);
            UpdateActiveArea(target.parent);

            yield return new WaitForSeconds(.3f);
            _changingArea = false;
        }
        else { Debug.Log("Cannot leave this area"); }

        _changeAreaCoroutine = null;
    }
}
