using UnityEngine;
using Game.Items.DragDrop;

namespace Game.Test
{
    /// <summary>
    /// 冒烟测试：DraggableItem 拖拽验证。
    /// 挂到带 DraggableItem 的物体上，Play 后拖它，Console 打印拖拽结果。
    /// 测完可删除本脚本。
    /// </summary>
    public class DragTest : MonoBehaviour
    {
        private void Start()
        {
            var drag = GetComponent<DraggableItem>();
            if (drag == null)
            {
                Debug.LogError("[DragTest] 本物体没有 DraggableItem 组件！");
                return;
            }

            drag.OnDropped.AddListener(zone =>
            {
                Debug.Log($"[DragTest] ✅ 拖拽成功，落到 DropZone: {zone.name}");
            });

            drag.OnDropCancelled.AddListener(() =>
            {
                Debug.Log("[DragTest] ⚠️ 拖拽结束，没找到匹配的 DropZone，已归位");
            });

            Debug.Log($"[DragTest] 已监听拖拽事件，ItemId='{drag.ItemId}'，Play 后拖我试试（需 Main Camera 有 Physics2DRaycaster）");
        }
    }
}
