using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

namespace Unstable
{
    [RequireComponent(typeof(InputField))]
    public class PlayerNameInputField : MonoBehaviour
    {
        private const string playerNamePrefKey = "PlayerName";

        public void Start()
        {
            string defaultName = string.Empty;
            InputField inputField = GetComponent<InputField>();
            if (inputField != null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    inputField.text = defaultName;
                }
            }

            PhotonNetwork.NickName = defaultName;
        }

        public void SetPlayerName(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(playerNamePrefKey, value);
            Debug.Log(PhotonNetwork.NickName);
        }
    }
}
