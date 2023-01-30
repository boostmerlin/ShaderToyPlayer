using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RemovableTabBar
{ 
    public const string BufferA = "Buffer A";
    public const string BufferB = "Buffer B";
    public const string BufferC = "Buffer C";
    public const string BufferD = "Buffer D";
    public const string Image = "Image";

    public readonly Dictionary<string, int> tabBarWeight = new Dictionary<string, int>();

    public SortedList<int, string> Tabs { get; } = new SortedList<int, string>();
    
    public int selectedTab { get; private set; }

    private readonly HashSet<string> popupItems = new HashSet<string>();

    private GUIStyle selectedStyle;

    public RemovableTabBar()
    {
        tabBarWeight.Add(BufferA, 1);
        tabBarWeight.Add(BufferB, 2);
        tabBarWeight.Add(BufferC, 3);
        tabBarWeight.Add(BufferD, 4);

        popupItems.UnionWith(tabBarWeight.Keys);

        tabBarWeight.Add(Image, 10);

        Tabs.Add(tabBarWeight[Image], Image);
    }

    public string DrawTabBar()
    {
        GUILayout.BeginHorizontal();
        DrawDropdown("+");
        DrawTabItem();
        GUILayout.EndHorizontal();
        return SelectedTabItem;
    }

    private void InitStyle()
    {
        selectedStyle = new GUIStyle(GUI.skin.button);
        selectedStyle.normal.textColor = Color.red;
    }

    private void DrawTabItem() 
    {
        InitStyle();

        for (int i = 0; i < Tabs.Keys.Count; i++)
        {
            string item = Tabs[Tabs.Keys[i]];
            bool selected = selectedTab == i;
            var style = selected ? selectedStyle : GUI.skin.button;
            if (GUILayout.Button(item, style, GUILayout.Width(100)))
            {
                selectedTab = i;
            }
            var r = GUILayoutUtility.GetLastRect();
            r.width = 25;
            r.x += 100 - 1;
            if (item != Image && GUI.Button(r, "x", style))
            {
                Tabs.RemoveAt(i);
                popupItems.Add(item); 
                if (selected)
                {
                    selectedTab = Tabs.Count - 1;
                }
                else if(i < selectedTab)
                {
                    selectedTab--;
                }
                break;
            }
            GUILayout.Space(30);
        }
    }

    public void addTab(string name)
    {
        Tabs.Add(tabBarWeight[name], name);
        selectedTab = Tabs.IndexOfValue(name);
        popupItems.Remove(name);
    }

    void handleItemClicked(object parameter)
    {
        addTab(parameter.ToString());
    }

    Rect dropdownRect;
    private void DrawDropdown(string label)
    {
        if (!EditorGUILayout.DropdownButton(new GUIContent(label), FocusType.Keyboard, GUILayout.Height(22), GUILayout.Width(32)))
        {
            if (Event.current.type == EventType.Repaint)
            {
                dropdownRect = GUILayoutUtility.GetLastRect();
            }
            return;
        }

        GenericMenu menu = new GenericMenu();
        foreach(var item in popupItems)
        {
            menu.AddItem(new GUIContent(item), false, handleItemClicked, item);
        }
        menu.DropDown(dropdownRect);
    }

    public string SelectedTabItem
    {
        get
        {
            return Tabs.Values[selectedTab];
        }
    }
}
