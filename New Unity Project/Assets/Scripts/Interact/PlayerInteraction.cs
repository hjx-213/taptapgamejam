using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core.Interaction;

/// <summary>
    /// 通过鼠标点击检测场景中的可交互物体（IInteractable）。
    /// 主摄像机把鼠标屏幕坐标转为世界坐标，再用 Physics2D.OverlapPoint
    /// 检测该点是否有 2D 碰撞器；命中后取得 IInteractable 并调用 Interact。
    ///
    /// 注意：Physics2D.Raycast(point, Vector2.zero) 是零长度射线，永远打不到
    /// 任何碰撞器；正确 API 是 OverlapPoint（检测该点是否被碰撞器覆盖）。
    ///
    /// 对话进行中（DialogueManager.IsDialogueActive）禁止点击场景物品。
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("点击设置")]
        [SerializeField] private Camera mainCamera;

        private void Awake()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                TryInteract();
            }
        }

        private void TryInteract()
        {
            // 对话进行中不允许点击场景物品。
            Game.Dialogue.DialogueManager dialogueManager =
                FindObjectOfType<Game.Dialogue.DialogueManager>();

            if (dialogueManager != null && dialogueManager.IsDialogueActive)
            {
                return;
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("PlayerInteraction：没有找到主摄像机！");
                return;
            }

            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

            // OverlapPoint：检测鼠标世界坐标处是否有 2D 碰撞器。
            Collider2D hit = Physics2D.OverlapPoint(mousePosition);

            if (hit == null)
            {
                return;
            }

            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable == null)
            {
                return;
            }

            if (!interactable.CanInteract)
            {
                return;
            }

            interactable.Interact(gameObject);
        }
    }