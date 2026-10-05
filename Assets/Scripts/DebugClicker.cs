using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core.Interaction;

namespace Game.Test
{
    [RequireComponent(typeof(Collider2D))]
    public class DebugClicker : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            var interactable = GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract)
            {
                Debug.Log($"[DebugClicker] 点击了: {gameObject.name}");
                interactable.Interact(gameObject);
            }
        }
    }
}
//临时文件，可替换删除