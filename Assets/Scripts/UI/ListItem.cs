using UnityEngine;
using UnityEngine.UI;

namespace Unstable.UI
{
    public class ListItem : MonoBehaviour
    {
        public ListView ListView;

        public Image Image;

        public Text Text;

        public string Value;

        public bool Selected = false;

        public void Click()
        {
            ListView?.ItemClicked(this);
        }

        public void Select()
        {
            if (Selected)
                return;

            Selected = true;
            Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 1.0f);
        }

        public void Unselect()
        {
            if (!Selected)
                return;

            Selected = false;
            Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, 0.0f);
        }

        public void ToggleSelected()
        {
            if (Selected)
                Unselect();
            else
                Select();
        }
    }
}
