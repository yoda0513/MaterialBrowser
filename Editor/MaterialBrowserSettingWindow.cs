using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.UIElements;

public class MaterialBrowserSettingWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private VisualTreeAsset textFieldAsset = null;

    public List<Item> items = new List<Item>();

    public MaterialList materialList = null;

    public void OnDisable()
    {
        materialList.ReloadWindow();
    }

    //[MenuItem("Window/Example/Show Aux Window")]
    public static void ShowWindow()
    {
        MaterialBrowserSettingWindow wnd = CreateInstance<MaterialBrowserSettingWindow>();
        wnd.ShowModal();
        
    }

    public void CreateGUI()
    {
        string path = AssetDatabase.GetAssetPath(m_VisualTreeAsset);
        var packageBasePath = Regex.Match(path, "(.+" + Regex.Escape("/") + ").*?" + Regex.Escape(".") + ".*?$").Groups[1].Value;
        textFieldAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(packageBasePath + "TextField.uxml");

        var optionData = AssetDatabase.LoadAssetAtPath<MaterialBrowserOptionData>(MaterialList.settingdataFolder + "/" + MaterialList.settindataname);
        if (optionData == null)
        {
            optionData = CreateInstance<MaterialBrowserOptionData>();
            AssetDatabase.CreateAsset(optionData, MaterialList.settingdataFolder + "/" + MaterialList.settindataname);
            AssetDatabase.Refresh();
        }

        var serializedObject = new SerializedObject(optionData);

       
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        labelFromUXML.StretchToParentWidth();
        labelFromUXML.StretchToParentSize();


        
        root.Add(labelFromUXML);
        root.Bind(serializedObject);

        //root.Query<ListView>().First().bindingPath = "items";

        root.Query<ListView>().First().makeItem += () => 
        {
            return textFieldAsset.Instantiate();
        };
        
    }
}
