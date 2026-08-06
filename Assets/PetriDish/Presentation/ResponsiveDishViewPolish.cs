using System.Collections;
using PetriDish.Application;
using UnityEngine;
using UnityEngine.UI;

namespace PetriDish.Presentation
{
    /// <summary>
    /// Final presentation pass for the responsive Phase 2 interface.
    /// Reframes the generated dish camera to a locked scientific observation angle
    /// and replaces the competing legacy temperature label with one stable readout.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ResponsiveDishViewPolish : MonoBehaviour
    {
        [Header("Locked dish camera")]
        [SerializeField, Range(15f, 75f)] private float elevationDegrees = 48f;
        [SerializeField, Range(-180f, 180f)] private float azimuthDegrees = -28f;
        [SerializeField, Range(18f, 60f)] private float fieldOfView = 32f;
        [SerializeField, Range(1.2f, 4f)] private float framingDistanceMultiplier = 2.35f;
        [SerializeField, Range(0.8f, 1.4f)] private float targetHeightFactor = 0.96f;

        private ExperimentController controller;
        private Transform temperatureButton;
        private Transform scientificTemperatureController;
        private Text stableTemperatureReadout;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject host = new GameObject("ResponsiveDishViewPolish");
            host.hideFlags = HideFlags.HideAndDontSave;
            host.AddComponent<ResponsiveDishViewPolish>();
        }

        private IEnumerator Start()
        {
            PetriDishResponsiveUIBinder binder = null;
            Camera dishCamera = null;
            ColonySurfacePresenter colonySurface = null;

            for (int frame = 0; frame < 240; frame++)
            {
                binder = FindFirstObjectByType<PetriDishResponsiveUIBinder>();
                colonySurface = FindFirstObjectByType<ColonySurfacePresenter>();

                GameObject cameraObject = GameObject.Find("ResponsiveDishCamera");
                dishCamera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;

                if (binder != null && binder.IsInitialized && dishCamera != null && colonySurface != null)
                    break;

                yield return null;
            }

            if (binder == null)
            {
                Destroy(gameObject);
                yield break;
            }

            controller = FindFirstObjectByType<ExperimentController>();
            BuildStableTemperatureReadout(binder.transform);

            if (dishCamera != null && colonySurface != null)
                ConfigureLockedCamera(dishCamera, colonySurface);
        }

        private void LateUpdate()
        {
            if (stableTemperatureReadout == null || controller == null || controller.Simulation == null)
                return;

            // The binder and the runtime controller both update older labels. Some scene
            // configurations can reactivate those labels after this component starts,
            // producing an alternating TEMPERATURE/TARGET display. Enforce one owner at
            // the end of every frame while preserving the minus and plus button labels.
            DisableCompetingTemperatureLabels();

            stableTemperatureReadout.gameObject.SetActive(true);
            stableTemperatureReadout.transform.SetAsLastSibling();
            stableTemperatureReadout.text = string.Format(
                "TARGET  {0:0.0} °C",
                controller.Simulation.TargetTemperature);
        }

        private void BuildStableTemperatureReadout(Transform uiRoot)
        {
            temperatureButton = FindTransform(uiRoot, "TemperatureButton");
            if (temperatureButton == null)
                return;

            scientificTemperatureController =
                temperatureButton.Find("ScientificTemperatureController");

            Transform existing = temperatureButton.Find("StableTemperatureReadout");
            GameObject readoutObject;
            if (existing != null)
            {
                readoutObject = existing.gameObject;
            }
            else
            {
                readoutObject = new GameObject(
                    "StableTemperatureReadout",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Text));
                readoutObject.transform.SetParent(temperatureButton, false);
            }

            RectTransform rect = readoutObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.56f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(5f, 0f);
            rect.offsetMax = new Vector2(-5f, -1f);

            stableTemperatureReadout = readoutObject.GetComponent<Text>();
            stableTemperatureReadout.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stableTemperatureReadout.fontSize = 11;
            stableTemperatureReadout.resizeTextForBestFit = true;
            stableTemperatureReadout.resizeTextMinSize = 8;
            stableTemperatureReadout.resizeTextMaxSize = 12;
            stableTemperatureReadout.alignment = TextAnchor.MiddleCenter;
            stableTemperatureReadout.color = new Color(0.62f, 0.96f, 1f, 1f);
            stableTemperatureReadout.raycastTarget = false;
            stableTemperatureReadout.horizontalOverflow = HorizontalWrapMode.Overflow;
            stableTemperatureReadout.verticalOverflow = VerticalWrapMode.Truncate;
            stableTemperatureReadout.text = "TARGET  21.0 °C";

            DisableCompetingTemperatureLabels();
            stableTemperatureReadout.gameObject.SetActive(true);
            stableTemperatureReadout.transform.SetAsLastSibling();
        }

        private void DisableCompetingTemperatureLabels()
        {
            if (temperatureButton == null)
                return;

            if (scientificTemperatureController == null)
                scientificTemperatureController =
                    temperatureButton.Find("ScientificTemperatureController");

            Text[] labels = temperatureButton.GetComponentsInChildren<Text>(true);
            foreach (Text label in labels)
            {
                if (label == null || label == stableTemperatureReadout)
                    continue;

                if (IsTemperatureControlButtonLabel(label.transform))
                    continue;

                if (label.gameObject.activeSelf)
                    label.gameObject.SetActive(false);
            }
        }

        private bool IsTemperatureControlButtonLabel(Transform labelTransform)
        {
            if (scientificTemperatureController == null || labelTransform == null)
                return false;
            if (!labelTransform.IsChildOf(scientificTemperatureController))
                return false;

            Transform parent = labelTransform.parent;
            if (parent == null)
                return false;

            return parent.name == "TemperatureMinus" ||
                   parent.name == "TemperaturePlus";
        }

        private void ConfigureLockedCamera(Camera cameraComponent, ColonySurfacePresenter colonySurface)
        {
            Renderer colonyRenderer = colonySurface.GetComponent<Renderer>();
            if (colonyRenderer == null)
                colonyRenderer = colonySurface.GetComponentInChildren<Renderer>(true);
            if (colonyRenderer == null)
                return;

            Bounds bounds = CollectNearbyDishBounds(colonyRenderer);
            float radius = Mathf.Max(bounds.extents.x, bounds.extents.z, bounds.extents.y * 2f);
            radius = Mathf.Max(radius, 0.05f);

            float elevation = elevationDegrees * Mathf.Deg2Rad;
            float azimuth = azimuthDegrees * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(
                Mathf.Cos(elevation) * Mathf.Sin(azimuth),
                Mathf.Sin(elevation),
                Mathf.Cos(elevation) * Mathf.Cos(azimuth));

            Vector3 target = bounds.center + Vector3.up * bounds.extents.y * (targetHeightFactor - 1f);
            float distance = radius * framingDistanceMultiplier / Mathf.Tan(fieldOfView * 0.5f * Mathf.Deg2Rad);

            cameraComponent.orthographic = false;
            cameraComponent.fieldOfView = fieldOfView;
            cameraComponent.nearClipPlane = Mathf.Max(0.001f, distance - radius * 4f);
            cameraComponent.farClipPlane = distance + radius * 8f;
            cameraComponent.transform.position = target + direction.normalized * distance;
            cameraComponent.transform.LookAt(target, Vector3.up);
        }

        private static Bounds CollectNearbyDishBounds(Renderer colonyRenderer)
        {
            Bounds bounds = colonyRenderer.bounds;
            float searchRadius = Mathf.Max(0.08f, Mathf.Max(bounds.size.x, bounds.size.z) * 3f);

            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer candidate in renderers)
            {
                if (candidate == null || !candidate.enabled)
                    continue;
                if (Vector3.Distance(candidate.bounds.center, bounds.center) > searchRadius)
                    continue;

                bounds.Encapsulate(candidate.bounds);
            }

            return bounds;
        }

        private static Transform FindTransform(Transform root, string objectName)
        {
            if (root == null)
                return null;
            if (root.name == objectName)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindTransform(root.GetChild(i), objectName);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
