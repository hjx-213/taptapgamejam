using System.Collections;
using UnityEngine;
using Game.Core.Interaction;

namespace Game.Puzzle
{
    /// <summary>
    /// 3-1 旋转拼图的单个圆环：
    /// - 实现 IInteractable，挂了 Collider2D 后可被玩家点击
    /// - 点击后通知父 puzzle 旋转自己
    /// - 提供 IsMarkerAtTop() 给父检测解谜
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RotatingRing : MonoBehaviour, IInteractable
    {
        [SerializeField] private RotatingRingPuzzle parent;
        [SerializeField] private Transform marker;
        [SerializeField] private string prompt = "转动";

        private SpriteRenderer markerRenderer;
        private Color baseColor = Color.white;

        public string PromptText => (parent != null && parent.IsSolved) ? "" : prompt;
        public bool CanInteract => parent != null && !parent.IsSolved;
        public Transform MarkerTransform => marker;

        /// <summary>由 RotatingRingPuzzle.AutoCreateRings 调用，把 parent/marker 注入进来</summary>
        public void Initialize(RotatingRingPuzzle parentPuzzle, Transform markerTransform)
        {
            parent = parentPuzzle;
            marker = markerTransform;
            if (marker != null)
            {
                markerRenderer = marker.GetComponent<SpriteRenderer>();
                baseColor = markerRenderer != null ? markerRenderer.color : Color.white;
            }
        }

        /// <summary>对齐反馈：对齐时 marker 变绿，未对齐恢复初始金色（由父 puzzle 的 CheckSolve 调用）</summary>
        public void SetAlignedVisual(bool aligned)
        {
            if (markerRenderer == null) return;
            markerRenderer.color = aligned ? Color.green : baseColor;
        }

        public void Interact(GameObject player)
        {
            if (parent == null) return;
            if (!CanInteract) return;
            parent.OnRingClicked(this);
        }

        /// <summary>吸附到最近的 0°（或 360°）位置，让 marker 严丝合缝在 12 点钟</summary>
        public void SnapToTop()
        {
            if (parent != null && parent.IsSolved) return;
            StopAllCoroutines();
            float currentZ = transform.eulerAngles.z;
            // 计算到 0° 的最短旋转（例如 350° → +10°，10° → -10°）
            float correction = -currentZ;
            if (correction < -180f) correction += 360f;
            if (Mathf.Abs(correction) < 0.01f) return;  // 已经在顶部，不动
            StartCoroutine(RotateCoroutine(correction, 0.12f));
        }

        /// <summary>平滑旋转固定角度（用协程做缓动）</summary>
        public void RotateByStep(float stepDegrees, float duration)
        {
            StopAllCoroutines();
            StartCoroutine(RotateCoroutine(stepDegrees, duration));
        }

        private IEnumerator RotateCoroutine(float step, float duration)
        {
            float elapsed = 0f;
            float startZ = transform.eulerAngles.z;
            float endZ = startZ + step;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(startZ, endZ, t));
                yield return null;
            }
            transform.rotation = Quaternion.Euler(0, 0, endZ);
        }

        /// <summary>判断 marker 是否在顶部（容差范围内）</summary>
        public bool IsMarkerAtTop(float tolerance)
        {
            if (marker == null) return false;
            // marker 在 ring 局部坐标里的角度（+Y = 90°，+X = 0°）
            Vector3 localPos = marker.localPosition;
            float markerLocalAngle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
            // 当前 ring 的 Z 旋转
            float ringRot = transform.eulerAngles.z;
            // marker 的世界角度 = ring 旋转 + 局部角度
            float worldAngle = Mathf.Repeat(ringRot + markerLocalAngle, 360f);
            // 顶部 = 90°（+Y 方向），用 DeltaAngle 自动处理环绕
            return Mathf.Abs(Mathf.DeltaAngle(worldAngle, 90f)) <= tolerance;
        }
    }
}
