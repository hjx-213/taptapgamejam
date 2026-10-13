using UnityEngine;
using Game.Items.DragDrop;

namespace Game.Test
{
    /// <summary>
    /// 冒烟测试：DropZone 接收验证。
    /// 挂到带 DropZone 的物体上，Play 后把 DraggableItem 拖上来，Console 打印接收日志。
    /// 测完可删除本脚本。
    /// </summary>
    public class DropTest : MonoBehaviour
    {
        private void Start()
        {
            var zone = GetComponent<DropZone>();
            if (zone == null)
            {
                Debug.LogError("[DropTest] 本物体没有 DropZone 组件！");
                return;
            }

            zone.OnItemReceived.AddListener(item =>
            {
                Debug.Log($"[DropTest] ✅ 收到道具：{item.ItemId}（来自物体 {item.name}）");
            });

            Debug.Log("[DropTest] 已监听 OnItemReceived，把匹配的 DraggableItem 拖过来试试");
        }
    }
}
