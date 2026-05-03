using UnityEngine;

namespace AlienCrusher.UI
{
    public class DummyPanelTag : MonoBehaviour
    {
        [SerializeField] private string panelId;
        [SerializeField] private bool startsVisible;

        public string PanelId => panelId;
        public bool StartsVisible => startsVisible;

        public void Configure(string id, bool visible)
        {
            panelId = id;
            startsVisible = visible;
            gameObject.SetActive(visible);
        }
    }
}
