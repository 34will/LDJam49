using UnityEngine;

namespace Unstable.UI
{
    public class UnstableUI : MonoBehaviour
    {
        public PausePanel PausePanel;

        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
