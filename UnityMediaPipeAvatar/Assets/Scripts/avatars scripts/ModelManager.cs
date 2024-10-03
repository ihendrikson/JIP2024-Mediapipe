using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ModelManager : MonoBehaviour
{
    public string dummyFolder = "Assets/dummy";  // Specify the dummy folder path
    private List<GameObject> modelPrefabs;  // List of model prefabs
    private GameObject currentModel;  // The current model instance

    private BananaManPartScaler currentPartScaler;
    private BananaManSkinChanger currentSkinChanger;

    // UI Controls
    public Slider bodySizeSlider;
    public Slider leftArmSizeSlider;
    public Slider rightArmSizeSlider;
    public Slider leftLegSizeSlider;
    public Slider rightLegSizeSlider;

    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    public Button nextSkinButton;
    public Button previousSkinButton;

    public Button nextModelButton;
    public Button previousModelButton;

    private int currentModelIndex = 0;

    void Start()
    {
        // Load all model prefabs
        LoadModelsFromDummyFolder();

        // Load the initial model
        LoadModel(currentModelIndex);

        // Bind model toggle buttons
        nextModelButton.onClick.AddListener(SwitchToNextModel);
        previousModelButton.onClick.AddListener(SwitchToPreviousModel);

        // Bind skin toggle buttons
        nextSkinButton.onClick.AddListener(OnNextSkinButtonClicked);
        previousSkinButton.onClick.AddListener(OnPreviousSkinButtonClicked);
    }

    // Load all prefabs from the dummy folder
    void LoadModelsFromDummyFolder()
    {
        modelPrefabs = new List<GameObject>();

        // Ensure that the path is correct to load the prefab from the Resources/dummy folder
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("dummy");

        if (loadedPrefabs.Length == 0)
        {
            Debug.LogError("No model prefabs found in Resources/dummy folder!");
            return;
        }

        // Debug log loaded prefabs
        foreach (var prefab in loadedPrefabs)
        {
            Debug.Log("Loaded model prefab: " + prefab.name);
            modelPrefabs.Add(prefab);
        }
    }

    // Switch to the next model
    void SwitchToNextModel()
    {
        currentModelIndex = (currentModelIndex + 1) % modelPrefabs.Count;
        LoadModel(currentModelIndex);
    }

    // Switch to the previous model
    void SwitchToPreviousModel()
    {
        currentModelIndex = (currentModelIndex - 1 + modelPrefabs.Count) % modelPrefabs.Count;
        LoadModel(currentModelIndex);
    }

    // Load the specified model and rebind Part Scaler and Skin Changer
    void LoadModel(int modelIndex)
    {
        if (currentModel != null)
        {
            Destroy(currentModel);  // Destroy the current model
        }

        // Instantiate a new model
        currentModel = Instantiate(modelPrefabs[modelIndex], transform);

        // Get the Part Scaler and Skin Changer components of the model
        currentPartScaler = currentModel.GetComponent<BananaManPartScaler>();
        currentSkinChanger = currentModel.GetComponent<BananaManSkinChanger>();

        // Bind sliders to Part Scaler
        if (currentPartScaler != null)
        {
            BindPartScalerSliders();
        }
        else
        {
            Debug.LogError("BananaManPartScaler component not found!");
        }

        // Bind sliders to Skin Changer
        if (currentSkinChanger != null)
        {
            BindSkinChangerSliders();
            currentSkinChanger.ChangeSkinStyle(0);  // Initial setup skin
        }
        else
        {
            Debug.LogError("BananaManSkinChanger component not found!");
        }
    }

    // Bind sliders for joint scaling
    void BindPartScalerSliders()
    {
        bodySizeSlider.onValueChanged.RemoveAllListeners();
        leftArmSizeSlider.onValueChanged.RemoveAllListeners();
        rightArmSizeSlider.onValueChanged.RemoveAllListeners();
        leftLegSizeSlider.onValueChanged.RemoveAllListeners();
        rightLegSizeSlider.onValueChanged.RemoveAllListeners();

        // Set the initial values for the sliders
        bodySizeSlider.value = currentPartScaler.body.localScale.x;
        leftArmSizeSlider.value = currentPartScaler.leftArm.localScale.x;
        rightArmSizeSlider.value = currentPartScaler.rightArm.localScale.x;
        leftLegSizeSlider.value = currentPartScaler.leftLeg.localScale.x;
        rightLegSizeSlider.value = currentPartScaler.rightLeg.localScale.x;

        // Bind slider value changes to Part Scaler functions
        bodySizeSlider.onValueChanged.AddListener(currentPartScaler.ChangeBodySize);
        leftArmSizeSlider.onValueChanged.AddListener(currentPartScaler.ChangeLeftArmSize);
        rightArmSizeSlider.onValueChanged.AddListener(currentPartScaler.ChangeRightArmSize);
        leftLegSizeSlider.onValueChanged.AddListener(currentPartScaler.ChangeLeftLegSize);
        rightLegSizeSlider.onValueChanged.AddListener(currentPartScaler.ChangeRightLegSize);
    }

    void ColorChange(float value)
    {
        currentSkinChanger.OnColorSliderChanged(redSlider.value, greenSlider.value, blueSlider.value);
    }

    // Bind sliders for skin and color adjustments
    void BindSkinChangerSliders()
    {
        redSlider.onValueChanged.RemoveAllListeners();
        greenSlider.onValueChanged.RemoveAllListeners();
        blueSlider.onValueChanged.RemoveAllListeners();

        // Set the initial values for the sliders
        redSlider.value = 0;
        greenSlider.value = 0;
        blueSlider.value = 0;

        // Bind slider value changes to Skin Changer functions
        redSlider.onValueChanged.AddListener(ColorChange);
        greenSlider.onValueChanged.AddListener(ColorChange);
        blueSlider.onValueChanged.AddListener(ColorChange);
    }

    // Switch to the next skin
    public void OnNextSkinButtonClicked()
    {
        if (currentSkinChanger != null)
        {
            currentSkinChanger.OnNextSkinButtonClicked();
        }
    }

    // Switch to the previous skin
    public void OnPreviousSkinButtonClicked()
    {
        if (currentSkinChanger != null)
        {
            currentSkinChanger.OnPreviousSkinButtonClicked();
        }
    }
}
