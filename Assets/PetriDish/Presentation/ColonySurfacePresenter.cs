using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace PetriDish.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ColonySurfacePresenter : MonoBehaviour
    {
        private const string DefaultTexturePropertyName = "_MainTex";
        private const float MinimumUvExtent = 0.000001f;

        [Header("Texture Binding")]
        [SerializeField] private MeshRenderer targetRenderer;
        [SerializeField] private DishRenderer textureSource;
        [Tooltip("Shader texture property that receives the live colony texture. Its matching '<property>_ST' vector receives alignment values.")]
        [SerializeField] private string texturePropertyName = DefaultTexturePropertyName;
        [SerializeField] private bool hideFlatDishImageAfterSuccessfulBinding;

        [Header("Texture Alignment")]
        [Tooltip("UV tiling applied before offset. (1, 1) preserves the current texture size.")]
        [SerializeField] private Vector2 textureScale = Vector2.one;
        [Tooltip("UV offset applied after scale and optional flips. (0, 0) preserves the current position.")]
        [SerializeField] private Vector2 textureOffset = Vector2.zero;
        [Tooltip("Mirror the live colony texture horizontally without changing the model UVs.")]
        [SerializeField] private bool flipX;
        [Tooltip("Mirror the live colony texture vertically without changing the model UVs.")]
        [SerializeField] private bool flipY;

        private MaterialPropertyBlock propertyBlock;
        private MaterialPropertyBlock originalPropertyBlock;
        private MeshRenderer appliedRenderer;
        private int appliedPropertyId;
        private int appliedTransformPropertyId;
        private bool subscribed;

        public MeshRenderer TargetRenderer => targetRenderer;
        public DishRenderer TextureSource => textureSource;
        public string TexturePropertyName => texturePropertyName;
        public Vector2 TextureScale => textureScale;
        public Vector2 TextureOffset => textureOffset;
        public bool FlipX => flipX;
        public bool FlipY => flipY;
        public string LastValidationError { get; private set; }
        public bool HasAppliedTexture { get; private set; }

        private void Reset()
        {
            targetRenderer = GetComponent<MeshRenderer>();
            texturePropertyName = DefaultTexturePropertyName;
            textureScale = Vector2.one;
            textureOffset = Vector2.zero;
            flipX = false;
            flipY = false;
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled || textureSource == null || textureSource.ColonyTexture == null)
                return;
            TryApply(textureSource.ColonyTexture, false);
        }

        private void OnEnable()
        {
            if (textureSource == null) return;
            SubscribeToSource();
            TryApply(textureSource.ColonyTexture, true);
        }

        private void OnDisable()
        {
            UnsubscribeFromSource();
            ClearAppliedTexture();
            if (hideFlatDishImageAfterSuccessfulBinding && textureSource != null)
                textureSource.SetFlatPresentationVisible(true);
        }

        public void Configure(
            MeshRenderer renderer,
            string shaderTextureProperty,
            bool hideFlatDishImage = false)
        {
            if (hideFlatDishImageAfterSuccessfulBinding && textureSource != null)
                textureSource.SetFlatPresentationVisible(true);
            ClearAppliedTexture();
            targetRenderer = renderer;
            texturePropertyName = shaderTextureProperty;
            hideFlatDishImageAfterSuccessfulBinding = hideFlatDishImage;
        }

        public bool SetTextureAlignment(
            Vector2 scale,
            Vector2 offset,
            bool horizontalFlip = false,
            bool verticalFlip = false)
        {
            textureScale = scale;
            textureOffset = offset;
            flipX = horizontalFlip;
            flipY = verticalFlip;

            if (!isActiveAndEnabled || textureSource == null || textureSource.ColonyTexture == null)
            {
                if (!ValidateAlignment(out string error))
                    return Fail(error, true);
                LastValidationError = null;
                return true;
            }

            return TryApply(textureSource.ColonyTexture, true);
        }

        public bool AutoCentre()
        {
            if (!TryGetUvBounds(out Vector2 uvMinimum, out Vector2 uvMaximum, out string error))
                return FailAlignmentAction(error);

            Vector2 uvCenter = (uvMinimum + uvMaximum) * 0.5f;
            Vector2 centeredOffset = CalculateCenteredOffset(
                uvCenter,
                textureScale,
                flipX,
                flipY);
            return SetTextureAlignment(textureScale, centeredOffset, flipX, flipY);
        }

        public bool AutoFit()
        {
            if (!TryGetUvBounds(out Vector2 uvMinimum, out Vector2 uvMaximum, out string error))
                return FailAlignmentAction(error);

            Vector2 uvSize = uvMaximum - uvMinimum;
            float largestExtent = Mathf.Max(uvSize.x, uvSize.y);
            if (!IsFinite(largestExtent) || largestExtent < MinimumUvExtent)
                return FailAlignmentAction("The target mesh UV0 bounds must have a non-zero width or height for Auto Fit.");

            float uniformScale = 1f / largestExtent;
            var fittedScale = new Vector2(uniformScale, uniformScale);
            Vector2 centeredOffset = CalculateCenteredOffset(
                (uvMinimum + uvMaximum) * 0.5f,
                fittedScale,
                flipX,
                flipY);
            return SetTextureAlignment(fittedScale, centeredOffset, flipX, flipY);
        }

        public bool ResetAlignment()
        {
            return SetTextureAlignment(Vector2.one, Vector2.zero, false, false);
        }

        public bool Bind(DishRenderer source)
        {
            if (hideFlatDishImageAfterSuccessfulBinding && textureSource != null)
                textureSource.SetFlatPresentationVisible(true);
            UnsubscribeFromSource();
            textureSource = source;
            if (textureSource == null)
                return Fail("A DishRenderer texture source is required.", true);

            if (!isActiveAndEnabled)
            {
                if (!ValidateConfiguration(out string error)) return Fail(error, true);
                LastValidationError = null;
                return true;
            }

            SubscribeToSource();
            return TryApply(textureSource.ColonyTexture, true);
        }

        public bool ValidateConfiguration(out string error)
        {
            if (targetRenderer == null)
            {
                error = "A target MeshRenderer is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(texturePropertyName))
            {
                error = "The shader texture property name cannot be empty.";
                return false;
            }

            if (!ValidateAlignment(out error))
                return false;

            Material sharedMaterial = targetRenderer.sharedMaterial;
            if (sharedMaterial == null)
            {
                error = $"MeshRenderer '{targetRenderer.name}' has no shared material.";
                return false;
            }

            int propertyId = Shader.PropertyToID(texturePropertyName);
            if (!sharedMaterial.HasProperty(propertyId))
            {
                error =
                    $"Material '{sharedMaterial.name}' using shader '{sharedMaterial.shader.name}' " +
                    $"does not expose texture property '{texturePropertyName}'.";
                return false;
            }

            error = null;
            return true;
        }

        private void SubscribeToSource()
        {
            if (subscribed || textureSource == null) return;
            textureSource.ColonyTextureChanged += OnColonyTextureChanged;
            subscribed = true;
        }

        private void UnsubscribeFromSource()
        {
            if (!subscribed) return;
            if (textureSource != null)
                textureSource.ColonyTextureChanged -= OnColonyTextureChanged;
            subscribed = false;
        }

        private void OnColonyTextureChanged(Texture2D colonyTexture)
        {
            TryApply(colonyTexture, true);
        }

        private bool TryApply(Texture colonyTexture, bool logError)
        {
            if (!ValidateConfiguration(out string error))
                return Fail(error, logError);
            if (colonyTexture == null)
                return Fail("The DishRenderer has not created a colony texture.", logError);

            int propertyId = Shader.PropertyToID(texturePropertyName);
            int transformPropertyId = Shader.PropertyToID(texturePropertyName + "_ST");
            PreparePropertyBlocks(propertyId, transformPropertyId);
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(propertyId, colonyTexture);
            propertyBlock.SetVector(transformPropertyId, CalculateTextureTransform());
            targetRenderer.SetPropertyBlock(propertyBlock);

            HasAppliedTexture = true;
            LastValidationError = null;
            if (hideFlatDishImageAfterSuccessfulBinding && textureSource != null)
                textureSource.SetFlatPresentationVisible(false);
            return true;
        }

        public static Vector4 CalculateTextureTransform(
            Vector2 scale,
            Vector2 offset,
            bool horizontalFlip,
            bool verticalFlip)
        {
            float scaleX = horizontalFlip ? -scale.x : scale.x;
            float scaleY = verticalFlip ? -scale.y : scale.y;
            float offsetX = horizontalFlip ? offset.x + scale.x : offset.x;
            float offsetY = verticalFlip ? offset.y + scale.y : offset.y;
            return new Vector4(scaleX, scaleY, offsetX, offsetY);
        }

        private bool Fail(string error, bool logError)
        {
            ClearAppliedTexture();
            LastValidationError = error;
            if (hideFlatDishImageAfterSuccessfulBinding && textureSource != null)
                textureSource.SetFlatPresentationVisible(true);
            if (logError) Debug.LogError($"ColonySurfacePresenter: {error}", this);
            return false;
        }

        private bool FailAlignmentAction(string error)
        {
            LastValidationError = error;
            Debug.LogError($"ColonySurfacePresenter: {error}", this);
            return false;
        }

        private void PreparePropertyBlocks(int propertyId, int transformPropertyId)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            if (originalPropertyBlock == null) originalPropertyBlock = new MaterialPropertyBlock();
            if (appliedRenderer == targetRenderer &&
                appliedPropertyId == propertyId &&
                appliedTransformPropertyId == transformPropertyId)
                return;

            ClearAppliedTexture();
            targetRenderer.GetPropertyBlock(originalPropertyBlock);
            appliedRenderer = targetRenderer;
            appliedPropertyId = propertyId;
            appliedTransformPropertyId = transformPropertyId;
        }

        private void ClearAppliedTexture()
        {
            if (HasAppliedTexture && appliedRenderer != null && originalPropertyBlock != null)
                appliedRenderer.SetPropertyBlock(originalPropertyBlock);
            appliedRenderer = null;
            appliedPropertyId = 0;
            appliedTransformPropertyId = 0;
            HasAppliedTexture = false;
        }

        private bool ValidateAlignment(out string error)
        {
            if (!IsFinite(textureScale.x) || !IsFinite(textureScale.y))
            {
                error = "Texture scale values must be finite numbers.";
                return false;
            }

            if (!IsFinite(textureOffset.x) || !IsFinite(textureOffset.y))
            {
                error = "Texture offset values must be finite numbers.";
                return false;
            }

            error = null;
            return true;
        }

        private Vector4 CalculateTextureTransform()
        {
            float scaleX = flipX ? -textureScale.x : textureScale.x;
            float scaleY = flipY ? -textureScale.y : textureScale.y;
            float offsetX = textureOffset.x + (flipX ? textureScale.x : 0f);
            float offsetY = textureOffset.y + (flipY ? textureScale.y : 0f);
            return new Vector4(scaleX, scaleY, offsetX, offsetY);
        }

        private bool TryGetUvBounds(
            out Vector2 uvMinimum,
            out Vector2 uvMaximum,
            out string error)
        {
            uvMinimum = default;
            uvMaximum = default;

            if (targetRenderer == null)
            {
                error = "A target MeshRenderer is required before calculating texture alignment.";
                return false;
            }

            MeshFilter meshFilter = targetRenderer.GetComponent<MeshFilter>();
            Mesh mesh = meshFilter != null ? meshFilter.sharedMesh : null;
            if (mesh == null)
            {
                error =
                    $"MeshRenderer '{targetRenderer.name}' requires a MeshFilter with a shared Mesh " +
                    "before calculating texture alignment.";
                return false;
            }

            if (!mesh.HasVertexAttribute(VertexAttribute.TexCoord0))
            {
                error = $"Mesh '{mesh.name}' does not contain UV0 texture coordinates.";
                return false;
            }

#if !UNITY_EDITOR
            if (!mesh.isReadable)
            {
                error =
                    $"Mesh '{mesh.name}' is not readable at runtime. Auto Centre and Auto Fit remain " +
                    "available in the Unity Editor without changing the model import settings.";
                return false;
            }
#endif

#if UNITY_EDITOR
            using (Mesh.MeshDataArray meshDataArray = UnityEditor.MeshUtility.AcquireReadOnlyMeshData(mesh))
#else
            using (Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh))
#endif
            {
                Mesh.MeshData meshData = meshDataArray[0];
                if (meshData.vertexCount == 0)
                {
                    error = $"Mesh '{mesh.name}' has no vertices to align.";
                    return false;
                }

                using (var uvs = new NativeArray<Vector2>(meshData.vertexCount, Allocator.Temp))
                {
                    meshData.GetUVs(0, uvs);
                    uvMinimum = uvs[0];
                    uvMaximum = uvs[0];
                    if (!IsFinite(uvMinimum.x) || !IsFinite(uvMinimum.y))
                    {
                        error = $"Mesh '{mesh.name}' contains non-finite UV0 coordinates.";
                        return false;
                    }

                    for (int index = 1; index < uvs.Length; index++)
                    {
                        Vector2 uv = uvs[index];
                        if (!IsFinite(uv.x) || !IsFinite(uv.y))
                        {
                            error = $"Mesh '{mesh.name}' contains non-finite UV0 coordinates.";
                            return false;
                        }

                        uvMinimum = Vector2.Min(uvMinimum, uv);
                        uvMaximum = Vector2.Max(uvMaximum, uv);
                    }
                }
            }

            error = null;
            return true;
        }

        private static Vector2 CalculateCenteredOffset(
            Vector2 uvCenter,
            Vector2 scale,
            bool horizontalFlip,
            bool verticalFlip)
        {
            float effectiveScaleX = horizontalFlip ? -scale.x : scale.x;
            float effectiveScaleY = verticalFlip ? -scale.y : scale.y;
            float appliedOffsetX = 0.5f - uvCenter.x * effectiveScaleX;
            float appliedOffsetY = 0.5f - uvCenter.y * effectiveScaleY;
            return new Vector2(
                appliedOffsetX - (horizontalFlip ? scale.x : 0f),
                appliedOffsetY - (verticalFlip ? scale.y : 0f));
        }

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
