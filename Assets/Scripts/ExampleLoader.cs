using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleLoader : MonoBehaviour
{
    struct ExampleInfo
    {
        public string name { get; set; }
        public string path { get; set; }
        public string tooltip { get; set; }

        override
        public string ToString()
        {
            return string.Format("{0}-{1}-{2}", name, path, tooltip);
        }

        public ExampleInfo(string name, string path, string tooltip)
        {
            this.name = name;
            this.path = path;
            this.tooltip = tooltip;
        }
    }

    private List<ExampleInfo> exampleInfos = new List<ExampleInfo>();

    private Vector2 scrollViewVector = Vector2.zero;

    private const int COL_COUNT = 3;

    private bool backButton
    {
        get
        {
            return sceneLoaded.IsValid() && sceneLoaded.isLoaded;
        }
    }



    private Scene sceneLoaded;

    private void SetGUIStyle()
    {
        var label = GUI.skin.label;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
    }

    private void Start()
    {
        var paths = AssetDatabase.FindAssets("glob:\"**.unity\"", new string[] { "Assets/Example" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => !p.EndsWith("Main.unity"));
        foreach(var p in paths)
        {
            string name = Path.GetFileNameWithoutExtension(p);
            string tooltipPath = Path.Combine(Path.GetDirectoryName(p), "tooltip.txt");
            string tooltip = "";
            if (File.Exists(tooltipPath))
            {
                TextAsset t = AssetDatabase.LoadAssetAtPath<TextAsset>(tooltipPath);
                tooltip = t.text;
            }

            exampleInfos.Add(new ExampleInfo(name, p, tooltip));
        }
    }

    private Rect center(int y, int w, int h)
    {
        var p = new Vector2((Screen.width - w) / 2, y);
        return new Rect(p, new Vector2(w, h));
    }

    private void GotoScene(ExampleInfo info)
    {
        Scene scene = EditorSceneManager.LoadSceneInPlayMode(info.path, new LoadSceneParameters(LoadSceneMode.Additive));

        StartCoroutine(WaitSceneLoaded(scene));
    }

    IEnumerator WaitSceneLoaded(Scene s)
    {
        yield return new WaitUntil(() => s.isLoaded);

        sceneLoaded = s;
    }

    private void ShowExamle()
    {
        SetGUIStyle();
        GUI.Label(center(5, 200, 30), "点击加载Example下的示例");

        GUILayout.BeginArea(new Rect(10, 30, Screen.width / 2, Screen.height - 50));

        scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, false, true);

        for (int i = 0; i < exampleInfos.Count; i += COL_COUNT)
        {
            GUILayout.BeginHorizontal();
            for (int j = i; j < i + COL_COUNT; j++)
            {
                if (j >= exampleInfos.Count)
                {
                    continue;
                }
                var info = exampleInfos[j];
                if (GUILayout.Button(new GUIContent(info.name, info.tooltip)))
                {
                    GotoScene(info);
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUI.Label(new Rect(Screen.width / 2 + 5, 10, Screen.width / 2, Screen.height - 50), GUI.tooltip);
    }
    
    private void ShowBackButton()
    {
        if(GUILayout.Button("Back"))
        {
            SceneManager.UnloadSceneAsync(sceneLoaded);
        }
    }

    private void OnGUI()
    {
        if(backButton)
        {
            ShowBackButton();
        }
        else
        {
            ShowExamle();
        }
    }
}
