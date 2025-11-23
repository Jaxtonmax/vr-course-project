using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public LineRenderer pathLinePrefab;
    public LayerMask groundMask;
    public Camera cam;

    List<NavMeshAgent> selection = new List<NavMeshAgent>();
    Vector3? startPoint;
    bool waitingStartClick;
    bool waitingGoalClick;

    public void SelectAgents(List<NavMeshAgent> agents) { selection = agents; }

    void Awake()
    {
        if (groundMask.value == 0)
        {
            int mask = LayerMask.NameToLayer("Ground");
            groundMask = mask >= 0 ? 1 << mask : Physics.DefaultRaycastLayers;
        }
    }

    public void SetStartFromClick()
    {
        // 切换到“等待下一次鼠标左键点击”的模式，真正的取点在 Update 里做
        waitingStartClick = true;
        waitingGoalClick = false;
    }

    public void SetGoalFromClickAndApply(bool batch = true)
    {
        // 同样让 Update 里的点击来决定目标点
        if (startPoint == null)
        {
            Debug.LogWarning("SetGoalFromClickAndApply: startPoint is null, please pick start first.");
            return;
        }
        waitingGoalClick = true;
    }

    public void ApplyPath(Vector3 start, Vector3 goal, IList<NavMeshAgent> agents, bool drawLine)
    {
        NavMeshPath refPath = null;

        foreach (var a in agents)
        {
            if (a == null) continue;
            // 对每个 agent 用自己的当前位置作为起点，避免“全体从同一 start 点出发”导致路径无效
            var from = a.transform.position;
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(from, goal, NavMesh.AllAreas, path) || path.corners.Length == 0)
            {
                a.SetDestination(goal);
            }
            else
            {
                a.SetPath(path);
                if (drawLine)
                {
                    // 只用第一个 agent 的路径来画线，避免屏幕太乱
                    if (refPath == null) refPath = path;
                }
            }
        }
        if (drawLine && refPath != null)
        {
            DrawPath(refPath);
        }
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (waitingStartClick)
        {
            if (TryPickNavmeshPoint(out var p))
            {
                startPoint = p;
            }
            waitingStartClick = false;
        }
        else if (waitingGoalClick)
        {
            if (startPoint != null && TryPickNavmeshPoint(out var goal))
            {
                ApplyPath(startPoint.Value, goal, selection, true);
                startPoint = null;
            }
            waitingGoalClick = false;
        }
    }

    bool TryPickNavmeshPoint(out Vector3 point)
    {
        point = default;
        if (cam == null) cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("PathManager: no camera assigned.");
            return false;
        }

        var ray = cam.ScreenPointToRay(Input.mousePosition);
        int mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask.value;
        if (Physics.Raycast(ray, out var hit, 2000f, mask))
        {
            if (NavMesh.SamplePosition(hit.point, out var navHit, 2f, NavMesh.AllAreas))
            {
                point = navHit.position;
                return true;
            }
        }
        return false;
    }

    void DrawPath(NavMeshPath path)
    {
        if (pathLinePrefab == null || path.corners == null || path.corners.Length == 0) return;
        var lr = Instantiate(pathLinePrefab);
        lr.positionCount = path.corners.Length;
        lr.SetPositions(path.corners);
        Destroy(lr.gameObject, 3f);
    }
}
