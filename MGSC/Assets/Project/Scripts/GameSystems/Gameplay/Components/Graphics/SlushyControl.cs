using UnityEngine;

[ExecuteAlways]
public class SlushyControl : MonoBehaviour
{
    public float RedLiquidAmount
    {
        get => redLiquidAmount;
        set
        {
            redLiquidAmount = value;
            UpdateFeedbacks();
        }
    }

    public Renderer slushyRed;
    public Renderer slushyGreen;
    public Renderer slushyBlue;

    [Range(0f, 1f)]
    public float redLiquidAmount = 0f;

    [Range(0f, 1f)]
    public float greenLiquidAmount = 0f;
    
    [Range(0f, 1f)]
    public float blueLiquidAmount = 0f;

    public string shaderProperty = "_LiquidLevel";

    private void OnValidate()
    {
        UpdateFeedbacks();
    }

    public void UpdateFeedbacks()
    {
        if (slushyRed == null)
            return;

        slushyRed.sharedMaterial.SetFloat(shaderProperty, redLiquidAmount);

        if (slushyGreen == null)
            return;

        slushyGreen.sharedMaterial.SetFloat(shaderProperty, greenLiquidAmount);

        if (slushyBlue == null)
            return;

        slushyBlue.sharedMaterial.SetFloat(shaderProperty, blueLiquidAmount);
    }
}