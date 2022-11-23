using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    [SerializeField] GameObject _effectPrefab;
    PlayerScript _playerScript;
    int _maxPool = 20;
    Dictionary<string, List<GameObject>> objectPools;

    void Awake(){
        objectPools = new Dictionary<string, List<GameObject>>();
        
        Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();

        // prefabs["Bullet"] = _bulletPrefab.transform;
        prefabs["Effect"] = _effectPrefab.transform;

        foreach(KeyValuePair<string, Transform> prefab in prefabs){
            string key = prefab.Key;

            objectPools[key] = new List<GameObject>();

            for(int j = 0; j < _maxPool; j++){
                Transform obj = Instantiate(prefab.Value, Vector2.zero, Quaternion.identity);

                obj.parent = transform;
                obj.gameObject.SetActive(false);
                objectPools[key].Add(obj.gameObject);
            }
        }
    }

    void Start() {
        _playerScript = GameObject.Find("/Player").GetComponent<PlayerScript>();
    }

    // public void SpawnBullet(Transform origin, Vector2 post, Quaternion rotation, float speed, float damage){
    //     Transform bullet = GetPooledObject("Bullet").transform;

    //     bullet.gameObject.SetActive(true);
    //     bullet.position = post;
    //     bullet.rotation = rotation;
    //     bullet.GetComponent<Rigidbody2D>().velocity = bullet.right * speed;
    //     bullet.GetComponent<BulletScript>().SetDamage(damage);
    //     bullet.GetComponent<BulletScript>().Destroy(2);
    // }

    public IEnumerator SpawnHit(Vector2 post){
        Transform hit = GetPooledObject("Effect").transform;

        hit.gameObject.SetActive(true);
        hit.position = post;
        hit.GetComponent<Animator>().Play("explode_1", 0);

        yield return new WaitForSeconds(1);
        hit.gameObject.SetActive(false);
    }

    GameObject GetPooledObject(string key){
        List<GameObject> pools = objectPools[key];
        GameObject pooledObj = null;

        // find disabled item in pool
        for(int i = 0; i < pools.Count; i++){
            if (!pools[i].activeInHierarchy) { 
                pooledObj = pools[i];
                break;
            }
        }

        // expand pool if it's not enough
        if (pooledObj == null) {
            pooledObj = Instantiate(pools[0], Vector2.zero, Quaternion.identity);
            pooledObj.transform.parent = transform;
            pooledObj.gameObject.SetActive(false);
            pools.Add(pooledObj.gameObject);
        }

        return pooledObj; 
    }
}
