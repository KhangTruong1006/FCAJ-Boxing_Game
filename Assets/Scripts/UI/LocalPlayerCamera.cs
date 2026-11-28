using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class LocalPlayerCamera : NetworkBehaviour
{
    [SerializeField] private Transform cameraTarget;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            return;
        }

        CinemachineCamera cineCamera = FindAnyObjectByType<CinemachineCamera>();

        if(cineCamera == null)
        {
            Debug.Log("No camera was found"); 
            return;
        }

        Transform target = cameraTarget != null ? cameraTarget : transform;
        cineCamera.Follow = target;
    }
}
