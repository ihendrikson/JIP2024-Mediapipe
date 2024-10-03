using UnityEngine;

public class HumanoidController : MonoBehaviour
{
    public GameObject originalModel; // Assign in the inspector
    public Transform upperLeg; // Assign in the inspector
    public Transform lowerLeg; // Assign in the inspector
    public Transform foot; // Assign in the inspector
    public float targetKneeAngle = 90f; // Adjust in the inspector

    private GameObject clonedModel;

    void Start()
    {
        // Check if all necessary components are assigned
        if (originalModel == null || upperLeg == null || lowerLeg == null || foot == null)
        {
            Debug.LogError("Please assign all necessary components in the inspector.");
            return;
        }

        // Check if a clone already exists
        if (clonedModel == null)
        {
            // Clone the model
            clonedModel = CloneModel(originalModel);

            // Set the knee angle to the target angle
            SetKneeAngle(clonedModel, targetKneeAngle);
        }
    }

    public GameObject CloneModel(GameObject originalModel)
    {
        GameObject clone = Instantiate(originalModel);
        // Remove this script from the clone to prevent further cloning
        Destroy(clone.GetComponent<HumanoidController>());

        // Place the clone 10 units to the right
        clone.transform.position = originalModel.transform.position + new Vector3(10, 0, 0);

        return clone;
    }

    public void SetKneeAngle(GameObject model, float targetAngle)
    {
        // Find the corresponding transforms in the cloned model
        Transform clonedUpperLeg = model.transform.Find(upperLeg.name);
        Transform clonedLowerLeg = model.transform.Find(lowerLeg.name);
        Transform clonedFoot = model.transform.Find(foot.name);

        if (clonedUpperLeg == null || clonedLowerLeg == null || clonedFoot == null)
        {
            Debug.LogError("Could not find the corresponding transforms in the cloned model.");
            return;
        }

        // Calculate the direction vectors
        Vector3 upperLegDirection = clonedLowerLeg.position - clonedUpperLeg.position;
        Vector3 lowerLegDirection = clonedFoot.position - clonedLowerLeg.position;

        // Calculate the current angle
        float currentAngle = Vector3.Angle(upperLegDirection, lowerLegDirection);

        // Calculate the rotation needed to set the angle to the target angle
        float angleDifference = targetAngle - currentAngle;
        clonedLowerLeg.RotateAround(clonedLowerLeg.position, Vector3.Cross(upperLegDirection, lowerLegDirection), angleDifference);
    }
}
