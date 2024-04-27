using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ShaderKeywordFilter;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

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


    //スタートアップ
    [MenuItem("Tools/MaterialBrowser %m")]
    public static void ShowMaterialList()
    {
        MaterialList wnd = GetWindow<MaterialList>();
        wnd.titleContent = new GUIContent("MaterialBrowser");
    }




    //画面生成時に自動で呼ばれる 各種ウィンドウの設定
    public void CreateGUI()
    {
        minSize = new Vector2(200, 200);



        //マテリアルの一覧表を生成
        InitializeWindow();



        //画面上部のツールバーを生成
        var toolbar = new Toolbar();

        var menu = new ToolbarMenu();
        menu.menu.ClearItems();
        menu.text = "Folder";
        menu.name = "ToolberMenuName";
        
        
        toolbar.Add(menu);

        toolbar.Add(new ToolbarSpacer());

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
        //ツールバーのFolder項目をリセット
        SetToolberMenu();

    }


    //オプションを読み込む
    public static MaterialBrowserOptionData LoadoptionData()
    {
        MaterialBrowserOptionData optionData = AssetDatabase.LoadAssetAtPath<MaterialBrowserOptionData>(settingdataFolder + "/" + settindataname);

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

        return optionData;
    }





    private void SetToolberMenu()
    {
        MaterialBrowserOptionData optionData = LoadoptionData();

        var menu = rootVisualElement.Query<ToolbarMenu>().First();
        menu.menu.ClearItems();


        optionData.items.Where(x => x.path != "")
            .Select((x,index) =>
            {
                if ((x.path).Contains("/"))
                {
                    return (Regex.Match(x.path, ".*" + Regex.Escape("/") + "(.*?$)").Groups[1].Value,index);
                }
                else
                {
                    return ("Assets",index);
                }
                
            }).ToList()
            .ForEach(x => 
            {
                menu.menu.AppendAction(x.Item1, action => GenerateMaterialGroupBar(x.index));
            });
    }

    //画面を再構築する。設定画面から戻った際にこれを呼び出す。
    public void ReloadWindow()
    {
        GenerateMaterialGroupBar(-1);
        SetToolberMenu();
    }


    //起動時の画面生成
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
        GenerateMaterialGroupBar(-1);
        
    }


    //マテリアルのリストの生成
    private void GenerateMaterialGroupBar(int index)
    {
        //画面の要素をすべて消す

        MaterialListContent = rootVisualElement.Query("MaterialContent");

        if (MaterialListContent != null)
        {
            while (MaterialListContent.childCount != 0)
            {
                MaterialListContent.RemoveAt(0);
            }
        }

        //ScrollViewの要素をすべて消す
        var groupScrollView = rootVisualElement.Query("GroupScrollview").First();
        if (groupScrollView != null)
        {
            while (groupScrollView.childCount != 0)
            {
                groupScrollView.RemoveAt(0);
            }
        }




        MaterialBrowserOptionData optionData = LoadoptionData();

        //設定ファイルのフォルダリストの読み込み
        string[] diraug = null;
        if (optionData.items.Where(x => x.path != "").Count() == 0) return;

        if(index == -1)
        {
            diraug = new string[] { optionData.items.Where(x => x.path != "").Select(x => x.path).First() };
        }
        else
        {
            diraug = new string[] { optionData.items[index].path }; 
        }

        




        if (diraug == null || diraug.Length == 0) return;
        Label materialLabel = rootVisualElement.Query<Label>("GroupLabel").First();
        
        if (diraug[0].Contains("/"))
        {

            materialLabel.text = Regex.Match(diraug[0], "(.+)" + Regex.Escape("/") + "(.*?$)").Groups[2].Value;
        }
        else
        {
            materialLabel.text = "Assets";
        }


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
                var match = Regex.Match(path, "(.+)" + Regex.Escape("/") + "(.*?$)");
                basepath = match.Groups[1].Value;

            }
            
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
        element.name = MaterialName;

        element.Add(materialRow);        element.Query("Image").First().style.backgroundImage = new StyleBackground(Icon);
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
            Material target = AssetDatabase.LoadAssetAtPath<Material>(path);
            string name = Regex.Match(path, ".+" + Regex.Escape("/") + "(.*?)" + Regex.Escape(".mat")).Groups[1].Value;


            GenerateMaterialItem(icon, name, path);


            //AssetDatabase.in
            
            EditorCoroutine.start(PreviewLoad(rootVisualElement , target.GetInstanceID(), name, path));
            /*
            while (AssetPreview.IsLoadingAssetPreviews())
            {
            }

            */


            
        });
    }









    static IEnumerator PreviewLoad(VisualElement root, int instanceID, string materialname, string path)
    {
        int index = 0;
        while(AssetPreview.IsLoadingAssetPreview(instanceID))
        {
            index++;
            yield return null;
        }

        root.Query(materialname).First().Query("Image").First().style.backgroundImage = new StyleBackground(AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<Material>(path)));


    }
}

public class EditorCoroutine
{
    public static EditorCoroutine start(IEnumerator _routine)
    {
        EditorCoroutine coroutine = new EditorCoroutine(_routine);
        coroutine.start();
        return coroutine;
    }
    readonly IEnumerator routine;
    EditorCoroutine(IEnumerator _routine)
    {
        routine = _routine;
    }
    void start()
    {
        //Debug.Log("start");
        EditorApplication.update += update;
    }
    public void stop()
    {
        //Debug.Log("stop");
        EditorApplication.update -= update;
    }
    void update()
    {
        /* NOTE: no need to try/catch MoveNext,
         * if an IEnumerator throws its next iteration returns false.
         * Also, Unity probably catches when calling EditorApplication.update.
         */
        //Debug.Log("update");
        if (!routine.MoveNext())
        {
            stop();
        }
    }
}


