using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PhotoCountdown.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DashedSpriteOutline : MonoBehaviour
    {
        private const string OutlineObjectName = "Generated Dashed Outline";
        private const float Epsilon = 0.0001f;

        [Header("Appearance")]
        [SerializeField] private Color color = Color.white;
        [SerializeField, Min(0f)] private float space = 0.06f;
        [SerializeField, Min(0.001f)] private float thickness = 0.04f;

        [Header("Dash Pattern")]
        [SerializeField, Min(0.001f)] private float dashLength = 0.12f;
        [SerializeField, Min(0f)] private float gapLength = 0.07f;
        [SerializeField, Range(0f, 1f)] private float patternOffset;

        [Header("Seamless Cycle")]
        [Tooltip("Подгоняет длины так, чтобы по контуру помещалось целое число циклов.")]
        [SerializeField] private bool fitWholeCycles = true;

        [Tooltip("Автоматически выбирает ближайшее количество циклов из Dash Length и Gap Length.")]
        [SerializeField] private bool automaticCycleCount = true;

        [Tooltip("Используется, когда Automatic Cycle Count выключен.")]
        [SerializeField, Min(1)] private int cycleCount = 12;

        [Header("Rounded Dash Caps")]
        [SerializeField] private bool roundCaps = true;

        [Tooltip("0 — плоские концы, 1 — полноценные полукруглые концы.")]
        [SerializeField, Range(0f, 1f)] private float capRoundness = 1f;

        [Tooltip("Количество сегментов на одном полукруглом конце.")]
        [SerializeField, Range(2, 16)] private int capSegments = 6;

        [Header("Line Corners")]
        [Tooltip("Ограничивает длинные острые выступы внешнего контура.")]
        [SerializeField, Range(1f, 8f)] private float lineCornerLimit = 2.5f;

        [Tooltip("Сглаживает изгиб штриха, если он проходит через угол контура.")]
        [SerializeField] private bool roundLineCorners = true;

        [Tooltip("Количество сегментов круглого соединения на углу.")]
        [SerializeField, Range(4, 24)] private int cornerSegments = 12;

        [Header("Animation")]
        [SerializeField] private bool animate;
        [SerializeField] private bool animateInEditMode = true;

        [Tooltip("Время одного полного бесшовного цикла движения пунктиров.")]
        [SerializeField, Min(0.01f)] private float animationCycleDuration = 1.5f;

        [SerializeField] private bool reverseAnimation;

        [Header("Rendering")]
        [SerializeField] private int sortingOrderOffset = 1;
        [SerializeField] private bool visible = true;

        private readonly List<Vector2> physicsShape = new();
        private readonly List<Vector3> vertices = new();
        private readonly List<Vector2> uvs = new();
        private readonly List<int> triangles = new();

        private SpriteRenderer sourceRenderer;
        private GameObject outlineObject;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Mesh mesh;
        private Material material;
        private MaterialPropertyBlock propertyBlock;

        private Sprite cachedSprite;
        private bool cachedFlipX;
        private bool cachedFlipY;
        private bool rebuildRequested = true;
        private float animationPhase;

#if UNITY_EDITOR
        private double previousEditorTime;
#endif

        public bool IsVisible => visible;

        private void OnEnable()
        {
            sourceRenderer = GetComponent<SpriteRenderer>();
            EnsureGeneratedObjects();

            rebuildRequested = true;
            Refresh();
            SyncRenderer();

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

            EnsureGeneratedObjects();
            CheckSourceChanges();
            UpdateAnimation(Time.deltaTime);
            Refresh();
            SyncRenderer();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.delayCall -= DelayedEditorRefresh;
#endif

            if (meshRenderer != null)
                meshRenderer.enabled = false;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.delayCall -= DelayedEditorRefresh;
#endif

            DestroyGeneratedObject(mesh);
            DestroyGeneratedObject(material);
            DestroyGeneratedObject(outlineObject);
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
            SyncRenderer();
        }

        public void SetColor(Color value)
        {
            color = value;
            ApplyColor();
        }

        public void SetSpace(float value)
        {
            space = Mathf.Max(0f, value);
            RequestRebuild();
        }

        public void SetThickness(float value)
        {
            thickness = Mathf.Max(0.001f, value);
            RequestRebuild();
        }

        public void SetDashLength(float value)
        {
            dashLength = Mathf.Max(0.001f, value);
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
            thickness = Mathf.Max(0.001f, newThickness);
            dashLength = Mathf.Max(0.001f, newDashLength);
            gapLength = Mathf.Max(0f, newGapLength);
            RequestRebuild();
        }

        public void Rebuild()
        {
            RequestRebuild();
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
            if (sourceRenderer == null)
                sourceRenderer = GetComponent<SpriteRenderer>();

            if (sourceRenderer.sprite == cachedSprite &&
                sourceRenderer.flipX == cachedFlipX &&
                sourceRenderer.flipY == cachedFlipY)
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
                EnsureGeneratedObjects();
                Refresh();
                SyncRenderer();
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
            if (!rebuildRequested || sourceRenderer == null)
                return;

            EnsureGeneratedObjects();
            BuildMesh();

            cachedSprite = sourceRenderer.sprite;
            cachedFlipX = sourceRenderer.flipX;
            cachedFlipY = sourceRenderer.flipY;
            rebuildRequested = false;

            ApplyColor();
        }

        private void EnsureGeneratedObjects()
        {
            if (sourceRenderer == null)
                sourceRenderer = GetComponent<SpriteRenderer>();

            if (outlineObject == null)
            {
                Transform existingTransform = transform.Find(OutlineObjectName);

                if (existingTransform != null)
                {
                    outlineObject = existingTransform.gameObject;
                }
                else
                {
                    outlineObject = new GameObject(OutlineObjectName);
                    outlineObject.transform.SetParent(transform, false);
                }

                outlineObject.hideFlags = HideFlags.HideAndDontSave;
            }

            Transform outlineTransform = outlineObject.transform;
            outlineTransform.localPosition = Vector3.zero;
            outlineTransform.localRotation = Quaternion.identity;
            outlineTransform.localScale = Vector3.one;

            if (meshFilter == null)
            {
                meshFilter = outlineObject.GetComponent<MeshFilter>();

                if (meshFilter == null)
                    meshFilter = outlineObject.AddComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = outlineObject.GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                    meshRenderer = outlineObject.AddComponent<MeshRenderer>();
            }

            if (mesh == null)
            {
                mesh = new Mesh
                {
                    name = $"{name} Dashed Outline Mesh",
                    hideFlags = HideFlags.HideAndDontSave
                };

                mesh.MarkDynamic();
            }

            meshFilter.sharedMesh = mesh;

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

            EnsureMaterial();
        }

        private void EnsureMaterial()
        {
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");

                if (shader == null)
                    shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");

                if (shader == null)
                {
                    Debug.LogError("DashedSpriteOutline could not find a compatible shader.", this);
                    return;
                }

                material = new Material(shader)
                {
                    name = "Generated Dashed Outline Material",
                    hideFlags = HideFlags.HideAndDontSave
                };

                material.mainTexture = Texture2D.whiteTexture;
            }

            meshRenderer.sharedMaterial = material;
        }

        private void BuildMesh()
        {
            mesh.Clear();
            vertices.Clear();
            uvs.Clear();
            triangles.Clear();

            Sprite sprite = sourceRenderer.sprite;

            if (sprite == null)
                return;

            int shapeCount = sprite.GetPhysicsShapeCount();

            if (shapeCount == 0)
            {
                Debug.LogWarning($"{name}: sprite has no Custom Physics Shape.", this);
                return;
            }

            for (int shapeIndex = 0; shapeIndex < shapeCount; shapeIndex++)
            {
                physicsShape.Clear();
                sprite.GetPhysicsShape(shapeIndex, physicsShape);

                if (physicsShape.Count < 3)
                    continue;

                ApplyRendererFlip(physicsShape);
                BuildShape(physicsShape);
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
        }

        private void ApplyRendererFlip(List<Vector2> points)
        {
            if (!sourceRenderer.flipX && !sourceRenderer.flipY)
                return;

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 point = points[i];

                if (sourceRenderer.flipX)
                    point.x = -point.x;

                if (sourceRenderer.flipY)
                    point.y = -point.y;

                points[i] = point;
            }
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

                BuildClosedDash(contour, distances, totalLength, startDistance,
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

                Vector2 previousNormal = GetOutwardNormal(previousDirection, counterClockwise);
                Vector2 nextNormal = GetOutwardNormal(nextDirection, counterClockwise);

                Vector2 miter = previousNormal + nextNormal;

                if (miter.sqrMagnitude <= Epsilon)
                    miter = nextNormal;
                else
                    miter.Normalize();

                float denominator = Vector2.Dot(miter, nextNormal);
                float multiplier = Mathf.Abs(denominator) > Epsilon
                    ? 1f / denominator
                    : 1f;

                multiplier = Mathf.Clamp(multiplier, -lineCornerLimit, lineCornerLimit);
                result.Add(current + miter * centerOffset * multiplier);
            }

            return result;
        }

        private void BuildClosedDash(IReadOnlyList<Vector2> contour,
            IReadOnlyList<float> distances, float totalLength, float startDistance,
            float currentDashLength)
        {
            startDistance = Mathf.Repeat(startDistance, totalLength);

            float endDistance = startDistance + currentDashLength;
            var dashPoints = new List<Vector2>();

            if (endDistance <= totalLength + Epsilon)
            {
                AppendInterval(dashPoints, contour, distances, startDistance,
                    Mathf.Min(endDistance, totalLength), true);
            }
            else
            {
                AppendInterval(dashPoints, contour, distances, startDistance,
                    totalLength, true);

                AppendInterval(dashPoints, contour, distances, 0f,
                    endDistance - totalLength, false);
            }

            RemoveConsecutiveDuplicates(dashPoints);

            if (dashPoints.Count >= 2)
                BuildRibbon(dashPoints);
        }

        private static void AppendInterval(List<Vector2> result,
            IReadOnlyList<Vector2> contour, IReadOnlyList<float> distances,
            float startDistance, float endDistance, bool includeStart)
        {
            if (endDistance - startDistance <= Epsilon)
                return;

            if (includeStart)
            {
                AddPointIfDistinct(result,
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

            AddPointIfDistinct(result,
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

            Vector2 startDirection = (points[0] - points[1]).normalized;
            int lastIndex = points.Count - 1;
            Vector2 endDirection = (points[lastIndex] - points[lastIndex - 1]).normalized;

            AddRoundedCap(points[0], startDirection, halfThickness);
            AddRoundedCap(points[lastIndex], endDirection, halfThickness);
        }

        private void AddRibbonSegment(Vector2 start, Vector2 end, float halfThickness)
        {
            Vector2 direction = end - start;

            if (direction.sqrMagnitude <= Epsilon * Epsilon)
                return;

            float segmentLength = direction.magnitude;
            direction /= segmentLength;

            Vector2 offset = GetPerpendicular(direction) * halfThickness;
            int startVertex = vertices.Count;

            vertices.Add(start - offset);
            vertices.Add(start + offset);
            vertices.Add(end + offset);
            vertices.Add(end - offset);

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(segmentLength, 1f));
            uvs.Add(new Vector2(segmentLength, 0f));

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
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i <= segmentCount; i++)
            {
                float angle = Mathf.PI * 2f * i / segmentCount;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                vertices.Add(center + direction * radius);
                uvs.Add(direction * 0.5f + Vector2.one * 0.5f);
            }

            for (int i = 0; i < segmentCount; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + i + 1);
                triangles.Add(centerIndex + i + 2);
            }
        }

        private void AddRoundedCap(Vector2 center, Vector2 outwardDirection,
            float halfThickness)
        {
            if (outwardDirection.sqrMagnitude <= Epsilon)
                return;

            outwardDirection.Normalize();

            Vector2 sideDirection = GetPerpendicular(outwardDirection);
            float forwardRadius = halfThickness * capRoundness;
            int centerIndex = vertices.Count;

            vertices.Add(center);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i <= capSegments; i++)
            {
                float angle = -Mathf.PI * 0.5f + Mathf.PI * i / capSegments;
                float forward = Mathf.Cos(angle) * forwardRadius;
                float side = Mathf.Sin(angle) * halfThickness;

                Vector2 offset = outwardDirection * forward + sideDirection * side;

                vertices.Add(center + offset);
                uvs.Add(new Vector2(
                    0.5f + forward / Mathf.Max(thickness, Epsilon),
                    0.5f + side / Mathf.Max(thickness, Epsilon)));
            }

            for (int i = 0; i < capSegments; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(centerIndex + i + 1);
                triangles.Add(centerIndex + i + 2);
            }
        }

        private static float[] BuildDistances(IReadOnlyList<Vector2> points,
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

        private static Vector2 GetPointAtDistance(IReadOnlyList<Vector2> contour,
            IReadOnlyList<float> distances, float distance)
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

        private static Vector2 GetOutwardNormal(Vector2 direction,
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

        private static void AddPointIfDistinct(List<Vector2> points, Vector2 point)
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

        private void SyncRenderer()
        {
            if (meshRenderer == null || sourceRenderer == null)
                return;

            meshRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
            meshRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;
            meshRenderer.enabled = visible && sourceRenderer.enabled &&
                                   sourceRenderer.sprite != null;

            ApplyColor();
        }

        private void ApplyColor()
        {
            if (meshRenderer == null)
                return;

            propertyBlock ??= new MaterialPropertyBlock();

            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", color);
            meshRenderer.SetPropertyBlock(propertyBlock);
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

            EnsureGeneratedObjects();
            CheckSourceChanges();

            if (animate && animateInEditMode)
                UpdateAnimation(deltaTime);

            Refresh();
            SyncRenderer();

            if (animate && animateInEditMode && visible)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }

        private void OnValidate()
        {
            space = Mathf.Max(0f, space);
            thickness = Mathf.Max(0.001f, thickness);
            dashLength = Mathf.Max(0.001f, dashLength);
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

            sourceRenderer = GetComponent<SpriteRenderer>();
            EnsureGeneratedObjects();
            Refresh();
            SyncRenderer();
            SceneView.RepaintAll();
        }
#endif
    }
}
