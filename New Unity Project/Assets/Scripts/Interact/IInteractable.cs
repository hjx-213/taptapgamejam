using UnityEngine;

namespace Game.Core.Interaction
{
    // 所有可交互物体的统一接口。
    // "通用点击系统"在玩家点击时会调用 Interact()，
    // 任何想被点击的物体只要实现这个接口即可。
    public interface IInteractable
    {
        // 鼠标悬停时 UI 上显示的提示文字。
        string PromptText { get; }

        // 当前是否允许交互。
        bool CanInteract { get; }

        // 点击触发。具体表现由实现者决定：
        // - ItemPickup：加入背包并消失
        // - PuzzleBase：触发谜题 UI / 检测解谜条件
        // - ItemUsage：检测是否持有道具
        void Interact(GameObject player);
    }
}