using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(ShaderToyRenderer))]
public class DebugShaderProperty : MonoBehaviour
{
    private Material _material;

    public string[] inspectPropertyNames;

    // Use this for initialization
    public void showDebugInfo()
    {
        if (inspectPropertyNames == null) return;

        var m = _material;
        Shader s = m.shader;
        Debug.Log("<b>Material : </b>" + m.name + " <b>use shader: </b>" + s.name);
        foreach (var name in inspectPropertyNames)
        {
            if (m.HasProperty(name))
            {
                var v = m.GetVector(name);

                Debug.LogFormat("Property: {0} Value: {1}", name, v);
            }
        }
    }

    private void checkAndShowDebugInfo()
    {
        var r = GetComponent<ShaderToyRenderer>();
        if (r.RenderMaterial != _material)
        {
            _material = r.RenderMaterial;
            showDebugInfo();
        }
    }

    void Update()
    {
        checkAndShowDebugInfo();
    }
}