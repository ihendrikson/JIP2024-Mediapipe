using UnityEngine;
using System.Collections.Generic;

public class HumanoidCalculator : MonoBehaviour
{
    public SkinnedMeshRenderer skinnedMeshRenderer; // Assign in the inspector
    public Color targetColor = Color.red;
    public Color defaultColor = Color.white;

    [System.Serializable]
    public class AngleCalculator
    {
        public string name;
        public Transform top;
        public Transform middle;
        public Transform bottom;
        public float minAngle;
        public float maxAngle;

        public AngleCalculator(string name, Transform top, Transform middle, Transform bottom, float minAngle, float maxAngle)
        {
            this.name = name;
            this.top = top;
            this.middle = middle;
            this.bottom = bottom;
            this.minAngle = minAngle;
            this.maxAngle = maxAngle;
        }

        // Calculate angle between top, middle, and bottom points
        public float CalculateAngle()
        {
            if (top && middle && bottom)
            {
                Vector3 upperVec = top.position - middle.position;
                Vector3 lowerVec = bottom.position - middle.position;
                return Vector3.Angle(upperVec, lowerVec);
            }
            return 0f;
        }

        // Check if the angle is within the valid range
        public bool IsAngleInRange(float angle) => angle >= minAngle && angle <= maxAngle;

        // Determine the red intensity based on how far the angle is from the valid range
        public float GetRedIntensity(float angle)
        {
            if (angle < minAngle) return Mathf.Clamp01((minAngle - angle) / 50f);
            if (angle > maxAngle) return Mathf.Clamp01((angle - maxAngle) / 50f);
            return 0f;
        }
    }

    public List<AngleCalculator> angleCalculators = new List<AngleCalculator>();

    void Update()
    {
        ResetAllVertexColors();

        foreach (var angleCalculator in angleCalculators)
        {
            float angle = angleCalculator.CalculateAngle();
            float redIntensity = angleCalculator.GetRedIntensity(angle);
            Color color = Color.Lerp(defaultColor, targetColor, redIntensity);

            // Change color of vertices associated with the top bone
            ChangeColor(angleCalculator.top, color);
        }
    }

    // Reset the colors of all vertices in the skinned mesh
    private void ResetAllVertexColors()
    {
        if (skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
        {
            Color[] vertexColors = new Color[skinnedMeshRenderer.sharedMesh.vertexCount];
            for (int i = 0; i < vertexColors.Length; i++) vertexColors[i] = defaultColor;
            skinnedMeshRenderer.sharedMesh.colors = vertexColors;
        }
    }

    // Change the color of vertices corresponding to a given bone
    private void ChangeColor(Transform targetBone, Color color)
    {
        if (!skinnedMeshRenderer || !targetBone) return;

        Mesh mesh = skinnedMeshRenderer.sharedMesh;
        if (mesh == null) return;

        int boneIndex = GetBoneIndex(targetBone);
        if (boneIndex == -1) return;

        BoneWeight[] boneWeights = mesh.boneWeights;
        Color[] vertexColors = mesh.colors;

        for (int i = 0; i < boneWeights.Length; i++)
        {
            if (boneWeights[i].boneIndex0 == boneIndex) vertexColors[i] = color;
        }

        mesh.colors = vertexColors;
    }

    // Get the index of the bone in the skinned mesh renderer
    private int GetBoneIndex(Transform bone)
    {
        Transform[] bones = skinnedMeshRenderer.bones;
        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i] == bone) return i;
        }
        return -1;
    }
}
