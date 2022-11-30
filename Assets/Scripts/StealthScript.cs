using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StealthScript : MonoBehaviour
{
    [SerializeField] Transform _stealthBar;
    GhostScript _ghost;
    PlayerScript _player;
    AreaScript _area;
    AudioScript _audio;
    float _curStealth = 0, _maxStealth = 10;
    bool _isAlarmed = false;
    Coroutine _recoverStealthCoroutine;

    void Start() {
        _player = GameObject.Find("/Player").GetComponent<PlayerScript>();
        _ghost = GameObject.Find("/Ghost").GetComponent<GhostScript>();
        _audio = GameObject.Find("/Audio").GetComponent<AudioScript>();
        _area = GameObject.Find("/Area").GetComponent<AreaScript>();
        
        // _ghost.SetActive(false);
        ResetRecoverStealth(-1, 0);
    }

    public void UpdateArea(string areaName) {
        Transform targetArea = transform.Find(areaName);

        if (targetArea != null) {
            for(int i = 0; i < transform.childCount; i++) {
                GameObject child = transform.GetChild(i)?.gameObject;
                child?.SetActive(string.Equals(child.name, areaName));
            }
        }
    }

    public void UpdateStealth(float num, bool increment = false) {
        Vector2 scale = _stealthBar.localScale;

        if (increment) { _curStealth = Mathf.Max(Mathf.Min(_curStealth + num, _maxStealth), 0); }
        else { _curStealth = Mathf.Min(num, _maxStealth); }

        scale.x = _curStealth / _maxStealth;
        _stealthBar.localScale = scale;

        if (!_isAlarmed && _curStealth >= _maxStealth) { AlarmMob(true); }
        else if (_isAlarmed && _curStealth <= 0) { AlarmMob(false); }
    }

    public void ResetRecoverStealth(float dir, float delay){
        if (_isAlarmed && dir >= 0) { return; }
        if (_recoverStealthCoroutine != null) { StopCoroutine(_recoverStealthCoroutine); }
        _recoverStealthCoroutine = StartCoroutine(RecoverStealth(dir, delay));
    }

    public void AlarmMob(bool status){
        if (_isAlarmed != status) {
            Image bar = _stealthBar.GetComponent<Image>();

            _isAlarmed = status;
            _ghost.SetActive(status);
            bar.color = _isAlarmed ? Color.red : Color.white;

            _area.UpdateInteracts(status);

            if (status) { 
                _audio.PlayMusic("loop_2");
                //_audio.SetMusicPitch(1.2f); 
            }
            else { 
                _audio.PlayMusic("loop_1");
                // _audio.SetMusicPitch(1); 
            }
        }
    }

    public bool IsAlarmed() { return _isAlarmed; }

    public void TriggerIndicator() {
        _audio.PlayAudio(null, "heartbeat");
    }

    public void ToggleEnemy() {
        _area.GetSwitch().ToggleLight(false);
        _ghost.SetDisabled(false);
    }

    IEnumerator RecoverStealth(float dir, float delay){
        yield return new WaitForSeconds(delay);
        
        bool playerIsCrouching = _player.IsCrouching(),
             hasLight = _area.HasLight();
        float rate;

        if (dir < 0) { 
            rate = playerIsCrouching ? .2f : .1f; 
            rate = hasLight ? rate : rate * .3f;
        }
        else { 
            rate = playerIsCrouching ? .1f : .2f; 
            rate = hasLight ? rate * .3f : rate;
        }

        UpdateStealth(rate * dir, true);
        ResetRecoverStealth(dir, .1f);

        // Debug.Log(_recoverStealthCoroutine == null);
        //_recoverStealthCoroutine = StartCoroutine(RecoverStealth(dir, .1f));
    }
    
}
