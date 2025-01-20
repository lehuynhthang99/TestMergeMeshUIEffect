using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetGlobalShaderProp : MonoBehaviour
{
    [SerializeField] private List<Color> _colors;
    // Start is called before the first frame update
    void Awake()
    {
        UpdateColor();
    }

    private void OnValidate()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        List<Vector4> clrsArray = new List<Vector4>(_colors.Count);
        foreach (Color clr in _colors)
        {
            clrsArray.Add(new Vector4(clr.r, clr.g, clr.b, clr.a));
        }

        Shader.SetGlobalVectorArray("_TileColors", clrsArray);
    }
}
