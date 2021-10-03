using CMF;
using Photon.Pun;
using UnityEngine;
using Unstable.UI;

public class UIManager : MonoBehaviourPun
{
    private UnstableUI UI;

    private bool cancelKeyHeld = false;

    public KeyCode CancelKey = KeyCode.P;

    public CameraController CameraController;

    public AdvancedWalkerController WalkerController;

    public void Start()
    {
        UI = GameObject.Find("/UnstableUI").GetComponent<UnstableUI>();

        if (!photonView.IsMine)
            return;

        UI.PausePanel.ResumeButton.onClick.AddListener(() => UpdateUI(false));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Destroy()
    {
        UI?.PausePanel?.ResumeButton?.onClick?.RemoveAllListeners();
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
}
