using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Unstable.UI
{
    [RequireComponent(typeof(ScrollView))]
    public class ListView : MonoBehaviour
    {
        private Dictionary<string, ListItem> values = new Dictionary<string, ListItem>();

        private List<string> orderedKeys = new List<string>();

        private float lastHeight = 0.0f;

        private float itemHeight = 0.0f;

        private ScrollRect scrollRect;

        public float ItemPadding = 10.0f;

        public ListItem ListItemPrefab;

        public bool AllowSelection = false;

        public ListItem SelectedItem;

        public EventHandler<ListItem> ItemClickedEvent;

        public EventHandler<ListItem> ItemSelectedEvent;

        private void ResetSizes()
        {
            lastHeight = itemHeight;
            scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        }

        public void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();

            itemHeight = ListItemPrefab.GetComponent<RectTransform>().rect.height;
            ResetSizes();
        }

        public void Clear()
        {
            foreach (string key in orderedKeys)
                Destroy(values[key].gameObject);

            values = new Dictionary<string, ListItem>();
            orderedKeys = new List<string>();

            ResetSizes();
        }

        private void UpdateContentHeight()
        {
            scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ItemPadding + (values.Count * (itemHeight + ItemPadding)));
        }

        public bool AddItem(string value, string displayValue = null)
        {
            if (values.ContainsKey(value))
                return false;

            ListItem newItem = Instantiate(ListItemPrefab);
            newItem.Value = value;
            newItem.ListView = this;
            lastHeight -= itemHeight + ItemPadding;
            RectTransform rectTransform = newItem.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, lastHeight, 0);
            rectTransform.SetParent(scrollRect.content, false);
            newItem.Text.text = displayValue ?? value;
            UpdateContentHeight();
            values.Add(value, newItem);
            orderedKeys.Add(value);
            return true;
        }

        public bool UpdateItem(string value, string displayValue)
        {
            if (!values.TryGetValue(value, out ListItem item))
                return false;

            item.Text.text = displayValue;
            return true;
        }

        public bool RemoveItem(string value)
        {
            if (!values.ContainsKey(value))
                return false;

            ListItem toRemove = values[value];
            values.Remove(value);
            orderedKeys.Remove(value);
            Destroy(toRemove.gameObject);

            lastHeight = itemHeight;
            foreach (string key in orderedKeys)
            {
                lastHeight -= itemHeight + ItemPadding;
                RectTransform rectTransform = values[key].GetComponent<RectTransform>();
                rectTransform.localPosition = new Vector3(175.0f, lastHeight, 0);
            }

            UpdateContentHeight();
            return true;
        }

        public ListItem GetItem(string value)
        {
            values.TryGetValue(value, out ListItem item);
            return item;
        }

        public void ItemClicked(ListItem item)
        {
            ItemClickedEvent?.Invoke(this, item);

            if (AllowSelection)
            {
                bool wasSelected = item.Selected;
                foreach (ListItem toDeselect in values.Values)
                    toDeselect.Unselect();
                if (!wasSelected)
                    item.Select();

                SelectedItem = wasSelected ? null : item;
                ItemSelectedEvent?.Invoke(this, SelectedItem);
            }
        }
    }
}
