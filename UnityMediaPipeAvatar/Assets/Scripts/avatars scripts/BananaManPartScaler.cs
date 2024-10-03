using UnityEngine;
using UnityEngine.UI;

public class BananaManPartScaler : MonoBehaviour
{
    // To quote the main part of Banana Man's Transform
    public Transform body;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;

    // Slider control to control the size of the different sections
    public Slider bodySizeSlider;
    public Slider leftArmSizeSlider;
    public Slider rightArmSizeSlider;
    public Slider leftLegSizeSlider;
    public Slider rightLegSizeSlider;

    // initialisation
    void Start()
    {
        // Set the initial Slider value
        bodySizeSlider.value = body.localScale.x;  // Initialising the body Slider
        leftArmSizeSlider.value = leftArm.localScale.x;  // Initialise the Slider for the left arm
        rightArmSizeSlider.value = rightArm.localScale.x;  // Initialise the Slider for the right arm
        leftLegSizeSlider.value = leftLeg.localScale.x;  // Initialise the Slider for the left leg
        rightLegSizeSlider.value = rightLeg.localScale.x;  // Initialise the Slider for the right leg

        // Bind Slider value change event
        bodySizeSlider.onValueChanged.AddListener(ChangeBodySize);
        leftArmSizeSlider.onValueChanged.AddListener(ChangeLeftArmSize);
        rightArmSizeSlider.onValueChanged.AddListener(ChangeRightArmSize);
        leftLegSizeSlider.onValueChanged.AddListener(ChangeLeftLegSize);
        rightLegSizeSlider.onValueChanged.AddListener(ChangeRightLegSize);
    }

    // Changing body size
    public void ChangeBodySize(float newSize)
    {
        if (body != null)
        {
            body.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // Change left arm size
    public void ChangeLeftArmSize(float newSize)
    {
        if (leftArm != null)
        {
            leftArm.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // Change right arm size
    public void ChangeRightArmSize(float newSize)
    {
        if (rightArm != null)
        {
            rightArm.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // Changing the size of the left leg
    public void ChangeLeftLegSize(float newSize)
    {
        if (leftLeg != null)
        {
            leftLeg.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // Changing the size of the right leg
    public void ChangeRightLegSize(float newSize)
    {
        if (rightLeg != null)
        {
            rightLeg.localScale = new Vector3(newSize, newSize, newSize);
        }
    }
}
