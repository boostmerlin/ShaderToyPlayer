using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public enum PropType 
{ 
    Int, Texture, Texture3D, Range, Float, Vector, Color 
}

public class GlslToUnityCG
{
    private readonly string shaderContent;
    private readonly string shaderName;
    private string shaderTemplate;

    private struct PropInfo
    {
        public PropType propType;
        //public List<object> infos;
        //public string desc;

        public PropInfo(PropType type)
        {
            propType = type;
            //desc = null;
            //infos = new List<object>();
        }
    }

    private readonly Dictionary<string, PropInfo> props = new Dictionary<string, PropInfo>();

    public GlslToUnityCG(string shaderContent, string shaderName)
    {
        this.shaderName = shaderName;
        this.shaderContent = shaderContent;
        shaderTemplate = File.ReadAllText("Assets/Shaders/ShaderToyTemplate.shader");

        extractShaderInput();
    }

    private void extractShaderInput()
    {
        //mouse position
        if (shaderContent.Contains(ShaderInput.iMouse))
        {
            var p = new PropInfo(PropType.Vector);
            props.Add(ShaderInput.iMouse, p);
        }
        //channels
        var m = Regex.Match(shaderContent, ShaderInput.iChannel);
        if (m.Success)
        {
            var p = new PropInfo(PropType.Texture);
            props.Add(m.Groups[1].Value, p);
        }
    }

    public override string ToString()
    {
        return shaderTemplate;
    }

    public void Convert()
    {
        //shader name
        Replace("Shadertoy/Template", shaderName);

        //var headers = Regex.Match(shaderContent, @"(.*)(?=void\s+mainImage)", RegexOptions.Multiline | RegexOptions.Singleline);
        //Replace("//HEADERS", headers.Groups[1].Value);

        ////main image
        //var mainImage = Regex.Match(shaderContent, @"void\s+mainImage.+?\{(.+)\}", RegexOptions.Multiline | RegexOptions.Singleline);

        //var mainImageContent = mainImage.Groups[1].Value;

        //var mainImageComponents = Regex.Match(shaderContent, @"void\s+mainImage\s*\(\s*out\s*vec4\s*(.+?)\s*\,.*vec2\s*(.+?)\s*\)", RegexOptions.Multiline);
        //var fragColor = mainImageComponents.Groups[1].Value;
        //var fragCoord = mainImageComponents.Groups[2].Value;

        //mainImageContent = Regex.Replace(mainImageContent, fragColor, "fragColor");
        //mainImageContent = Regex.Replace(mainImageContent, fragCoord, "fragCoord");


        //Replace(@"(?<=vec4 mainImage\(vec2 fragCoord\) {\s+)return.*;(?=\s+})", mainImageContent);
        //Replace(@"fragColor\s*=\s?", "return ");

        Replace(@"void mainImage\(out vec4 fragColor, in vec2 fragCoord\) {.*?}", shaderContent, RegexOptions.Singleline);

        Replace(@"vec3\(([^(;,]+?)\)", "vec3($1,$1,$1)");
        Replace(@"vec4\(([^(;,]+?)\)", "vec4($1,$1,$1,$1)");

        //texture is overloaded, so may also have a lod
        Replace("texture", "tex2D");
        Replace("tex2DLod", "tex2Dlod");
        Replace(@"tex2D\(([^,]+)\,\s*vec2\(([^,].+)\)\,(.+)\)", "tex2Dlod($1,vec4($2,vec2($3,$3)))");
        Replace(@"(tex2Dlod\()([^,]+\,)([^)]+\)?[)]+.+(?=\)))", "$1$2float4($3,0)");

        foreach (var p in props)
        {
            DeclareProperties(p.Key, p.Value.propType);
        }
    }

    public void ToFile(string fullFilePath)
    {
        File.WriteAllText(fullFilePath, shaderTemplate);
    }
    private void Replace(string pattern, string replacement)
    {
        shaderTemplate = Regex.Replace(shaderTemplate, pattern, replacement);
    }

    void Replace(string pattern, string replacement, RegexOptions options)
    {
        shaderTemplate = Regex.Replace(shaderTemplate, pattern, replacement, options);
    }

    void DeclareProperties(string name, PropType type)
    {
        string shaderType;
        string initialize;
        string variableType;
        switch (type)
        {
            case PropType.Float:
                shaderType = "float";
                variableType = "float";
                initialize = "0";
                break;
            case PropType.Texture:
                shaderType = "2D";
                variableType = "sampler2D";
                initialize = "\"white\" {}";
                break;
            case PropType.Color:
                shaderType = "Color";
                variableType = "float4";
                initialize = "(0,0,0,0)";
                break;
            case PropType.Vector:
                shaderType = "Vector";
                variableType = "float4";
                initialize = "(0,0,0,0)";
                break;
            default:
                shaderType = "int";
                initialize = "0";
                variableType = "int";
                break;
        }
        string propDeclare = $"{name} (\"{name}\", {shaderType}) = {initialize}";

        Replace(@"(\s+)(//PROPERTIES)", "$1$2$1" + propDeclare);

        string variableDeclare = $"{variableType} {name};";
        Replace(@"(\s+)(//VARIABLES)", "$1$2$1" + variableDeclare);
    }
}
