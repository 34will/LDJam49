using Photon.Pun;
using UnityEngine;

public class UIManager : MonoBehaviourPun
{
    private GameObject UI;

    public string CancelInputAxis = "Cancel";

    public void Start()
    {
        UI = GameObject.Find("/UnstableUI");
    }

    public void Update()
    {
        if (!photonView.IsMine || UI == null)
            return;

        if (Input.GetAxis(CancelInputAxis) > 0)
            UI.SetActive(!UI.activeSelf);
    }
}
