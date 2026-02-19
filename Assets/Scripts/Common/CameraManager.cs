using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("Virtual Cameras")]
    [SerializeField] private CinemachineCamera followCam;
    [SerializeField] private CinemachineCamera interactionCam;

    [Header("Target Group")]
    [SerializeField] private CinemachineTargetGroup targetGroup;

    private const int PriorityDefault = 10;
    private const int PriorityInteraction = 20;
    private const int PriorityInactive = 0;

    private void Awake()
    {
        Instance = this;

        followCam.Priority = PriorityDefault;
        interactionCam.Priority = PriorityInactive;
        interactionCam.Target.TrackingTarget = targetGroup.Transform;
    }

    public void StartInteraction(Transform player, Transform npc)
    {
        targetGroup.Targets.Clear();
        targetGroup.Targets.Add(new CinemachineTargetGroup.Target
        {
            Object = player,
            Weight = 1f,
            Radius = 1f
        });
        targetGroup.Targets.Add(new CinemachineTargetGroup.Target
        {
            Object = npc,
            Weight = 1f,
            Radius = 1f
        });

        interactionCam.Priority = PriorityInteraction;
    }

    public void EndInteraction()
    {
        interactionCam.Priority = PriorityInactive;
    }
}
