using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaScript : MonoBehaviour
{
    Transform _activeArea, _westBound, _eastBound;    
    PlayerScript _player; 
    StealthScript _stealth;
    SceneLoaderScript _sceneLoader;    
    Coroutine _changeAreaCoroutine;                                                                                                                                                                                                                

    void Awake() {
        UpdateActiveArea(transform.GetChild(0));
    }

    void Start(){
        _sceneLoader = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();
        _stealth = GameObject.Find("/Stealth").GetComponent<StealthScript>();
        _player = GameObject.Find("/Player").GetComponent<PlayerScript>();
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
        }
    }

    public Vector2 GetWestBound() { return (Vector2) _westBound?.position; }

    public Vector2 GetEastBound() { return (Vector2) _eastBound?.position; }

    public void GotoTarget(Transform target){
        if (_changeAreaCoroutine == null) { 
            _changeAreaCoroutine = StartCoroutine(ChangeArea(target));
        }
    }

    IEnumerator ChangeArea(Transform target){
        if (target != null && !_stealth.IsAlarmed()) {
            Vector2 playerPost = _player.transform.position,
                    targetPost = target.transform.position;
            string areaName = target.parent.name;

            yield return _sceneLoader.StartCoroutine(_sceneLoader.TriggerFadeIn());

            playerPost.x = targetPost.x;
            _player.transform.position = playerPost;
            UpdateArea(areaName);
            _stealth.UpdateArea(areaName);
        }
        else { Debug.Log("Cannot leave this area"); }

        _changeAreaCoroutine = null;
    }
}
