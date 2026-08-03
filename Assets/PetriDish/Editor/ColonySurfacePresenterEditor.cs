using System;
using PetriDish.Presentation;
using UnityEditor;
using UnityEngine;

namespace PetriDish.Editor
{
    [CustomEditor(typeof(ColonySurfacePresenter))]
    public sealed class ColonySurfacePresenterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var presenter = (ColonySurfacePresenter)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Alignment Actions", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent(
                        "Auto Centre",
                        "Centre the texture on the target mesh UV0 bounds while preserving scale and flips.")))
                    ApplyAlignmentAction(
                        presenter,
                        "Auto Centre Colony Texture",
                        presenter.AutoCentre);

                if (GUILayout.Button(new GUIContent(
                        "Auto Fit",
                        "Uniformly scale and centre the texture to fill the UV0 footprint without stretching it.")))
                    ApplyAlignmentAction(
                        presenter,
                        "Auto Fit Colony Texture",
                        presenter.AutoFit);

                if (GUILayout.Button(new GUIContent(
                        "Reset Alignment",
                        "Restore scale 1,1, offset 0,0, and disable both flips.")))
                    ApplyAlignmentAction(
                        presenter,
                        "Reset Colony Texture Alignment",
                        presenter.ResetAlignment);
            }

            if (!string.IsNullOrWhiteSpace(presenter.LastValidationError))
                EditorGUILayout.HelpBox(presenter.LastValidationError, MessageType.Warning);
        }

        private void ApplyAlignmentAction(
            ColonySurfacePresenter presenter,
            string undoLabel,
            Func<bool> action)
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(presenter, undoLabel);
            if (!action()) return;

            EditorUtility.SetDirty(presenter);
            serializedObject.Update();
            SceneView.RepaintAll();
        }
    }
}
