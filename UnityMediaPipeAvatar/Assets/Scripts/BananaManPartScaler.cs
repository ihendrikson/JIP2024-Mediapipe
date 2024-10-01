using UnityEngine;
using UnityEngine.UI;

public class BananaManPartScaler : MonoBehaviour
{
    // 引用Banana Man的主要部分Transform
    public Transform body;
    public Transform leftArm;
    public Transform rightArm;
    public Transform leftLeg;
    public Transform rightLeg;

    // Slider控件，用于控制不同部分的大小
    public Slider bodySizeSlider;
    public Slider leftArmSizeSlider;
    public Slider rightArmSizeSlider;
    public Slider leftLegSizeSlider;
    public Slider rightLegSizeSlider;

    // 初始化
    void Start()
    {
        // 设置初始的Slider数值
        bodySizeSlider.value = body.localScale.x;  // 初始化身体的Slider
        leftArmSizeSlider.value = leftArm.localScale.x;  // 初始化左臂的Slider
        rightArmSizeSlider.value = rightArm.localScale.x;  // 初始化右臂的Slider
        leftLegSizeSlider.value = leftLeg.localScale.x;  // 初始化左腿的Slider
        rightLegSizeSlider.value = rightLeg.localScale.x;  // 初始化右腿的Slider

        // 绑定Slider值变化事件
        bodySizeSlider.onValueChanged.AddListener(ChangeBodySize);
        leftArmSizeSlider.onValueChanged.AddListener(ChangeLeftArmSize);
        rightArmSizeSlider.onValueChanged.AddListener(ChangeRightArmSize);
        leftLegSizeSlider.onValueChanged.AddListener(ChangeLeftLegSize);
        rightLegSizeSlider.onValueChanged.AddListener(ChangeRightLegSize);
    }

    // 改变身体大小
    public void ChangeBodySize(float newSize)
    {
        if (body != null)
        {
            body.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // 改变左臂大小
    public void ChangeLeftArmSize(float newSize)
    {
        if (leftArm != null)
        {
            leftArm.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // 改变右臂大小
    public void ChangeRightArmSize(float newSize)
    {
        if (rightArm != null)
        {
            rightArm.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // 改变左腿大小
    public void ChangeLeftLegSize(float newSize)
    {
        if (leftLeg != null)
        {
            leftLeg.localScale = new Vector3(newSize, newSize, newSize);
        }
    }

    // 改变右腿大小
    public void ChangeRightLegSize(float newSize)
    {
        if (rightLeg != null)
        {
            rightLeg.localScale = new Vector3(newSize, newSize, newSize);
        }
    }
}
