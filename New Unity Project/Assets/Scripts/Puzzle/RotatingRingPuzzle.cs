using UnityEngine;
using Game.Items;

namespace Game.Puzzle
{
    /// <summary>
    /// 3-1 三层圆环旋转拼图
    ///
    /// 玩法：
    ///   - 3 个同心圆环（外/中/内），每环带一个 marker
    ///   - 玩家点击环 → 该环旋转 30°（带缓动动画）
    ///   - 3 个 marker 都转到顶部"目标窄条"位置 = 解开谜题
    ///
    /// 用法（最简）：
    ///   1. 场景里建一个空 GameObject
    ///   2. 挂这个脚本
    ///   3. rings 字段留空 → Awake 时自动生成所有视觉（环/marker/目标条）
    ///   4. 把这个 GameObject 拖到 PuzzleManager.puzzles 列表
    ///   5. Play
    /// </summary>
    public class RotatingRingPuzzle : PuzzleBase
    {
        [Header("Ring References（留空 = 自动生成）")]
        [SerializeField] private RotatingRing[] rings = new RotatingRing[0];

        [Header("Visual Config（自动生成时用）")]
        [SerializeField]
        private float[] ringRadii = new float[] { 2.4f, 1.7f, 1.0f };
        [SerializeField] private float ringThickness = 0.18f;
        [SerializeField]
        private Color[] ringColors = new Color[]
        {
            new Color(0.85f, 0.35f, 0.35f, 1f), // 外 - 红
            new Color(0.40f, 0.75f, 0.45f, 1f), // 中 - 绿
            new Color(0.40f, 0.55f, 0.85f, 1f)  // 内 - 蓝
        };
        [SerializeField] private Color markerColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color targetColor = new Color(1f, 1f, 1f, 0.9f);

        [Header("Puzzle Settings")]
        [SerializeField] private float rotateStep = 30f;   // 每次点击旋转角度
        // 容差必须 >= 步进的一半（30/2=15），否则随机初始角度下 marker 可能永远落不到容差内
        [SerializeField] private float tolerance = 15f;    // 解谜角度容差（半步 = 15°，保证任意初始角都能对齐）
        [SerializeField] private float animateDuration = 0.18f;
        [SerializeField] private bool randomizeStart = true;
        [SerializeField] private bool snapToTop = true;    // 对齐后自动吸附到正 12 点钟（让 marker 严丝合缝在一条直线）

        [Header("Feedback（可选，留空 = 无声）")]
        [SerializeField] private AudioClip rotateSound;
        [SerializeField] private AudioClip solveSound;

        [Header("解谜奖励")]
        [SerializeField] private string rewardItemId = "aosaitiyuanbing";

        public float RotateStep => rotateStep;
        public float Tolerance => tolerance;

        // ---------- Lifecycle ----------

        private void Awake()
        {
            OnSolved.AddListener(GrantReward);

            if (rings == null || rings.Length == 0 || AllNull(rings))
            {
                AutoCreateRings();
                return;
            }

            // Scene-authored rings keep their custom artwork and get the same
            // parent/marker initialization as procedurally created rings.
            foreach (RotatingRing ring in rings)
            {
                if (ring == null) continue;

                ring.Initialize(this, ring.MarkerTransform);
                if (randomizeStart)
                {
                    float startAngle = Random.Range(0f, 360f);
                    ring.transform.localRotation = Quaternion.Euler(0f, 0f, startAngle);
                }
            }
        }

        private void GrantReward()
        {
            if (string.IsNullOrWhiteSpace(rewardItemId)) return;

            if (Inventory.Instance == null)
            {
                Debug.LogError($"[RotatingRingPuzzle:{_puzzleName}] 解谜奖励发放失败：场景中没有 Inventory 实例。");
                return;
            }

            Inventory.Instance.Add(rewardItemId);
        }

        private static bool AllNull(RotatingRing[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != null) return false;
            return true;
        }

        /// <summary>
        /// 自动注册到场景里的 PuzzleManager，省得手动拖进列表。
        /// 必须 base.Start() 保留 PuzzleBase 的 _isSolvedOnStart 检查。
        /// </summary>
        protected override void Start()
        {
            base.Start();
            if (PuzzleManager.Instance != null)
                PuzzleManager.Instance.RegisterPuzzle(this);
        }

        // ---------- PuzzleBase override ----------

        /// <summary>
        /// PuzzleBase 抽象方法：玩家点击谜题本体时触发。
        /// 本谜题本体无 Collider2D，由子环各自处理点击，所以这里是空操作。
        /// </summary>
        protected override void OnPlayerInteract(GameObject player)
        {
            // no-op
        }

        // ---------- Public API（由 RotatingRing 调用） ----------

        /// <summary>某个环被点击时由 RotatingRing 调用</summary>
        public void OnRingClicked(RotatingRing ring)
        {
            if (IsSolved) return;
            ring.RotateByStep(rotateStep, animateDuration);

            if (rotateSound != null)
                AudioSource.PlayClipAtPoint(rotateSound, ring.transform.position);

            // 等动画结束再检测（避免读到中间角度）
            Invoke(nameof(CheckSolve), animateDuration + 0.05f);
        }

        // ---------- Solve Check ----------

        private void CheckSolve()
        {
            if (IsSolved) return;
            if (rings == null || rings.Length == 0) return;

            // 先更新所有环的视觉（对齐的变绿），再判断是否全对齐
            bool allAligned = true;
            foreach (var ring in rings)
            {
                if (ring == null) continue;
                bool aligned = ring.IsMarkerAtTop(tolerance);
                ring.SetAlignedVisual(aligned);
                if (aligned && snapToTop)
                {
                    // 进入容差后自动吸附到正 12 点钟，视觉上严丝合缝
                    ring.SnapToTop();
                }
                if (!aligned) allAligned = false;
            }
            if (!allAligned) return;

            // 全部对齐 → 解开
            if (solveSound != null)
                AudioSource.PlayClipAtPoint(solveSound, transform.position);

            Debug.Log($"[RotatingRingPuzzle:{_puzzleName}] ✅ All markers aligned! Solving.");
            Solve();
        }

        // ---------- Auto-create visuals ----------

        private void AutoCreateRings()
        {
            rings = new RotatingRing[ringRadii.Length];

            // 顶部目标线：竖直方向，位于外环**之外**（12 点钟方向）。
            // 这是"对齐"目标指示：marker 转正上方时正好指向它。
            // 不穿过圆环，纯粹是外部参照。
            float targetY = ringRadii[0] + 0.6f;   // 外环顶部上方 0.6 单位
            float targetHeight = 0.7f;
            GameObject targetGO = new GameObject("TargetZone");
            targetGO.transform.SetParent(transform, false);
            targetGO.transform.localPosition = new Vector3(0, targetY, 0);
            var tSR = targetGO.AddComponent<SpriteRenderer>();
            tSR.sprite = CreateBarSprite(0.18f, targetHeight, targetColor);
            tSR.sortingOrder = 30;

            for (int i = 0; i < ringRadii.Length; i++)
            {
                float radius = ringRadii[i];
                Color color = ringColors[i % ringColors.Length];

                // Ring GameObject
                GameObject ringGO = new GameObject($"Ring_{i}");
                ringGO.transform.SetParent(transform, false);

                var sr = ringGO.AddComponent<SpriteRenderer>();
                sr.sprite = CreateHollowRingSprite(radius, ringThickness, color);
                sr.sortingOrder = i;

                // 【关键】用 N 段 EdgeCollider2D 拼出一个空心圆环碰撞体
                // 这样点中心区域时本环不会响应，Physics2D.Raycast 会穿透到内圈。
                // 不能用 CircleCollider2D（实心圆盘，会盖住内圈的点击）。
                int segments = 24;
                float halfThickness = ringThickness / 2f;
                float midRadius = radius - halfThickness;
                for (int s = 0; s < segments; s++)
                {
                    float a1 = (s / (float)segments) * 360f * Mathf.Deg2Rad;
                    float a2 = ((s + 1) / (float)segments) * 360f * Mathf.Deg2Rad;
                    Vector2 p1 = new Vector2(Mathf.Cos(a1) * midRadius, Mathf.Sin(a1) * midRadius);
                    Vector2 p2 = new Vector2(Mathf.Cos(a2) * midRadius, Mathf.Sin(a2) * midRadius);
                    var edge = ringGO.AddComponent<EdgeCollider2D>();
                    edge.points = new Vector2[] { p1, p2 };
                    edge.edgeRadius = halfThickness;
                }

                // Marker（子物体，在环的"顶部"位置）
                GameObject markerGO = new GameObject("Marker");
                markerGO.transform.SetParent(ringGO.transform, false);
                markerGO.transform.localPosition = new Vector3(0, radius, 0);
                var mSR = markerGO.AddComponent<SpriteRenderer>();
                mSR.sprite = CreateDotSprite(0.14f, Color.white);
                mSR.color = markerColor;
                mSR.sortingOrder = 20 + i;

                // RotatingRing 组件
                var ring = ringGO.AddComponent<RotatingRing>();
                ring.Initialize(this, markerGO.transform);
                rings[i] = ring;

                // 初始随机角度（让谜题有变化）
                if (randomizeStart)
                {
                    float startAngle = Random.Range(0f, 360f);
                    ringGO.transform.rotation = Quaternion.Euler(0, 0, startAngle);
                }
            }
        }

        // ---------- Sprite factory（程序化生成贴图） ----------

        private static Sprite CreateHollowRingSprite(float radius, float thickness, Color color)
        {
            const int ppu = 64; // pixels per unit
            int size = Mathf.CeilToInt(radius * 2f * ppu);
            if (size < 16) size = 16;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = size * 0.5f;
            float outerR = radius * ppu;
            float innerR = Mathf.Max(0f, (radius - thickness) * ppu);
            float outerR2 = outerR * outerR;
            float innerR2 = innerR * innerR;
            Color clear = new Color(0, 0, 0, 0);

            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float d2 = dx * dx + dy * dy;
                tex.SetPixel(x, y, (d2 <= outerR2 && d2 >= innerR2) ? color : clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        }

        private static Sprite CreateDotSprite(float radius, Color color)
        {
            const int ppu = 64;
            int size = Mathf.CeilToInt(radius * 2f * ppu);
            if (size < 8) size = 8;

            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            float center = size * 0.5f;
            float r = radius * ppu;
            float r2 = r * r;
            Color clear = new Color(0, 0, 0, 0);

            for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                tex.SetPixel(x, y, (dx * dx + dy * dy <= r2) ? color : clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), ppu);
        }

        private static Sprite CreateBarSprite(float width, float height, Color color)
        {
            const int ppu = 64;
            int w = Mathf.CeilToInt(width * ppu);
            int h = Mathf.CeilToInt(height * ppu);
            if (w < 4) w = 4;
            if (h < 4) h = 4;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, color);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
