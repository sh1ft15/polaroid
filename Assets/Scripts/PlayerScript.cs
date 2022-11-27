using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rbody;
    [SerializeField] Animator _animator;
    [SerializeField] GameObject _character, _collision, _healthBar;
    Collider2D _standCollider;
    SpawnerScript _spawner;
    StealthScript _stealth;
    CardScript _card;
    AudioScript _audio;
    AreaScript _area;
    SceneLoaderScript _sceneLoader;
    Vector2 _direction, _collidedPost, _checkScale, _dodgeTarget;
    bool _facingRight = true, _isAttacking, _isDodging, _isCrouching, _isHit, _isDeath, _isDisabled, _forceCrouch;
    Coroutine _attackCoroutine, _hitCoroutine, _recoverArmorCoroutine, _stunCoroutine, footstepCoroutine;
    float _damage = 2, _moveSpeed = 3, _curHealth = 0, _maxHealth = 10;
    int _obstacleLayer;

    void Start(){
        _spawner = GameObject.Find("/Spawner").GetComponent<SpawnerScript>();
        _stealth = GameObject.Find("/Stealth").GetComponent<StealthScript>();
        _area = GameObject.Find("/Area").GetComponent<AreaScript>();
        _card = GameObject.Find("/Card").GetComponent<CardScript>();
        _audio = GameObject.Find("/Audio").GetComponent<AudioScript>();
        _sceneLoader = GameObject.Find("/SceneLoader").GetComponent<SceneLoaderScript>();

        _obstacleLayer = LayerMask.GetMask("Obstacle");
        _checkScale = new Vector2(1, 2);

        // init health
        CardObject plotArmor = _card.GetCard("plotArmor");

        if (plotArmor != null) {
            _curHealth = plotArmor.count;
            _maxHealth = plotArmor.limit;
        }
        
        UpdateHealth(_curHealth);

        // todo: disabled before deployment
        _card.ResetCards();
    }

    void Update() {
        if (_isDeath || _isDisabled || _card.IsActive() || _area.IsChanginArea()) { 
            if (_isCrouching) { 
                if (CanStandUp()) { StandUp(); }
                else { _forceCrouch = true; }
            }

            return; 
        }

        _direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

        if (Input.GetKeyDown(KeyCode.S)) { Crouch(); }

        if (Input.GetKeyUp(KeyCode.S)) { 
            if (CanStandUp()) { StandUp(); }
            else { _forceCrouch = true; }
        }

        if (Input.GetKeyDown(KeyCode.Space)) { 
            Vector2 dir = _direction;

            if (_direction.magnitude <= 0) { dir.x = _facingRight ? 1 : -1; }
            
            _attackCoroutine = StartCoroutine(DodgeRoll(dir)); 
        }

        if (Input.GetMouseButtonDown(0)) { _attackCoroutine = StartCoroutine(Attack());  }
    }

    void FixedUpdate() { MoveCharacter(_direction.x); }

    void LateUpdate() {
        if (_forceCrouch && CanStandUp()) { StandUp(); }
    }

    void StandUp() {
        if (_isCrouching) {
            _animator.SetBool("crouch", false); 
            _isCrouching = false;
            _forceCrouch = false;
            _moveSpeed = 3.5f;
        }
    }

    void Crouch() {
        if (!_isCrouching && _card.HasCard("stealth")) {
            _animator.SetBool("crouch", true); 
            _isCrouching = true;
            _forceCrouch = false;
            _moveSpeed = 2.5f;
        }
    }

    public bool IsCrouching() { return _isCrouching; }

    bool CanStandUp() {
        Collider2D[] colls = Physics2D.OverlapBoxAll(_rbody.position, _checkScale, 0, _obstacleLayer);
        return colls.Length == 0;
    }

    // void OnDrawGizmos(){
    //     Matrix4x4 oldMatrix = Gizmos.matrix;

    //     Gizmos.matrix = Matrix4x4.TRS(_rbody.position, Quaternion.identity, _checkScale);
    //     Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    //     Gizmos.matrix = oldMatrix;
    // }
 
    void MoveCharacter(float horizontal) {
        float num = 0;

        if (_isDeath || _isAttacking || _isDisabled || _card.IsActive() || _area.IsChanginArea()) { 
            _rbody.velocity = Vector2.zero; 
            _animator.SetFloat("horizontal", num); 
        }
        else if (_isDodging) {
            if (Vector2.Distance(_rbody.position, _dodgeTarget) > .5f) {
                _rbody.MovePosition(Vector2.Lerp(transform.position, _dodgeTarget, 3 * Time.deltaTime)); 
            }
            
            _animator.SetFloat("horizontal", 0); 
        }
        else if (_isHit) {
            _direction = new Vector2((_collidedPost.x > transform.position.x)? -1 : 1, 0);
            _rbody.velocity = _direction * (_moveSpeed * 0.5f);
            num = Mathf.Abs(_rbody.velocity.x);
            _animator.SetFloat("horizontal", num);
        }
        else {
            Flip(horizontal);
            _rbody.velocity = _direction * _moveSpeed; 
            num = Mathf.Abs(_rbody.velocity.x);
            _animator.SetFloat("horizontal", num); 
        }

        if (num > 0 && footstepCoroutine == null && !_isDodging) {
            footstepCoroutine = StartCoroutine(CycleFootStep());
        }
    }

    IEnumerator Attack(){
        if (!_isHit && !_isAttacking && _attackCoroutine == null) {
            Flip(_direction.x);
            _isAttacking = true;
            _animator.SetTrigger("punch");

            yield return new WaitForSeconds(.7f);
            _attackCoroutine = null;
            _isAttacking = false;
        }
    }

    IEnumerator DodgeRoll(Vector2 dir) {
        if (!_isHit && !_isDodging && _attackCoroutine == null) {
            Vector2 curPost = _rbody.position;

            Flip(_direction.x);            
            _animator.SetTrigger("dodge");

            _isDodging = true;
            _dodgeTarget = CheckPostBound(curPost + (dir * 3.5f));

            yield return new WaitForSeconds(.7f);
            _attackCoroutine = null;
            _isDodging = false;
        }
    }

    void Flip(float horizontal) {
        if ((horizontal > 0 && !_facingRight) || (horizontal < 0 && _facingRight)) {
            Quaternion rotation;

            _facingRight = !_facingRight;
            rotation = Quaternion.Euler(0, _facingRight ? 0 : 180, 0);
            _character.transform.rotation = rotation;
            _collision.transform.rotation = rotation;
        }
    }

    public void UpdateHealth(float num, bool increment = false){
        if (_healthBar == null) { return; }
        if (_curHealth <= 0) { StartCoroutine(TriggerDeath()); }

        Text text = _healthBar.transform.Find("Text").GetComponent<Text>();
        Animator animator = _healthBar.GetComponent<Animator>();

        if (increment) { _curHealth = Mathf.Max(Mathf.Min(_curHealth + num, _maxHealth), 0); }
        else { _curHealth = Mathf.Min(num, _maxHealth); }

        text.text = _curHealth.ToString("00");
        animator.SetTrigger("update");
    }

    public bool FacingRight() { return _facingRight; }

    void OnTriggerEnter2D(Collider2D col) {
        if (col != null && col.tag.Equals("Hit")) {

            // ignore if already death or not alerted by enemy
            if (_isDeath || !_stealth.IsAlarmed() || _isDodging) { return; }

            Transform root = col.transform.root;
            Vector2 targetPost = root.position,
                    curPost = transform.position,
                    dir = (targetPost - curPost).normalized,
                    hitPost = col.ClosestPoint(curPost);
            float damage = 1;
            bool back = false;

            _collidedPost = targetPost;

            switch(root.tag ?? "") {
                case "Player": break;
                default:
                    bool colOnRight = targetPost.x > curPost.x;

                    damage = 1;
                    back = colOnRight != _facingRight;
                break;
            }

            if (damage > 0) { TriggerHit(hitPost, damage, back); }
        }
    }

    void TriggerHit(Vector2 hitPost, float damage, bool back){
        if (_hitCoroutine == null) { 
            if (_isDodging) { return; }
            if (_isCrouching) { StandUp(); }

            _animator.Play(back ? "hurt_back" : "hurt", 0);
            _hitCoroutine = StartCoroutine(Hit(.6f));
            _spawner.StartCoroutine(_spawner.SpawnHit(hitPost));

            UpdateHealth(-damage, true);
        }
    }

    Vector2 CheckPostBound(Vector2 post) {
        float minX = _area.GetWestBound().x,
              maxX = _area.GetEastBound().x,
              offset = 3;
        
        if (post.x > maxX) { post.x = maxX - offset; }
        else if (post.x < minX) { post.x = minX + offset; }

        return post;
    }

    IEnumerator CycleFootStep(){
        _audio.PlayAudio(transform.GetComponent<AudioSource>(), "footstep");
        yield return new WaitForSeconds(_isCrouching ? .4f : .34f);
        footstepCoroutine = null;
    }

    IEnumerator Hit(float delay){
        if (!_isHit && !_isDisabled) {
            _isHit = true;
            yield return new WaitForSeconds(_isDeath ? 1 : delay);
            _hitCoroutine = null;
            _isHit = false;
        }
    }

    IEnumerator TriggerDeath(){
        if (!_isDeath) {
            _isDeath = true;
            _animator.Play("death", 0);

            yield return new WaitForSeconds(1); 
            _card.ResetCards();
            _sceneLoader.RestartCurrentScene();
        }    
    }

    AnimationClip GetAnimClip(Animator anim, string name) {
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips){
            if (clip.name == name) { return clip; }
        }

        return null;
    }

    // string Ucwords(string str){
    //     if (string.IsNullOrEmpty(str)) return string.Empty; 
    //     return char.ToUpper(str[0]) + str.Substring(1);
    // }

}
