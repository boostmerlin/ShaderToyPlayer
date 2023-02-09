using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEditor.SceneManagement;

public class GlslToUnityCGEditor : EditorWindow
{
    class ShaderCodeView
    {
        Vector2 scroll;
        public string Text { get; private set; }

        public HashSet<string> Channels { get; } = new HashSet<string>();

        public Dictionary<string, string> ChannelInput { get; } = new Dictionary<string, string>();

        readonly Dictionary<string, int> channelSelectIdx = new Dictionary<string, int>();

        void ExtractChannels()
        {
            Channels.Clear();
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }

            var m = Regex.Matches(Text, ShaderInput.iChannel, RegexOptions.Multiline);
            if (m.Count > 0)
            {
                foreach (Match item in m)
                {
                    Channels.Add(item.Value);
                }
            }
        }

        private string[] getAllPass(IEnumerable<string> passes)
        {
            var l = passes.Where(s => s != RemovableTabBar.Image).ToList();
            l.Sort();
            foreach (var item in Channels)
            {
                if (!ChannelInput.ContainsKey(item))
                {
                    channelSelectIdx[item] = -1;
                }
                else
                {
                    channelSelectIdx[item] = l.IndexOf(ChannelInput[item]);
                }
            }
            List<string> list = new List<string>();
            foreach (var item in ChannelInput)
            {
                if(!Channels.Contains(item.Key))
                {
                    list.Add(item.Key);
                }
            }
            foreach (var item in list)
            {
                ChannelInput.Remove(item);
            }

            return l.ToArray();
        }

        private void DrawChannels(string[] passes)
        {
            GUILayout.Space(10);
            List<List<string>> aa = new List<List<string>> ();
            List<string> temp = null;
            int n = 0;
            var l = Channels.ToList();
            l.Sort();
            foreach (var item in l)
            {
                if (n % 2 == 0)
                {
                    temp = new List<string>();
                    aa.Add(temp);
                }
                temp.Add(item);
                n++;
            }
            foreach(var r in aa)
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var item in r)
                {
                    channelSelectIdx[item] = EditorGUILayout.Popup(item, channelSelectIdx[item], passes, GUILayout.Width(250));
                    EditorGUILayout.Space(20);
                    if (channelSelectIdx[item] >= 0)
                    {
                        ChannelInput[item] = passes[channelSelectIdx[item]];
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        public void DrawCodeView(GlslToUnityCGEditor editor)
        {
            var h = editor.position.height * 0.82f;
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(h));
            EditorGUI.BeginChangeCheck();
            Text = EditorGUILayout.TextArea(Text, GUILayout.MinHeight(h));
            if (EditorGUI.EndChangeCheck())
            {
                ExtractChannels();
            }
            EditorGUILayout.EndScrollView();

            //draw channels:
            var passes = getAllPass(editor.tabViews.Keys);
            DrawChannels(passes);
         }
    }

    class Config
    {
        public const string OutputPath = "outputPath";
        public const string DefaultPath = "Assets/Example";
    }
    [MenuItem("ShaderToy/Converter")]
    static void Initialize()
    {
        GlslToUnityCGEditor window = (GlslToUnityCGEditor)GetWindow(typeof(GlslToUnityCGEditor), true, "Shader Converter");
        window.maxSize = new Vector2(1024, 768);
        window.minSize = new Vector2(1024, 768);
    }

    string outputPath;
    string _assetName;
    string assetName
    {
        get
        {
            return _assetName?.Trim();
        }
        set
        {
            _assetName = value;
        }
    }
    readonly RemovableTabBar removableTabBar = new RemovableTabBar();
    string currentTab;
    Dictionary<string, ShaderCodeView> tabViews = new Dictionary<string, ShaderCodeView>();
    string lastTab = "";

    void Awake()
    {
        outputPath = EditorPrefs.GetString(Config.OutputPath, Config.DefaultPath);
        tabViews.Add(RemovableTabBar.Image, new ShaderCodeView());
    }

    private void OnDestroy()
    {
        tabViews.Clear();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Path:", GUILayout.Width(100));
        outputPath = EditorGUILayout.TextField(outputPath);
        EditorGUILayout.LabelField("Shader Name:", GUILayout.Width(100));
        assetName = EditorGUILayout.TextField(assetName);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Convert"))
        {
            if(string.IsNullOrEmpty(assetName))
            {
                EditorUtility.DisplayDialog("Warning!",
                               "shader name can't be empty."
                , "ok");
            }
            CreateShaderDemoScene();

            if (!outputPath.Equals(Config.DefaultPath))
            {
                EditorPrefs.SetString(Config.OutputPath, outputPath);
            }
        }

        currentTab = removableTabBar.DrawTabBar();
        if(currentTab != lastTab)
        {
            GUIUtility.keyboardControl = 0;
            lastTab = currentTab;
        }
    
        if(tabViews.ContainsKey(currentTab))
        {
            tabViews[currentTab].DrawCodeView(this);
        }
        else
        {
            var view = new ShaderCodeView();
            tabViews.Add(currentTab, view);
            view.DrawCodeView(this);
        }
    }

    string GetAssetName(string assetName, string tabName, string ext)
    {
        if(string.IsNullOrEmpty(tabName))
        {
            return assetName + ext;
        }
        else
        {
            return assetName + "  " + tabName + ext;
        }
    }

    string GetAssetPath(string path, string shaderName, string tabName, string ext)
    {
        return Path.Combine(path, GetAssetName(shaderName, tabName, ext));
    }

    Material CreateShaderAndMaterial(string path, string passName, string assetName, string shaderFile)
    {
        var name = GetAssetName(assetName, passName, "");
        var shaderName = "Shadertoy/" + name;
        var glsl = new GlslToUnityCG(tabViews[passName].Text, shaderName);
        glsl.Convert();
        glsl.ToFile(shaderFile);

        //create material
        AssetDatabase.Refresh();
        var shader = Shader.Find(shaderName);
        if (shader == null)
        {
            Debug.LogError("no shader found: " + shaderName);
            return null;
        }
        var material = new Material(shader);
        AssetDatabase.CreateAsset(material, GetAssetPath(path, assetName, passName, ".mat"));
        return material;
    }

    void CreateShaderDemoScene()
    {
        foreach(var tv in tabViews)
        {
            if(string.IsNullOrWhiteSpace(tv.Value.Text))
            {
                EditorUtility.DisplayDialog("Warning!",
                               $"Tab [{tv.Key}] has NO shader code."
                , "ok");
                return;
            }
        }

        string path = Path.Combine(outputPath, assetName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        //create scene
        var scenPath = GetAssetPath(path, assetName, "", ".unity");
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        GameObject mainCamera = newScene.GetRootGameObjects()[0];

        Dictionary<string, Material> materialMap = new Dictionary<string, Material>();
        foreach (var tv in removableTabBar.Tabs.Values)
        {
            Debug.Log("create shader on tab: " + tv);
            var shaderFile = GetAssetPath(path, assetName, tv, ".shader");
            if (File.Exists(shaderFile))
            {
                var replace = EditorUtility.DisplayDialog("Warning",
                                   "Shader already exists."
                    , "Replace", "No Replace");

                if (!replace)
                {
                    return;
                }
            }

            var material = CreateShaderAndMaterial(path, tv, assetName, shaderFile);
            materialMap.Add(tv, material);

            if (tv == RemovableTabBar.Image)
            {
                var renderer = mainCamera.AddComponent<ShaderToyRenderer>();
                renderer.material = material;
            }
            else
            {
                var renderer = mainCamera.AddComponent<ShaderToyTextureRenderer>();
                renderer.material = material;
                //create render texture
                var rt = new CustomRenderTexture(512, 512);
                AssetDatabase.CreateAsset(rt, GetAssetPath(path, assetName, tv, ".renderTexture"));
            }
        }

        AssetDatabase.Refresh();

        //set channel info
        foreach (var item in tabViews)
        {
            string passName = item.Key;
            if(!materialMap.ContainsKey(passName))
            {
                return;
            }
            Material m = materialMap[passName];
            foreach (var channelInput in item.Value.ChannelInput)
            {
                if(m.HasProperty(channelInput.Key))
                {
                    Debug.Log($"material {m.name} has Channel: {channelInput.Key}");
                    var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(GetAssetPath(path, assetName, channelInput.Value, ".renderTexture"));
                    m.SetTexture(channelInput.Key, rt);
                }
            }
        }

        EditorSceneManager.SaveScene(newScene, scenPath);
        AssetDatabase.Refresh();
    }
}
