using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ShaderToyRenderer : MonoBehaviour
{
    public Shader shader;

    public bool followMouse;

    public Material material;


    public Material RenderMaterial
    {
        get
        {
            return CheckShaderAndCreateMaterial(shader, material);
        }
    }

    // Update is called once per frame
    void Update()
    {
        var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f);
        if (!Input.GetMouseButtonDown(0))
        {
            mousePosition.z = 0;
        }
        if (RenderMaterial && (followMouse || !followMouse && Math.Abs(mousePosition.z - 1.0f) < float.Epsilon))
        {
            RenderMaterial.SetVector(ShaderInput.iMouse, mousePosition);
        }
    }

    private Material CheckShaderAndCreateMaterial(Shader shader, Material m)
    {
        //判断当前着色器是否可在当前显卡设备上使用，如果对应的着色器的所有Fallback都不可支持或者传入的着色器就不存在，返回null
        if (m != null || shader == null || !shader.isSupported)
        {
            return m;
        }
        m = new Material(shader)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        material = m;
        return m;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (RenderMaterial != null)
        {
            Graphics.Blit(null, null, RenderMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
