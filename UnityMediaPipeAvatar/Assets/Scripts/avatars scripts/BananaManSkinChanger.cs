using UnityEngine;
using UnityEngine.UI;

public class BananaManSkinChanger : MonoBehaviour
{
    // Store all found SkinnedMeshRenderer components
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    // Material array for skin styles
    private Material[] skinStyles;

    private int currentStyleIndex = 0;

    // Button references
    public Button nextSkinButton;
    public Button previousSkinButton;

    // Color sliders
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    void Start()
    {
        // Find all SkinnedMeshRenderer components in this object and its children
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // Check if any SkinnedMeshRenderer components were found
        if (skinnedMeshRenderers == null || skinnedMeshRenderers.Length == 0)
        {
            Debug.LogError("No SkinnedMeshRenderer components found!");
            return;
        }

        // Clone the materials for each SkinnedMeshRenderer
        foreach (var renderer in skinnedMeshRenderers)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = new Material(materials[i]);  // Clone the material to ensure each model has a separate copy
            }
            renderer.materials = materials;  // Reassign the cloned materials
        }

        // Dynamically load all materials from the Resources folder
        skinStyles = Resources.LoadAll<Material>("Materials/Skinstyles");

        // Check if materials were successfully loaded and log the count
        if (skinStyles == null || skinStyles.Length == 0)
        {
            Debug.LogError("No materials loaded. Ensure materials are stored in Assets/Resources/Materials/Skinstyles.");
        }
        else
        {
            Debug.Log("Successfully loaded " + skinStyles.Length + " materials");
        }

        // Initialize with the first material
        ChangeSkinStyle(0);

        // Bind button click events
        if (nextSkinButton != null)
        {
            nextSkinButton.onClick.AddListener(OnNextSkinButtonClicked);
        }

        if (previousSkinButton != null)
        {
            previousSkinButton.onClick.AddListener(OnPreviousSkinButtonClicked);
        }

        // Bind RGB slider events
        if (redSlider != null)
        {
            redSlider.onValueChanged.AddListener(ColorChange);
        }

        if (greenSlider != null)
        {
            greenSlider.onValueChanged.AddListener(ColorChange);
        }

        if (blueSlider != null)
        {
            blueSlider.onValueChanged.AddListener(ColorChange);
        }
    }

    void ColorChange(float value)
    {
        OnColorSliderChanged(redSlider.value, greenSlider.value, blueSlider.value);
    }

    // Switch skin style and apply to all SkinnedMeshRenderer components
    public void ChangeSkinStyle(int styleIndex)
    {
        if (skinnedMeshRenderers != null && skinStyles.Length > styleIndex)
        {
            foreach (var renderer in skinnedMeshRenderers)
            {
                // Get the current material array and modify the first material
                Material[] materials = renderer.materials;
                materials[0] = new Material(skinStyles[styleIndex]);  // Clone the material to avoid sharing
                renderer.materials = materials;  // Reassign the material array
            }
            currentStyleIndex = styleIndex;
        }
    }

    // Switch to the next skin style when the button is clicked
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

    // Switch to the previous skin style when the button is clicked
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

    // Apply color changes to all SkinnedMeshRenderer components when the color sliders change
    public void OnColorSliderChanged(float value_red, float value_green, float value_blue)
    {
        if (skinnedMeshRenderers != null)
        {
            foreach (var renderer in skinnedMeshRenderers)
            {
                Material[] materials = renderer.materials;

                if (materials == null || materials.Length == 0)
                {
                    Debug.LogError("SkinnedMeshRenderer has no materials!");
                    continue;
                }

                // Modify the color of the first material
                float red = value_red;
                float green = value_green;
                float blue = value_blue;

                Color newColor = new Color(red, green, blue, materials[0].color.a);  // Retain transparency
                materials[0].color = newColor;
                renderer.materials = materials;  // Reassign the material array
            }
        }
        else
        {
            Debug.LogError("skinnedMeshRenderers is null, cannot adjust color!");
        }
    }
}
