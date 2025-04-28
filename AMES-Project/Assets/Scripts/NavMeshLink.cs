using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshLinkPlacer : MonoBehaviour
{
    public Transform start;
    public Transform end;

    void Start()
    {
        NavMeshLink link = gameObject.AddComponent<NavMeshLink>();
        link.startPoint = start.localPosition;
        link.endPoint = end.localPosition;
        link.bidirectional = true;
    }
}
