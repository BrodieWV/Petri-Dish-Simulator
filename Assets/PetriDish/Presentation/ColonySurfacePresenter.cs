using UnityEngine;

namespace PetriDish.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ColonySurfacePresenter : MonoBehaviour
    {
        private const string DefaultTexturePropertyName = "_MainTex";

        [SerializeField] private MeshRenderer targetRenderer;
        [SerializeField] private DishRenderer textureSource;
        [SerializeField] private string texturePropertyName = DefaultTexturePropertyName;
        [SerializeField] private bool hideFlatDishImageAfterSuccessfulBinding;

        private MaterialPropertyBlock propertyBlock;
        private MaterialPropertyBlock originalPropertyBlock;
        private MeshRenderer appliedRenderer;
        private int appliedPropertyId;
        private bool subscribed;

        public MeshRenderer TargetRenderer => targetRenderer;
        public DishRenderer TextureSource => textureSource;
        public string TexturePropertyName => texturePropertyName;
        public string LastValidationError { get; private set; }
        public bool HasAppliedTexture { get; private set; }

        private void Reset()
        {
            targetRenderer = GetComponent<MeshRenderer>();
            texturePropertyName = DefaultTexturePropertyName;
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
            PreparePropertyBlocks(propertyId);
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetTexture(propertyId, colonyTexture);
            targetRenderer.SetPropertyBlock(propertyBlock);

            HasAppliedTexture = true;
            LastValidationError = null;
            if (hideFlatDishImageAfterSuccessfulBinding && textureSource != null)
                textureSource.SetFlatPresentationVisible(false);
            return true;
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

        private void PreparePropertyBlocks(int propertyId)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            if (originalPropertyBlock == null) originalPropertyBlock = new MaterialPropertyBlock();
            if (appliedRenderer == targetRenderer && appliedPropertyId == propertyId) return;

            ClearAppliedTexture();
            targetRenderer.GetPropertyBlock(originalPropertyBlock);
            appliedRenderer = targetRenderer;
            appliedPropertyId = propertyId;
        }

        private void ClearAppliedTexture()
        {
            if (HasAppliedTexture && appliedRenderer != null && originalPropertyBlock != null)
                appliedRenderer.SetPropertyBlock(originalPropertyBlock);
            appliedRenderer = null;
            appliedPropertyId = 0;
            HasAppliedTexture = false;
        }
    }
}
