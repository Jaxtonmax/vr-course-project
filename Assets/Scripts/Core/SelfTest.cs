using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SelfTest : MonoBehaviour
{
    public CrowdSpawner spawner;
    public PathManager pathMgr;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(1.0f);
        if (spawner == null) spawner = FindObjectOfType<CrowdSpawner>();
        if (pathMgr == null) pathMgr = FindObjectOfType<PathManager>();
        if (spawner == null || pathMgr == null) yield break;

        if (RandomPointOnNavMesh(Vector3.zero, 20f, out var a) &&
            RandomPointOnNavMesh(new Vector3(5, 0, 5), 20f, out var b))
        {
            pathMgr.SelectAgents(spawner.SpawnedAgents);
            pathMgr.ApplyPath(a, b, spawner.SpawnedAgents, true);
            Debug.Log($"SelfTest: applied path from {a} to {b} for {spawner.SpawnedAgents.Count} agents.");
        }
        else
        {
            Debug.LogWarning("SelfTest: could not find two navmesh points. Did you bake NavMesh?");
        }
    }

    bool RandomPointOnNavMesh(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            Vector3 random = center + Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(random, out var hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = default;
        return false;
    }
}