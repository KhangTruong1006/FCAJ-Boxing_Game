using Unity.Netcode;
using UnityEngine;

public class JoinMenu : MonoBehaviour
{
    public void Join()
    {
        NetworkManager.Singleton.GetComponent<NetworkSession>().joinMatch();
    }
}
