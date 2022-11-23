using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostScript : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rbody;
    [SerializeField] Animator _animator;
    [SerializeField] GameObject _character, _collision;
    Vector2 _direction, _collidedPost, _nextPost, _dashTarget;
    float _moveSpeed, _damage, _curHealth, _maxHealth, _dashDelay, _digDelay, _verticalPost = -.35f;
    PlayerScript _player;
    AreaScript _area;
    StealthScript _stealth;
    AudioScript _audio;
    bool _facingRight = true, _isAttacking, _isHit, _isDeath, _isUnderground, _isActive, _isDisabled, _hasNextPost;
    bool _startingDash, _isDashing, _endingDash;
    Coroutine _hitCoroutine, _attackCoroutine, _updatePatrolCoroutine, _triggerActiveCoroutine;

    void Start() {
        _player = GameObject.Find("/Player").GetComponent<PlayerScript>();
        _area = GameObject.Find("/Area").GetComponent<AreaScript>();
        _stealth = GameObject.Find("/Stealth").GetComponent<StealthScript>();
        _audio = GameObject.Find("/Audio").GetComponent<AudioScript>();

        _moveSpeed = 2;
        _damage = 1;
        _maxHealth = _curHealth = 30;
        _dashDelay = _digDelay = 0;

        SetDisabled(true);
    }

    void Update() {
        Patrol();
    }

    void FixedUpdate() { 
        MoveCharacter(_direction.x); 
    }

    void LateUpdate() {
        Vector2 playerPost = _player.transform.position,
                curPost = _rbody.position;

        UpdateAlpha(Vector2.Distance(playerPost, curPost));
    }

    void Patrol(){
        Vector2 playerPost = _player.transform.position,
                curPost = _rbody.position,
                dir;
        float distToPost;

        // special delays
        if (!_isDashing && _dashDelay > 0) { _dashDelay -= Time.deltaTime; }
        if (!_isUnderground && _digDelay > 0) { _digDelay -= Time.deltaTime; }

        if (_isActive) {
            _nextPost = new Vector2(playerPost.x, _verticalPost);
            distToPost = Mathf.Abs(curPost.x - _nextPost.x);
            dir = (_nextPost - curPost).normalized;

            // do nothing if currently doing stuff below;
            if (_isAttacking || _isDeath || _isHit || _isUnderground) { _direction = Vector2.zero; }

            // too far, start dashing
            else if (_dashDelay <= 0 && distToPost >= 3f) { 
                if (_attackCoroutine == null) { _attackCoroutine = StartCoroutine(StartDash(dir)); } 
            }

            // too far, start digging
            else if (_digDelay <= 0 && distToPost >= 3f) { 
                if (_attackCoroutine == null) { _attackCoroutine = StartCoroutine(DigIn()); } 
            }

            // in attacking range, attack or start digging (for some reason)
            else if (distToPost <= 1f) { 
                bool colOnRight = _nextPost.x > curPost.x;

                _direction = Vector2.zero;
                Flip(colOnRight ? 1 : -1);

                if (_attackCoroutine == null) { _attackCoroutine = StartCoroutine(Attack()); }
            }

            // else, move to player
            else { _direction = dir; }
        }
        else {
            distToPost = Mathf.Abs(curPost.x - _nextPost.x);
            dir = (_nextPost - curPost).normalized;
            
            if (_digDelay <= 0 && distToPost >= 4f) {
                if (_attackCoroutine == null) { _attackCoroutine = StartCoroutine(DigIn()); } 
            }
            else if (distToPost <= 1f) { 
                if (_hasNextPost) {
                    _hasNextPost = false;
                    StartCoroutine(UpdatePatrolPath());     
                }
                else {  _direction = Vector2.zero; }
            }
            else { _direction = dir; }
        }        
    }

    // void OnTriggerEnter2D(Collider2D col) {
    //     if (col != null) {
    //         Transform root = col.transform.root;
    //         Vector2 targetPost = root.position,
    //                 curPost = transform.position,
    //                 dir = (targetPost - curPost).normalized, 
    //                 hitPost = col.ClosestPoint(curPost);
    //         bool colFacingRight,
    //              colOnRight,
    //              facingEachOther;

    //         if (root.tag.Equals("Player") && col.tag.Equals("Hit")) {
    //             colFacingRight = _player.FacingRight();
    //             colOnRight = targetPost.x > curPost.x;
    //             _collidedPost = targetPost;
    //             facingEachOther = _facingRight != colFacingRight;

    //             if (facingEachOther || (colFacingRight == !colOnRight)) { 
    //                 TriggerHit(root.position, dir); 
    //                 // _spawner.StartCoroutine(_spawner.SpawnHit(hitPost));
    //             }
    //         }
    //     }
    // }

    IEnumerator UpdatePatrolPath() {
        if (!_isActive && !_hasNextPost) {
            yield return new WaitForSeconds(Random.Range(.5f, 2));

            _hasNextPost = true;
            _nextPost = GetRandPost(_player.transform.position);
            // Debug.Log("test: " + transform.position);
        }
    }

    Vector2 GetRandPost(Vector3 relativePost) {
        float offsetNum = _isActive ? .5f : (Random.Range(1.5f, 4f));
        Vector3 newPost,
                offset;
    
        offset = new Vector3((Random.value <= .5f ? 1 : -1) * offsetNum, 0, 0);
        newPost = relativePost + offset;

        return CheckPostBound(newPost);

        // float minX = _area.GetWestBound().x,
        //       maxX = _area.GetEastBound().x,
        //       randX = Random.Range(minX + offset, maxX - offset),
        //       dist;
        // Vector2 post = new Vector2(randX, _rbody.position.y);
        // int maxLoop = 100, i = 0;

        // dist = Vector2.Distance(_rbody.position, post);

        // while(!(dist >= 3) && i < maxLoop) {
        //     post = GetRandPost();
        //     dist = Vector2.Distance(_rbody.position, post);
        //     i++;
        // }
        
        // return post;
    }

    Vector2 CheckPostBound(Vector2 post) {
        float minX = _area.GetWestBound().x,
              maxX = _area.GetEastBound().x,
              offset = 3;
        
        if (post.x > maxX) { post.x = maxX - offset; }
        else if (post.x < minX) { post.x = minX + offset; }

        return post;
    }

    public void SetActive(bool status) { 
        if (_isDisabled && status) { return; }

        _isActive = status;
        _moveSpeed = status ? 2.7f : 2;

        _stealth?.ResetRecoverStealth(-.5f, 2);
        
        if (!_isActive) { StartCoroutine(UpdatePatrolPath()); } 
    }

    public void SetDisabled(bool status) {
        _isDisabled = status;

        if (status) { SetActive(false); }
    }

    public void UpdateAlpha(float dist){
        SpriteRenderer character = _character.transform.Find("Sprite").GetComponent<SpriteRenderer>(),
                       shadow = _character.transform.Find("Shadow").GetComponent<SpriteRenderer>();
        Color charColor = character.color,
              shadowColor = shadow.color;

        if (_isDisabled) {
            if (charColor.a != 0) { charColor.a = 0; }
            if (shadowColor.a != 0) { shadowColor.a = 0; }
        }
        else {
            float maxAlpha = _isActive ? .9f : .5f,
                  maxDist = _isActive ? 6.5f : 5;

            dist = maxAlpha - Mathf.Min(dist / maxDist, maxAlpha);
            charColor.a = dist;
            shadowColor.a = dist * .3f;
        }

        if (character.color != charColor) { character.color = charColor; }
        if (shadow.color != shadowColor) { shadow.color = shadowColor; }
    }

    // public void SetActive(bool active){
    //     if (_triggerActiveCoroutine == null) { 
    //         _triggerActiveCoroutine = StartCoroutine(TriggerActive(active)); 
    //     }
    // }

    // public IEnumerator TriggerActive(bool active) {
    //     if (_attackCoroutine != null) {
    //         if (_triggerActiveCoroutine != null) { StopCoroutine(_triggerActiveCoroutine); }

    //         yield return new WaitForSeconds(.1f);
    //         _triggerActiveCoroutine = StartCoroutine(TriggerActive(active));
    //     }
    //     else {
    //         if (active) { _attackCoroutine = StartCoroutine(DigOut()); }
    //         else { _attackCoroutine = StartCoroutine(DigIn(true)); }

    //         _isActive = active;
    //         _triggerActiveCoroutine = null;
    //     }
    // }
    
    IEnumerator TriggerDeath(){
        if (!_isDeath) {
            if (_attackCoroutine != null) { 
                StopCoroutine(_attackCoroutine);
                _isAttacking = false;
                _animator.Play("idle", 0);
            }

            _isDeath = true;
            _animator.Play("death", 0);
            _rbody.gravityScale = 0;
            // _spawner.UpdateDeathCount(1);

            yield return new WaitForSeconds(2);
            gameObject.SetActive(false);
        }    
    }

    public void UpdateHealth(float num, bool increment = false){
        // Vector2 scale = _healthBar.transform.localScale;

        if (increment) {
            _curHealth = Mathf.Max(Mathf.Min(_curHealth + num, _maxHealth), 0);
        }
        else { _curHealth = Mathf.Min(num, _maxHealth); }

        // scale.x = _curHealth / _maxHealth;
        // _healthBar.transform.localScale = scale;

        if (_curHealth <= 0) { StartCoroutine(TriggerDeath()); }
    }

    public void TriggerHit(Vector2 post, Vector2 dir){
        if (!_isHit && !_isUnderground && !_isDeath && _hitCoroutine == null) {
            _collidedPost = post;
            _collidedPost.y = transform.position.y;
            _hitCoroutine = StartCoroutine(Hit(dir, 0.4f));
            // UpdateHealth(-_player.GetDamage(), true);
        }
    }

    IEnumerator Hit(Vector2 dir, float delay){
        if (!_isHit) {
            if (_attackCoroutine != null) { StopCoroutine(_attackCoroutine); }

            _isAttacking = false;
            _isUnderground = false;
            _attackCoroutine = null;
            _isHit = true; 
            _animator.Play("hurt", 0);

            yield return new WaitForSeconds(_isDeath ? 1 : delay);
            _hitCoroutine = null;
            _isHit = false;
        }
    }


    IEnumerator Attack() {
        if (!_isHit && !_isAttacking) {
            _isAttacking = true;
            _animator.SetTrigger("attack");

            yield return new WaitForSeconds(2f);
            _isAttacking = false;
            _attackCoroutine = null;
        }
    }

    IEnumerator DigIn(bool noDigOut = false) {
        if (!_isHit && !_isAttacking && !_isUnderground) {
            _isAttacking = true;
            _animator.SetTrigger("dig_in");

            yield return new WaitForSeconds(3f);

            _isUnderground = true;
            _isAttacking = false;

            if (!noDigOut) { _attackCoroutine = StartCoroutine(DigOut()); }
            else { _attackCoroutine = null; }
        }
    }

    IEnumerator DigOut() {
        if (_isUnderground && !_isHit && !_isAttacking) {
            Vector3 playerPost = _player.transform.position,
                    newPost = GetRandPost(playerPost);
            bool colOnRight;

            colOnRight = playerPost.x > transform.position.x;
            newPost.y = _verticalPost;
            transform.position = newPost;
            _direction = Vector2.zero;
            Flip(colOnRight ? 1 : -1);

            yield return new WaitForSeconds(.1f);;
            _isAttacking = true;
            _animator.SetTrigger("dig_out");

            yield return new WaitForSeconds(2f);
            _isAttacking = false;
            _attackCoroutine = null;
            _isUnderground = false;
            _digDelay = _isActive ? 1.2f : 3f;
        }
    }

    IEnumerator StartDash(Vector2 dir) {
        if (!_startingDash) {
            _startingDash = true;
            _animator.SetTrigger("dash");

            Vector2 curPost = transform.position;

            Flip(dir.x);

            // let animation play out a bit before actual dash forward
            yield return new WaitForSeconds(.2f);

            _startingDash = false;
            _isDashing = true;
            _dashTarget = curPost + (dir * 3.5f);
        }
    }

    IEnumerator EndDash() {
        if (!_endingDash) {
            _endingDash = true;

            // let animation play out a bit before ending the dash
            yield return new WaitForSeconds(.2f);

            _isDashing = _endingDash = false;
            _dashTarget = Vector2.zero;
            _attackCoroutine = null;
            _dashDelay = 1.1f;
        }
    }

    void MoveCharacter(float horizontal) {
        _direction.y = 0;

        if (_isHit) {
            _direction = new Vector2((_collidedPost.x > transform.position.x)? -1 : 1, 0);
            _rbody.velocity = _direction * (_moveSpeed * 0.5f);
            _animator.SetFloat("horizontal", Mathf.Abs(_rbody.velocity.x));
        }
        else if (_isDashing) {
            Vector2 curPost = transform.position;

            if (Vector2.Distance(_dashTarget, curPost) <= 1) { StartCoroutine(EndDash()); }
            else { _rbody.MovePosition(Vector2.Lerp(curPost, _dashTarget, 3 * Time.deltaTime)); }

            _animator.SetFloat("horizontal", 0); 
        }
        else if (_isAttacking || _isDeath || _isUnderground || _startingDash || _endingDash) { 
            _rbody.velocity = Vector2.zero;
            _animator.SetFloat("horizontal", 0); 
        }
        else {
            _rbody.velocity = _direction * _moveSpeed;
            Flip(horizontal);
            _animator.SetFloat("horizontal", Mathf.Abs(_rbody.velocity.x));
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

    public bool FacingRight() { return _facingRight; }

    public bool IsDisabled() { return _isDisabled; }

    public void Revive(){
        if (_isDeath) {
            gameObject.SetActive(true);

            _isUnderground = false;
            // _stunCoroutine = null;
            _isAttacking = false;

            // re-initialize stats
            _maxHealth = _curHealth = 30;
            UpdateHealth(_maxHealth);

            _character.GetComponent<Collider2D>().enabled = true;
            _animator.Play("idle", 0);
            _isDeath = false;
        }
    }
}
