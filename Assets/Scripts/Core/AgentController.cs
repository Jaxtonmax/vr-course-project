using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class AgentController : MonoBehaviour
{
    public float walkThreshold = 0.2f;
    public float runThreshold = 1.8f;
    public string speedParam = "Speed";
    public string jumpTrigger = "Jump";

    NavMeshAgent agent;
    Animator anim;
    bool jumping;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        if (agent.speed < 0.1f) agent.speed = 2.2f;
        if (agent.acceleration < 0.1f) agent.acceleration = 8f;
        if (agent.angularSpeed < 1f) agent.angularSpeed = 240f;
        agent.autoBraking = true;
    }

    void Update()
    {
        float v = agent.velocity.magnitude;
        float blend = v < walkThreshold ? 0f : (v < runThreshold ? 1f : 2f);
        anim.SetFloat(speedParam, blend, 0.2f, Time.deltaTime);

        if (agent.isOnOffMeshLink && !jumping)
            StartCoroutine(DoJump());
    }

    System.Collections.IEnumerator DoJump()
    {
        jumping = true;
        agent.autoTraverseOffMeshLink = false;
        var data = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = data.endPos + Vector3.up * 0.1f;

        Vector3 horiz = new Vector3(end.x - start.x, 0f, end.z - start.z);
        float height = Mathf.Abs(end.y - start.y);
        if (horiz.magnitude < 0.5f && height < 0.2f)
        {
            agent.Warp(end);
            agent.CompleteOffMeshLink();
            agent.autoTraverseOffMeshLink = true;
            jumping = false;
            yield break;
        }

        anim.SetTrigger(jumpTrigger);
        float dur = 0.6f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float nt = t / dur;
            Vector3 pos = Vector3.Lerp(start, end, nt);
            pos.y += 1.5f * Mathf.Sin(Mathf.PI * nt);
            agent.Warp(pos);
            yield return null;
        }

        agent.CompleteOffMeshLink();
        agent.autoTraverseOffMeshLink = true;
        jumping = false;
    }
}