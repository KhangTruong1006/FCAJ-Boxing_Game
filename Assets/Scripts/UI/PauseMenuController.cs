using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;

    public static bool isOpen { get; private set; }

    private void Awake()
    {
        closeMenu();

    }

    private void Update()
    {
        if(NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            return;
        }

        handleMenuInput();
    }

    private void handleMenuInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (!isOpen)
            {
                openMenu();
            }

            else
            {
                closeMenu();
            }
        }
    }

    // ===== Menu Toggling =====
    public void openMenu()
    {
        setMenuState(true);
    }

    public void closeMenu()
    {
        setMenuState(false);
    }

    private void setMenuState(bool state)
    {
        isOpen = state;
        pauseMenu.SetActive(state);
    }

    // ===== Leaving =====
    public void leaveMatch()
    {
        closeMenu() ;
        NetworkManager.Singleton.GetComponent<NetworkSession>().leaveMatch();
    }

    private void OnDisable()
    {
        isOpen = false;
    }
}
