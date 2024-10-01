using UnityEngine;
using UnityEngine.UI;

public class BananaManSkinChanger : MonoBehaviour
{
    // References the SkinnedMeshRenderer component in the Banana Man subobject.
    private SkinnedMeshRenderer bananaManRenderer;

    // Prepare an array for storing skin styles
    private Material[] skinStyles;

    private int currentStyleIndex = 0;

    // Referencing the Next and Previous button components
    public Button nextSkinButton;
    public Button previousSkinButton;

    // Quote RGB Slider
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    public Slider transparencySlider;

    void Start()
    {
        // Find the Banana Man child "Body" and get the SkinnedMeshRenderer component.
        Transform bodyTransform = transform.Find("Body");
        if (bodyTransform != null)
        {
            bananaManRenderer = bodyTransform.GetComponent<SkinnedMeshRenderer>();
        }

        // Make sure you find the SkinnedMeshRenderer component!
        if (bananaManRenderer == null)
        {
            Debug.LogError("Not find SkinnedMeshRenderer in the body！");
            return;
        }

        // Dynamically loads all materials in the Resources folder.
        skinStyles = Resources.LoadAll<Material>("Materials/Skinstyles");

        // Checks if the material was successfully loaded and outputs the number of materials
        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("No material is loaded, make sure the material is stored in the Assets/Resources/Materials/Skinstyles ");
        }
        else
        {
            Debug.Log("Successfully loaded " + skinStyles.Length + " texture");
        }

        // Initialisation sets the first material
        ChangeSkinStyle(0);

        // Binding button click events
        if (nextSkinButton != null)
        {
            nextSkinButton.onClick.AddListener(OnNextSkinButtonClicked);
        }

        if (previousSkinButton != null)
        {
            previousSkinButton.onClick.AddListener(OnPreviousSkinButtonClicked);
        }

        // Binding RGB Slider Events
        if (redSlider != null)
        {
            redSlider.onValueChanged.AddListener(OnColorSliderChanged);
        }

        if (greenSlider != null)
        {
            greenSlider.onValueChanged.AddListener(OnColorSliderChanged);
        }

        if (blueSlider != null)
        {
            blueSlider.onValueChanged.AddListener(OnColorSliderChanged);
        }

        // Binding transparency slider events
        if (transparencySlider != null)
        {
            transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
        }
    }

    // Toggle skin style (Body material only)
    public void ChangeSkinStyle(int styleIndex)
    {
        if (bananaManRenderer != null && skinStyles.Length > styleIndex)
        {
            // Get the current material array, replace the first material (Body), keep the second material (Joints) unchanged.
            Material[] materials = bananaManRenderer.materials;
            materials[0] = skinStyles[styleIndex];  // Modify only the Body material
            bananaManRenderer.materials = materials;  // Re-assigning material arrays
            currentStyleIndex = styleIndex;
        }
    }

    // When the Next button is clicked, switch to the next skin style.
    public void OnNextSkinButtonClicked()
    {
        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("Skin styles array is empty. Please assign materials to the skinStyles array.");
            return;
        }

        int nextStyleIndex = (currentStyleIndex + 1) % skinStyles.Length;
        ChangeSkinStyle(nextStyleIndex);
    }

    // Switch to the previous skin style when the Previous button is clicked
    public void OnPreviousSkinButtonClicked()
    {
        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("Skin styles array is empty. Please assign materials to the skinStyles array.");
            return;
        }

        int previousStyleIndex = (currentStyleIndex - 1 + skinStyles.Length) % skinStyles.Length;
        ChangeSkinStyle(previousStyleIndex);
    }

    // Called when the colour slider changes
    public void OnColorSliderChanged(float value)
    {
        if (bananaManRenderer != null)
        {
            // Get current material array
            Material[] materials = bananaManRenderer.materials;

            // Modify only the colour of the Body material
            float red = redSlider.value;
            float green = greenSlider.value;
            float blue = blueSlider.value;

            Color newColor = new Color(red, green, blue, materials[0].color.a); // Keep transparency constant
            materials[0].color = newColor;
            bananaManRenderer.materials = materials;  // Re-assigning material arrays
        }
    }

    // Called when the transparency slider changes
    public void OnTransparencyChanged(float newAlpha)
    {
        if (bananaManRenderer != null)
        {
            // Get current material array
            Material[] materials = bananaManRenderer.materials;

            // Modifies only the transparency of the Body material.
            Color currentColor = materials[0].color;
            currentColor.a = newAlpha; // Setting transparency
            materials[0].color = currentColor;
            bananaManRenderer.materials = materials;  //Re-assigning material arrays
        }
    }
}
