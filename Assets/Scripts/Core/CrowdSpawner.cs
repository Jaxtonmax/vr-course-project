using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// 负责批量生成 NavMeshAgent。即便还没有正式角色 prefab，也会自动创建占位胶囊并挂好 Agent/Animator/AgentController。
/// </summary>
public class CrowdSpawner : MonoBehaviour
{
    [Tooltip("可用的角色预制体列表。若为空，将自动使用占位胶囊。")]
    public List<GameObject> characterPrefabs = new List<GameObject>();

    [Tooltip("出生点。若为空，会在运行时自动创建 5 个沿 X 轴排列的点。")]
    public Transform[] spawnPoints;

    [Tooltip("一次性要生成多少个 NavMeshAgent。")]
    public int count = 10;

    [Tooltip("随机速度区间（米/秒）。")]
    public Vector2 speedRange = new Vector2(1.4f, 3.2f);

    [Tooltip("是否在 Start 时自动生成角色。也可以调用 Spawn() 手动生成。")]
    public bool spawnOnStart = true;

    [Tooltip("生成出的角色会放在该 Transform 之下。空则自动创建一个子物体。")]
    public Transform instancesParent;

    public List<NavMeshAgent> SpawnedAgents { get; } = new List<NavMeshAgent>();

    static GameObject _placeholderPrefab;

    void Awake()
    {
        if (instancesParent == null)
        {
            var root = new GameObject("SpawnedAgents");
            root.transform.SetParent(transform);
            root.transform.localPosition = Vector3.zero;
            instancesParent = root.transform;
        }
    }

    void Start()
    {
        if (spawnOnStart)
        {
            Spawn();
        }
    }

    /// <summary>清空已生成的 agent。</summary>
    public void Clear()
    {
        for (int i = 0; i < SpawnedAgents.Count; i++)
        {
            if (SpawnedAgents[i] != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(SpawnedAgents[i].gameObject);
                else
#endif
                    Destroy(SpawnedAgents[i].gameObject);
            }
        }
        SpawnedAgents.Clear();
    }

    /// <summary>根据当前配置重新生成一批 agent。</summary>
    public void Spawn()
    {
        Clear();

        var points = EnsureSpawnPoints();
        if (points.Length == 0)
        {
            Debug.LogWarning("CrowdSpawner: There are no spawn points available.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var prefab = GetPrefabForIndex(i);
            var sp = points[Random.Range(0, points.Length)];
            var go = Instantiate(prefab, sp.position, sp.rotation, instancesParent);
            go.name = $"{prefab.name}_Instance_{i}";

            var agent = EnsureNavMeshAgent(go);
            agent.speed = Random.Range(speedRange.x, speedRange.y);
            agent.angularSpeed = 240f;
            agent.acceleration = 8f;
            agent.autoBraking = true;

            EnsureAgentController(go);

            SpawnedAgents.Add(agent);
        }
    }

    GameObject GetPrefabForIndex(int index)
    {
        if (characterPrefabs != null && characterPrefabs.Count > 0)
        {
            return characterPrefabs[index % characterPrefabs.Count];
        }

        return GetOrCreatePlaceholder();
    }

    Transform[] EnsureSpawnPoints()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
            return spawnPoints;

        const int defaultCount = 5;
        var list = new List<Transform>(defaultCount);
        var root = new GameObject("AutoSpawnPoints");
        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;

        for (int i = 0; i < defaultCount; i++)
        {
            var p = new GameObject($"AutoSpawnPoint_{i}");
            p.transform.SetParent(root.transform);
            p.transform.localPosition = new Vector3((i - (defaultCount - 1) * 0.5f) * 2f, 0f, 0f);
            list.Add(p.transform);
        }

        spawnPoints = list.ToArray();
        return spawnPoints;
    }

    static GameObject GetOrCreatePlaceholder()
    {
        if (_placeholderPrefab == null)
        {
            var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "CapsuleAgentPlaceholder";
            capsule.GetComponent<Collider>().sharedMaterial = null;
            capsule.AddComponent<AgentController>(); // 会自动附带 NavMeshAgent + Animator
#if UNITY_EDITOR
            capsule.hideFlags = HideFlags.HideAndDontSave;
#endif
            _placeholderPrefab = capsule;
        }
        return _placeholderPrefab;
    }

    NavMeshAgent EnsureNavMeshAgent(GameObject go)
    {
        var agent = go.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = go.AddComponent<NavMeshAgent>();
        }
        return agent;
    }

    void EnsureAgentController(GameObject go)
    {
        if (go.GetComponent<AgentController>() == null)
        {
            go.AddComponent<AgentController>();
        }
    }
}
