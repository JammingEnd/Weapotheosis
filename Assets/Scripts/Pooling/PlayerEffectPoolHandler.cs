using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectPoolHandler : MonoBehaviour
{
    public static PlayerEffectPoolHandler Instance;

    [System.Serializable]
    public class EffectPool
    {
        public string name;
        public GameObject prefab;
        public int size = 10;
    }

    public List<EffectPool> effectPools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in effectPools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.parent = transform; // optional: keep hierarchy clean
                objectPool.Enqueue(obj);
            }

            poolDictionary[pool.name] = objectPool;
        }
    }

    public GameObject SpawnEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(effectName))
        {
            Debug.LogWarning($"Effect pool {effectName} doesn't exist!");
            return null;
        }

        GameObject effect = poolDictionary[effectName].Dequeue();
        effect.SetActive(true);
        effect.transform.position = position;
        effect.transform.rotation = rotation;

        // Return it to the pool after some time
        ParticleSystem ps = effect.transform.GetChild(0).GetComponent<ParticleSystem>();
        float lifetime = 1f;
        if (ps != null)
        { 
            lifetime = ps.main.duration;
            
            Debug.Log("Play!");
            ps.Stop();
            ps.Play();
            
        }
        
        StartCoroutine(ReturnToPool(effectName, effect, lifetime));
        

        return effect;
    }

    private IEnumerator ReturnToPool(string effectName, GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
        poolDictionary[effectName].Enqueue(effect);
    }
}
