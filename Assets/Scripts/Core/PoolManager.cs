using System.Collections.Generic;
using UnityEngine;

public sealed class PoolManager : Singleton<PoolManager>
{
    [System.Serializable] private struct PoolDefinition { public GameObject prefab; [Min(0)] public int prewarmCount; }
    private sealed class Pool { public readonly GameObject Prefab; public readonly Queue<GameObject> Available = new(); public Pool(GameObject prefab) => Prefab = prefab; }

    [SerializeField] private PoolDefinition[] prewarmPools;
    private readonly Dictionary<GameObject, Pool> pools = new();
    private readonly Dictionary<GameObject, Pool> instancePools = new();
    private Transform poolRoot;

    protected override void Awake()
    {
        base.Awake();
        if (!IsPrimaryInstance) return;
        poolRoot = new GameObject("Pooled Objects").transform;
        poolRoot.SetParent(transform);
        if (prewarmPools != null)
            foreach (PoolDefinition definition in prewarmPools) CreatePool(definition.prefab, definition.prewarmCount);
    }

    public void CreatePool(GameObject prefab, int prewarmCount = 0)
    {
        if (prefab == null || pools.ContainsKey(prefab)) return;
        Pool pool = new(prefab);
        pools.Add(prefab, pool);
        for (int i = 0; i < prewarmCount; i++) pool.Available.Enqueue(CreateInstance(pool));
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null) return null;
        if (!pools.TryGetValue(prefab, out Pool pool)) { CreatePool(prefab); pool = pools[prefab]; }
        GameObject instance = pool.Available.Count > 0 ? pool.Available.Dequeue() : CreateInstance(pool);
        instance.transform.SetParent(parent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        Notify(instance, true);
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;
        if (!instancePools.TryGetValue(instance, out Pool pool)) { Destroy(instance); return; }
        Notify(instance, false);
        instance.SetActive(false);
        instance.transform.SetParent(poolRoot);
        pool.Available.Enqueue(instance);
    }

    private GameObject CreateInstance(Pool pool)
    {
        GameObject instance = Instantiate(pool.Prefab, poolRoot);
        instance.SetActive(false);
        instancePools.Add(instance, pool);
        return instance;
    }

    private static void Notify(GameObject instance, bool spawned)
    {
        foreach (MonoBehaviour component in instance.GetComponentsInChildren<MonoBehaviour>(true))
            if (component is IPoolable poolable) { if (spawned) poolable.OnSpawned(); else poolable.OnDespawned(); }
    }
}
