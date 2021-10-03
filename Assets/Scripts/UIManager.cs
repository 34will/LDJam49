using System;
using CMF;
using Photon.Pun;
using UnityEngine;
using Unstable.UI;

public class UIManager : MonoBehaviourPun
{
    public UnstableUI UI;

    private bool cancelKeyHeld = false;

    public KeyCode CancelKey = KeyCode.P;

    public CameraController CameraController;

    public AdvancedWalkerController WalkerController;

    public void Awake()
    {
        UI = GameObject.Find("/UnstableUI").GetComponent<UnstableUI>();
    }

    public void Start()
    {
        if (!photonView.IsMine)
            return;

        UI.PausePanel.ResumeButton.onClick.AddListener(() => UpdateUI(false));
        UI.DeathMessage.OnMessageShown += OnDeathAnimationFinished;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Destroy()
    {
        UI?.PausePanel?.ResumeButton?.onClick?.RemoveAllListeners();
        UI.DeathMessage.OnMessageShown -= OnDeathAnimationFinished;
    }

    private void UpdateUI(bool uiActive)
    {
        UI.PausePanel.gameObject.SetActive(uiActive);
        CameraController.enabled = !uiActive;
        WalkerController.enabled = !uiActive;
        Cursor.visible = uiActive;
        Cursor.lockState = uiActive ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void Update()
    {
        if (!photonView.IsMine || UI == null)
            return;

        if (Input.GetKeyDown(CancelKey) && !cancelKeyHeld)
        {
            cancelKeyHeld = true;
            UpdateUI(!UI.PausePanel.gameObject.activeSelf);
        }

        if (Input.GetKeyUp(CancelKey) && cancelKeyHeld)
            cancelKeyHeld = false;
    }

    public void OnDeath()
    {
        UI.DeathMessage.gameObject.SetActive(true);
        UI.DeathMessage.Animate();
    }

    public void OnDeathAnimationFinished(object sender, EventArgs e)
    {
        UI.DeathMessage.gameObject.SetActive(false);
        UI.SpectatingPanel.gameObject.SetActive(true);
    }
}
