using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

public class DialogueKeyboardNav : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;
        if (EventSystem.current == null) return;

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return;

        var item = selected.GetComponent<OptionItem>();
        if (item != null && item.isActiveAndEnabled)
            item.InvokeOptionSelected();
    }
}