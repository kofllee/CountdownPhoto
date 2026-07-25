using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhotoCountdown.Presentation.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class DashedUIImageOutline : MonoBehaviour
    {
        private const string OutlineObjectName = "Generated Dashed UI Outline";
        private const float Epsilon = 0.0001f;

        [Header("Appearance")]
        [SerializeField] private Color color = Color.white;
        [SerializeField, Min(0f)] private float space = 6f;
        [SerializeField, Min(0.01f)] private float thickness = 4f;

        [Header("Dash Pattern")]
        [SerializeField, Min(0.01f)] private float dashLength = 12f;
        [SerializeField, Min(0f)] private float gapLength = 7f;
        [SerializeField, Range(0f, 1f)] private float patternOffset;

        [Header("Seamless Cycle")]
        [SerializeField] private bool fitWholeCycles = true;
        [SerializeField] private bool automaticCycleCount = true;
        [SerializeField, Min(1)] private int cycleCount = 12;

        [Header("Rounded Dash Caps")]
        [SerializeField] private bool roundCaps = true;
        [SerializeField, Range(0f, 1f)] private float capRoundness = 1f;
        [SerializeField, Range(2, 16)] private int capSegments = 6;

        [Header("Line Corners")]
        [SerializeField, Range(1f, 8f)] private float lineCornerLimit = 2.5f;
        [SerializeField] private bool roundLineCorners = true;
        [SerializeField, Range(4, 24)] private int cornerSegments = 12;

        [Header("Animation")]
        [SerializeField] private bool animate;
        [SerializeField] private bool animateInEditMode = true;
        [SerializeField, Min(0.01f)] private float animationCycleDuration = 1.5f;
        [SerializeField] private bool reverseAnimation;

        [Header("State")]
        [SerializeField] private bool visible = true;

        private readonly List<Vector2> physicsShape = new();
        private readonly List<Vector3> vertices = new();
        private readonly List<int> triangles = new();

        private Image sourceImage;
        private RectTransform sourceRectTransform;
        private GameObject outlineObject;
        private RectTransform outlineRectTransform;
        private DashedOutlineGraphic outlineGraphic;

        private Sprite cachedSprite;
        private Rect cachedRect;
        private bool cachedPreserveAspect;
        private Image.Type cachedImageType;
        private bool rebuildRequested = true;
        private float animationPhase;

#if UNITY_EDITOR
        private double previousEditorTime;
#endif

        public bool IsVisible => visible;

        private void OnEnable()
        {
            CacheSource();
            EnsureGeneratedObject();
            rebuildRequested = true;
            Refresh();
            SyncGraphic();

#if UNITY_EDITOR
            previousEditorTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
#endif
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            EnsureGeneratedObject();
            CheckSourceChanges();
            UpdateAnimation(Time.unscaledDeltaTime);
            Refresh();
            SyncGraphic();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.delayCall -= DelayedEditorRefresh;
#endif

            if (outlineGraphic != null)
                outlineGraphic.enabled = false;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.delayCall -= DelayedEditorRefresh;
#endif

            DestroyGeneratedObject(outlineObject);
        }

        private void OnRectTransformDimensionsChange()
        {
            RequestRebuild();
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void SetVisible(bool value)
        {
            visible = value;
            SyncGraphic();
        }

        public void SetColor(Color value)
        {
            color = value;
            SyncGraphic();
        }

        public void SetSpace(float value)
        {
            space = Mathf.Max(0f, value);
            RequestRebuild();
        }

        public void SetThickness(float value)
        {
            thickness = Mathf.Max(0.01f, value);
            RequestRebuild();
        }

        public void SetDashLength(float value)
        {
            dashLength = Mathf.Max(0.01f, value);
            RequestRebuild();
        }

        public void SetGapLength(float value)
        {
            gapLength = Mathf.Max(0f, value);
            RequestRebuild();
        }

        public void SetPatternOffset(float value)
        {
            patternOffset = Mathf.Repeat(value, 1f);
            RequestRebuild();
        }

        public void SetCapRoundness(float value)
        {
            capRoundness = Mathf.Clamp01(value);
            RequestRebuild();
        }

        public void SetAnimation(bool enabled)
        {
            animate = enabled;
            ResetEditorAnimationTime();
        }

        public void SetAnimation(bool enabled, float cycleDuration)
        {
            animate = enabled;
            animationCycleDuration = Mathf.Max(0.01f, cycleDuration);
            ResetEditorAnimationTime();
        }

        public void SetAnimationPhase(float value)
        {
            animationPhase = Mathf.Repeat(value, 1f);
            RequestRebuild();
        }

        public void RestartAnimation()
        {
            animationPhase = 0f;
            ResetEditorAnimationTime();
            RequestRebuild();
        }

        public void Configure(Color newColor, float newSpace, float newThickness,
            float newDashLength, float newGapLength)
        {
            color = newColor;
            space = Mathf.Max(0f, newSpace);
            thickness = Mathf.Max(0.01f, newThickness);
            dashLength = Mathf.Max(0.01f, newDashLength);
            gapLength = Mathf.Max(0f, newGapLength);
            RequestRebuild();
        }

        public void Rebuild()
        {
            RequestRebuild();
        }

        private void CacheSource()
        {
            if (sourceImage == null)
                sourceImage = GetComponent<Image>();

            if (sourceRectTransform == null)
                sourceRectTransform = (RectTransform)transform;
        }

        private void UpdateAnimation(float deltaTime)
        {
            if (!animate || !visible || deltaTime <= 0f)
                return;

            float direction = reverseAnimation ? -1f : 1f;
            animationPhase = Mathf.Repeat(
                animationPhase + direction * deltaTime / animationCycleDuration, 1f);

            rebuildRequested = true;
        }

        private void CheckSourceChanges()
        {
            CacheSource();

            Sprite sprite = sourceImage.overrideSprite;
            Rect rect = sourceRectTransform.rect;

            if (sprite == cachedSprite && rect == cachedRect &&
                sourceImage.preserveAspect == cachedPreserveAspect &&
                sourceImage.type == cachedImageType)
            {
                return;
            }

            rebuildRequested = true;
        }

        private void RequestRebuild()
        {
            rebuildRequested = true;

            if (isActiveAndEnabled)
            {
                EnsureGeneratedObject();
                Refresh();
                SyncGraphic();
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
#endif
        }

        private void Refresh()
        {
            if (!rebuildRequested)
                return;

            CacheSource();
            EnsureGeneratedObject();
            BuildGeometry();

            cachedSprite = sourceImage.overrideSprite;
            cachedRect = sourceRectTransform.rect;
            cachedPreserveAspect = sourceImage.preserveAspect;
            cachedImageType = sourceImage.type;
            rebuildRequested = false;
        }

        private void EnsureGeneratedObject()
        {
            CacheSource();

            if (outlineObject == null)
            {
                Transform existing = transform.Find(OutlineObjectName);

                if (existing != null)
                {
                    outlineObject = existing.gameObject;
                }
                else
                {
                    outlineObject = new GameObject(
                        OutlineObjectName,
                        typeof(RectTransform),
                        typeof(CanvasRenderer),
                        typeof(DashedOutlineGraphic));

                    outlineObject.transform.SetParent(transform, false);
                }

                outlineObject.hideFlags = HideFlags.HideAndDontSave;
            }

            outlineRectTransform = outlineObject.GetComponent<RectTransform>();
            outlineGraphic = outlineObject.GetComponent<DashedOutlineGraphic>();

            if (outlineGraphic == null)
                outlineGraphic = outlineObject.AddComponent<DashedOutlineGraphic>();

            SyncRectTransform();

            outlineGraphic.raycastTarget = false;
            outlineGraphic.maskable = sourceImage.maskable;
        }

        private void SyncRectTransform()
        {
            if (outlineRectTransform == null || sourceRectTransform == null)
                return;

            outlineRectTransform.anchorMin = Vector2.zero;
            outlineRectTransform.anchorMax = Vector2.one;
            outlineRectTransform.offsetMin = Vector2.zero;
            outlineRectTransform.offsetMax = Vector2.zero;
            outlineRectTransform.pivot = sourceRectTransform.pivot;
            outlineRectTransform.localRotation = Quaternion.identity;
            outlineRectTransform.localScale = Vector3.one;
            outlineRectTransform.anchoredPosition3D = Vector3.zero;
        }

        private void BuildGeometry()
        {
            vertices.Clear();
            triangles.Clear();

            Sprite sprite = sourceImage.overrideSprite;

            if (sprite == null)
            {
                outlineGraphic.SetGeometry(vertices, triangles, color);
                return;
            }

            if (sourceImage.type != Image.Type.Simple)
            {
                Debug.LogWarning(
                    $"{name}: DashedUIImageOutline supports Image Type Simple only.",
                    this);
            }

            int shapeCount = sprite.GetPhysicsShapeCount();

            if (shapeCount == 0)
            {
                Debug.LogWarning(
                    $"{name}: the UI sprite has no Custom Physics Shape.",
                    this);

                outlineGraphic.SetGeometry(vertices, triangles, color);
                return;
            }

            Rect drawingRect = GetDrawingRect(sprite);

            for (int shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
            {
                physicsShape.Clear();
                sprite.GetPhysicsShape(shapeIndex, physicsShape);

                if (physicsShape.Count < 3)
                    continue;

                List<Vector2> mappedShape = MapShapeToRect(
                    physicsShape, sprite.bounds, drawingRect);

                BuildShape(mappedShape);
            }

            outlineGraphic.SetGeometry(vertices, triangles, color);
        }

        private Rect GetDrawingRect(Sprite sprite)
        {
            Rect rect = sourceRectTransform.rect;

            if (!sourceImage.preserveAspect)
                return rect;

            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;

            if (spriteWidth <= Epsilon || spriteHeight <= Epsilon)
                return rect;

            float spriteRatio = spriteWidth / spriteHeight;
            float rectRatio = rect.width / Mathf.Max(rect.height, Epsilon);
            Vector2 size;

            if (spriteRatio > rectRatio)
                size = new Vector2(rect.width, rect.width / spriteRatio);
            else
                size = new Vector2(rect.height * spriteRatio, rect.height);

            Vector2 center = rect.center;
            return new Rect(center - size * 0.5f, size);
        }

        private static List<Vector2> MapShapeToRect(
            IReadOnlyList<Vector2> points,
            Bounds spriteBounds,
            Rect drawingRect)
        {
            Vector3 boundsMin = spriteBounds.min;
            Vector3 boundsSize = spriteBounds.size;
            var result = new List<Vector2>(points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                float normalizedX = boundsSize.x > Epsilon
                    ? (point.x - boundsMin.x) / boundsSize.x
                    : 0.5f;

                float normalizedY = boundsSize.y > Epsilon
                    ? (point.y - boundsMin.y) / boundsSize.y
                    : 0.5f;

                result.Add(new Vector2(
                    Mathf.Lerp(drawingRect.xMin, drawingRect.xMax, normalizedX),
                    Mathf.Lerp(drawingRect.yMin, drawingRect.yMax, normalizedY)));
            }

            return result;
        }

        private void BuildShape(IReadOnlyList<Vector2> sourcePoints)
        {
            List<Vector2> contour = CreateOffsetContour(sourcePoints);

            if (contour.Count < 3)
                return;

            float[] distances = BuildDistances(contour, out float totalLength);

            if (totalLength <= Epsilon)
                return;

            GetPattern(totalLength, out int actualCycleCount, out float period,
                out float actualDashLength);

            if (period <= Epsilon || actualDashLength <= Epsilon)
                return;

            float effectivePhase = Mathf.Repeat(patternOffset + animationPhase, 1f);
            float phaseDistance = effectivePhase * period;

            for (int i = 0; i < actualCycleCount; i++)
            {
                float startDistance = i * period + phaseDistance;

                BuildClosedDash(
                    contour,
                    distances,
                    totalLength,
                    startDistance,
                    actualDashLength);
            }
        }

        private void GetPattern(float totalLength, out int actualCycleCount,
            out float period, out float actualDashLength)
        {
            float requestedPeriod = Mathf.Max(dashLength + gapLength, Epsilon);
            float dashRatio = Mathf.Clamp(dashLength / requestedPeriod, 0.001f, 0.999f);

            if (fitWholeCycles)
            {
                actualCycleCount = automaticCycleCount
                    ? Mathf.Max(1, Mathf.RoundToInt(totalLength / requestedPeriod))
                    : Mathf.Max(1, cycleCount);

                period = totalLength / actualCycleCount;
                actualDashLength = period * dashRatio;
                return;
            }

            period = requestedPeriod;
            actualDashLength = Mathf.Min(dashLength, period);
            actualCycleCount = Mathf.Max(1, Mathf.CeilToInt(totalLength / period));
        }

        private List<Vector2> CreateOffsetContour(IReadOnlyList<Vector2> points)
        {
            int count = points.Count;
            bool counterClockwise = GetSignedArea(points) > 0f;
            float centerOffset = space + thickness * 0.5f;
            var result = new List<Vector2>(count);

            for (int i = 0; i < count; i++)
            {
                Vector2 previous = points[(i - 1 + count) % count];
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % count];

                Vector2 previousDirection = (current - previous).normalized;
                Vector2 nextDirection = (next - current).normalized;

                Vector2 previousNormal = GetOutwardNormal(
                    previousDirection, counterClockwise);

                Vector2 nextNormal = GetOutwardNormal(
                    nextDirection, counterClockwise);

                Vector2 miter = previousNormal + nextNormal;

                if (miter.sqrMagnitude <= Epsilon)
                    miter = nextNormal;
                else
                    miter.Normalize();

                float denominator = Vector2.Dot(miter, nextNormal);
                float multiplier = Mathf.Abs(denominator) > Epsilon
                    ? 1f / denominator
                    : 1f;

                multiplier = Mathf.Clamp(
                    multiplier, -lineCornerLimit, lineCornerLimit);

                result.Add(current + miter * centerOffset * multiplier);
            }

            return result;
        }

        private void BuildClosedDash(
            IReadOnlyList<Vector2> contour,
            IReadOnlyList<float> distances,
            float totalLength,
            float startDistance,
            float currentDashLength)
        {
            startDistance = Mathf.Repeat(startDistance, totalLength);

            float endDistance = startDistance + currentDashLength;
            var dashPoints = new List<Vector2>();

            if (endDistance <= totalLength + Epsilon)
            {
                AppendInterval(
                    dashPoints,
                    contour,
                    distances,
                    startDistance,
                    Mathf.Min(endDistance, totalLength),
                    true);
            }
            else
            {
                AppendInterval(
                    dashPoints,
                    contour,
                    distances,
                    startDistance,
                    totalLength,
                    true);

                AppendInterval(
                    dashPoints,
                    contour,
                    distances,
                    0f,
                    endDistance - totalLength,
                    false);
            }

            RemoveConsecutiveDuplicates(dashPoints);

            if (dashPoints.Count >= 2)
                BuildRibbon(dashPoints);
        }

        private static void AppendInterval(
            List<Vector2> result,
            IReadOnlyList<Vector2> contour,
            IReadOnlyList<float> distances,
            float startDistance,
            float endDistance,
            bool includeStart)
        {
            if (endDistance - startDistance <= Epsilon)
                return;

            if (includeStart)
            {
                AddPointIfDistinct(
                    result,
                    GetPointAtDistance(contour, distances, startDistance));
            }

            for (int i = 1; i < contour.Count; i++)
            {
                float pointDistance = distances[i];

                if (pointDistance > startDistance + Epsilon &&
                    pointDistance < endDistance - Epsilon)
                {
                    AddPointIfDistinct(result, contour[i]);
                }
            }

            AddPointIfDistinct(
                result,
                GetPointAtDistance(contour, distances, endDistance));
        }

        private void BuildRibbon(IReadOnlyList<Vector2> points)
        {
            float halfThickness = thickness * 0.5f;

            for (int i = 0; i < points.Count - 1; i++)
                AddRibbonSegment(points[i], points[i + 1], halfThickness);

            if (roundLineCorners)
            {
                for (int i = 1; i < points.Count - 1; i++)
                    AddRoundJoin(points[i], halfThickness);
            }

            if (!roundCaps || capRoundness <= Epsilon)
                return;

            Vector2 startOutward = (points[0] - points[1]).normalized;
            int last = points.Count - 1;
            Vector2 endOutward = (points[last] - points[last - 1]).normalized;

            AddRoundedCap(points[0], startOutward, halfThickness);
            AddRoundedCap(points[last], endOutward, halfThickness);
        }

        private void AddRibbonSegment(Vector2 start, Vector2 end, float halfThickness)
        {
            Vector2 direction = end - start;

            if (direction.sqrMagnitude <= Epsilon * Epsilon)
                return;

            direction.Normalize();

            Vector2 offset = GetPerpendicular(direction) * halfThickness;
            int startVertex = vertices.Count;

            vertices.Add(start - offset);
            vertices.Add(start + offset);
            vertices.Add(end + offset);
            vertices.Add(end - offset);

            triangles.Add(startVertex);
            triangles.Add(startVertex + 1);
            triangles.Add(startVertex + 2);

            triangles.Add(startVertex);
            triangles.Add(startVertex + 2);
            triangles.Add(startVertex + 3);
        }

        private void AddRoundJoin(Vector2 center, float radius)
        {
            int segmentCount = Mathf.Max(4, cornerSegments);
            int centerIndex = vertices.Count;

            vertices.Add(center);

            for (int i = 0; i <= segmentCount; i++)
            {
                float angle = Mathf.PI * 2f * i / segmentCount;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                vertices.Add(center + direction * radius);
            }

            for (int i = 0; i < segmentCount; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + i + 1);
                triangles.Add(centerIndex + i + 2);
            }
        }

        private void AddRoundedCap(
            Vector2 center,
            Vector2 outwardDirection,
            float halfThickness)
        {
            if (outwardDirection.sqrMagnitude <= Epsilon)
                return;

            outwardDirection.Normalize();

            Vector2 sideDirection = GetPerpendicular(outwardDirection);
            float forwardRadius = halfThickness * capRoundness;
            int centerIndex = vertices.Count;

            vertices.Add(center);

            for (int i = 0; i <= capSegments; i++)
            {
                float angle = -Mathf.PI * 0.5f + Mathf.PI * i / capSegments;
                float forward = Mathf.Cos(angle) * forwardRadius;
                float side = Mathf.Sin(angle) * halfThickness;

                Vector2 offset =
                    outwardDirection * forward + sideDirection * side;

                vertices.Add(center + offset);
            }

            for (int i = 0; i < capSegments; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + i + 1);
                triangles.Add(centerIndex + i + 2);
            }
        }

        private static float[] BuildDistances(
            IReadOnlyList<Vector2> points,
            out float totalLength)
        {
            int count = points.Count;
            var distances = new float[count + 1];
            totalLength = 0f;

            for (int i = 0; i < count; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % count];

                totalLength += Vector2.Distance(current, next);
                distances[i + 1] = totalLength;
            }

            return distances;
        }

        private static Vector2 GetPointAtDistance(
            IReadOnlyList<Vector2> contour,
            IReadOnlyList<float> distances,
            float distance)
        {
            int count = contour.Count;
            float totalLength = distances[count];

            if (distance <= Epsilon || distance >= totalLength - Epsilon)
                return contour[0];

            distance = Mathf.Clamp(distance, 0f, totalLength);

            for (int i = 0; i < count; i++)
            {
                float segmentStart = distances[i];
                float segmentEnd = distances[i + 1];

                if (distance > segmentEnd)
                    continue;

                float segmentLength = segmentEnd - segmentStart;

                if (segmentLength <= Epsilon)
                    return contour[i];

                float t = (distance - segmentStart) / segmentLength;
                Vector2 next = contour[(i + 1) % count];

                return Vector2.Lerp(contour[i], next, t);
            }

            return contour[0];
        }

        private static float GetSignedArea(IReadOnlyList<Vector2> points)
        {
            float area = 0f;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % points.Count];

                area += current.x * next.y - next.x * current.y;
            }

            return area * 0.5f;
        }

        private static Vector2 GetOutwardNormal(
            Vector2 direction,
            bool counterClockwise)
        {
            return counterClockwise
                ? new Vector2(direction.y, -direction.x)
                : new Vector2(-direction.y, direction.x);
        }

        private static Vector2 GetPerpendicular(Vector2 direction)
        {
            return new Vector2(-direction.y, direction.x);
        }

        private static void AddPointIfDistinct(
            List<Vector2> points,
            Vector2 point)
        {
            if (points.Count == 0 ||
                Vector2.SqrMagnitude(points[^1] - point) > Epsilon * Epsilon)
            {
                points.Add(point);
            }
        }

        private static void RemoveConsecutiveDuplicates(List<Vector2> points)
        {
            for (int i = points.Count - 1; i > 0; i--)
            {
                if (Vector2.SqrMagnitude(points[i] - points[i - 1]) <=
                    Epsilon * Epsilon)
                {
                    points.RemoveAt(i);
                }
            }
        }

        private void SyncGraphic()
        {
            if (outlineGraphic == null || sourceImage == null)
                return;

            SyncRectTransform();

            outlineGraphic.maskable = sourceImage.maskable;
            outlineGraphic.SetColor(color);
            outlineGraphic.enabled = visible && sourceImage.enabled &&
                                     sourceImage.gameObject.activeInHierarchy &&
                                     sourceImage.overrideSprite != null;
        }

        private void ResetEditorAnimationTime()
        {
#if UNITY_EDITOR
            previousEditorTime = EditorApplication.timeSinceStartup;
#endif
        }

        private static void DestroyGeneratedObject(Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (this == null || Application.isPlaying || !isActiveAndEnabled)
                return;

            double currentTime = EditorApplication.timeSinceStartup;
            float deltaTime = Mathf.Clamp(
                (float)(currentTime - previousEditorTime), 0f, 0.1f);

            previousEditorTime = currentTime;

            EnsureGeneratedObject();
            CheckSourceChanges();

            if (animate && animateInEditMode)
                UpdateAnimation(deltaTime);

            Refresh();
            SyncGraphic();

            if (animate && animateInEditMode && visible)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }

        private void OnValidate()
        {
            space = Mathf.Max(0f, space);
            thickness = Mathf.Max(0.01f, thickness);
            dashLength = Mathf.Max(0.01f, dashLength);
            gapLength = Mathf.Max(0f, gapLength);
            patternOffset = Mathf.Repeat(patternOffset, 1f);
            cycleCount = Mathf.Max(1, cycleCount);
            capRoundness = Mathf.Clamp01(capRoundness);
            capSegments = Mathf.Clamp(capSegments, 2, 16);
            lineCornerLimit = Mathf.Max(1f, lineCornerLimit);
            cornerSegments = Mathf.Clamp(cornerSegments, 4, 24);
            animationCycleDuration = Mathf.Max(0.01f, animationCycleDuration);

            rebuildRequested = true;

            EditorApplication.delayCall -= DelayedEditorRefresh;
            EditorApplication.delayCall += DelayedEditorRefresh;
        }

        private void DelayedEditorRefresh()
        {
            if (this == null || !isActiveAndEnabled)
                return;

            CacheSource();
            EnsureGeneratedObject();
            Refresh();
            SyncGraphic();
            SceneView.RepaintAll();
        }
#endif
    }

    [ExecuteAlways]
    [AddComponentMenu("")]
    internal sealed class DashedOutlineGraphic : MaskableGraphic
    {
        private readonly List<Vector3> geometryVertices = new();
        private readonly List<int> geometryTriangles = new();

        public override Texture mainTexture => Texture2D.whiteTexture;

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
        }

        public void SetGeometry(
            IReadOnlyList<Vector3> sourceVertices,
            IReadOnlyList<int> sourceTriangles,
            Color geometryColor)
        {
            geometryVertices.Clear();
            geometryTriangles.Clear();

            for (int i = 0; i < sourceVertices.Count; i++)
                geometryVertices.Add(sourceVertices[i]);

            for (int i = 0; i < sourceTriangles.Count; i++)
                geometryTriangles.Add(sourceTriangles[i]);

            color = geometryColor;
            SetVerticesDirty();
        }

        public void SetColor(Color value)
        {
            if (color == value)
                return;

            color = value;
        }

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            UIVertex vertex = UIVertex.simpleVert;
            vertex.color = color;
            vertex.uv0 = Vector2.zero;

            for (int i = 0; i < geometryVertices.Count; i++)
            {
                vertex.position = geometryVertices[i];
                vertexHelper.AddVert(vertex);
            }

            for (int i = 0; i + 2 < geometryTriangles.Count; i += 3)
            {
                vertexHelper.AddTriangle(
                    geometryTriangles[i],
                    geometryTriangles[i + 1],
                    geometryTriangles[i + 2]);
            }
        }
    }
}
