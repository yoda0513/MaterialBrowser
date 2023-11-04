using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MaterialList : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private VisualTreeAsset MaterialGroupButton;
    private VisualTreeAsset MaterialRow;

    private VisualElement MaterialListContent;
    private string packageBasePath;

    public const string settingdataFolder = "Assets/Resources";
    public const string settindataname = "setting.asset";

    [MenuItem("Tools/MaterialList %m")]
    public static void ShowMaterialList()
    {
        MaterialList wnd = GetWindow<MaterialList>();
        wnd.titleContent = new GUIContent("MaterialBrowser");
    }

    public void CreateGUI()
    {
        minSize = new Vector2(200, 200);




        InitializeWindow();

        //
        var toolbar = new Toolbar();

        var btn1 = new ToolbarButton { text = "Option" };
        toolbar.Add(btn1);

        btn1.clicked += () => 
        {

            MaterialBrowserSettingWindow wnd = CreateInstance<MaterialBrowserSettingWindow>();
            wnd.materialList = this;
            wnd.ShowModal();
        };

        var btn2 = new ToolbarButton { text = "Help" };
        toolbar.Add(btn2);


        rootVisualElement.Add(toolbar);

    }

    public void ReloadWindow()
    {
        MaterialListContent = rootVisualElement.Query("MaterialContent");

        if (MaterialListContent != null)
        {
            while (MaterialListContent.childCount != 0)
            {
                MaterialListContent.RemoveAt(0);
            }
        }


        var groupScrollView = rootVisualElement.Query("GroupScrollview").First();
        if (groupScrollView != null)
        {
            while (groupScrollView.childCount != 0)
            {
                groupScrollView.RemoveAt(0);
            }
        }

        GenerateMaterialGroupBar();

    }

    public void InitializeWindow()
    {
        
        


        string path = AssetDatabase.GetAssetPath(m_VisualTreeAsset);
        packageBasePath = Regex.Match(path, "(.+" + Regex.Escape("/") + ").*?" + Regex.Escape(".") + ".*?$").Groups[1].Value;



        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        MaterialGroupButton = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(packageBasePath + "MaterialGroupRow.uxml");
        MaterialRow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(packageBasePath + "MaterialRow.uxml");
        labelFromUXML.StretchToParentWidth();
        labelFromUXML.StretchToParentSize();
        rootVisualElement.Add(labelFromUXML);

        MaterialListContent = rootVisualElement.Query("MaterialContent");
        GenerateMaterialGroupBar();

    }



    private void GenerateMaterialGroupBar()
    {
        
        MaterialBrowserOptionData optionData =  AssetDatabase.LoadAssetAtPath<MaterialBrowserOptionData>(settingdataFolder + "/" + settindataname);
        if (optionData == null)
        {
            if (!Directory.Exists(settingdataFolder))
            {
                Directory.CreateDirectory(settingdataFolder);
                AssetDatabase.ImportAsset(settingdataFolder);
            }


            optionData = CreateInstance<MaterialBrowserOptionData>();
            AssetDatabase.CreateAsset(optionData, settingdataFolder + "/" + settindataname);
            AssetDatabase.Refresh();
        }

        string[] diraug = optionData.items.Where(x => x.path != "").Select(x => x.path).ToArray();
        



        if (diraug.Length == 0) return;

        var textfiles = AssetDatabase.FindAssets("t:folder", diraug).ToList().Where(x => 
        {
            //直下のフォルダのみ取り出す。
            string path = AssetDatabase.GUIDToAssetPath(x);
            string basepath = "";
            
            if (path == "Assets")
            {
                basepath = "Assets";
            }
            else
            {
                basepath = Regex.Match(path, "(.+)" + Regex.Escape("/") + ".*?$").Groups[1].Value;
            }
            
            Debug.Log(basepath);
            return diraug.ToList().Contains(basepath);
            
            
        })
            .Select(x => AssetDatabase.GUIDToAssetPath(x));

        int count = 0;
        textfiles.ToList().Select(x =>
        {
            var match = Regex.Match(x, ".+" + Regex.Escape("/") + "(.*?$)");
            return (match.Groups[0].Value ,match.Groups[1].Value);
        }).ToList().ForEach(x => 
        {
            var MaterialGroupPart = MaterialGroupButton.Instantiate();
            VisualElement element = new VisualElement();
            element.Add(MaterialGroupPart);
            Button button = element.Query<Button>().First();
            button.name = x.Item2;
            button.text = x.Item2;

            void MaterialListInstantiate()
            {
                SetMaterialList(x.Item1);

                rootVisualElement.Query("GroupScrollview").First().Query<Button>().ToList().ForEach(b =>
                {
                    b.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.3f, 0.3f, 0.3f, 1));
                });
                button.style.backgroundColor = new StyleColor(new UnityEngine.Color(0.5f, 0.5f, 0.5f, 1));
            }

            button.clicked += MaterialListInstantiate;
            if (count == 0) MaterialListInstantiate();

            count++;
            rootVisualElement.Query("GroupScrollview").First().Add(element);
            
        });
    }

    private void GenerateMaterialItem(Texture2D Icon,string MaterialName, string path)
    {
        var materialRow = MaterialRow.Instantiate();
        VisualElement element = new VisualElement();
        element.Add(materialRow);

        element.Query("Image").First().style.backgroundImage = new StyleBackground(Icon);
        element.Query<Label>("MaterialName").First().text = MaterialName;
        element.Query<Button>("SelectButton").First().clicked += () => 
        {
            Material target = AssetDatabase.LoadAssetAtPath<Material>(path);
            Selection.objects = new UnityEngine.Object[] { target};
            EditorGUIUtility.PingObject(target);
        };

        element.Query<Button>("CopyPath").First().clicked += () =>
        {
            GUIUtility.systemCopyBuffer = path;
        };

        element.Query<Button>("CopyName").First().clicked += () =>
        {
            GUIUtility.systemCopyBuffer = MaterialName;
        };
        MaterialListContent.Add(element);
    }

    private void SetMaterialList(string path)
    {
        string[] diraug = { path };
        var Materials = AssetDatabase.FindAssets("t:Material", diraug).ToList();

        while(MaterialListContent.childCount > 0)
        {
            MaterialListContent.RemoveAt(0);
        }

        Materials.ForEach(x => 
        {
            
            string path = AssetDatabase.GUIDToAssetPath(x);
            

            var icon = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<Material>(path));
            /*
            while (AssetPreview.IsLoadingAssetPreviews())
            {
            }

            */

            
            string name = Regex.Match(path, ".+" + Regex.Escape("/") + "(.*?)" + Regex.Escape(".mat")).Groups[1].Value;
            

            GenerateMaterialItem(icon, name, path);
        });
    }
}


