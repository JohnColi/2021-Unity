using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 自訂數字美術圖的工具
/// </summary>
public class CustomSpriteFontEditorWindow : EditorWindow
{
    Font _font;
    Texture2D _texture2D;
    Sprite sp;
    string fontPath = "_Sources/Font";
    const string itemName = "Tools/Custom Font Tool/";

    int _advance;
    float _minX;
    float _maxX;
    float _minY;
    float _maxY;

    bool changeAdvance;
    bool changeMinX;
    bool changeMaxX;
    bool changeMinY;
    bool changeMaxY;

    [MenuItem(itemName + "Custom Font Window")]
    static void Init()
    {
        CustomSpriteFontEditorWindow window = (CustomSpriteFontEditorWindow)EditorWindow.GetWindow(typeof(CustomSpriteFontEditorWindow));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Custom Font Window", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("要製作字型的圖片", EditorStyles.boldLabel);
        var te = EditorGUILayout.GetControlRect(GUILayout.Width(90), GUILayout.Height(55));
        _texture2D = (Texture2D)EditorGUI.ObjectField(te, _texture2D, typeof(Texture2D), false);
        GUILayout.EndHorizontal();

        //sp = (Sprite)EditorGUI.ObjectField(te, _texture2D, typeof(Sprite), false);

        GUILayout.Label("資料夾路徑 " + fontPath, EditorStyles.boldLabel);
        //fontPath = GUILayout.TextArea(fontPath);

        GUILayout.Space(1);
        if (GUILayout.Button("開始製作自定義字型"))
        {
            Debug.Log("開始製作字型檔");
            CreatFont(_texture2D, fontPath);
        }

        GUILayout.Space(10);

        //----------
        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal();
        GUILayout.Label("要修改的自定義字型", EditorStyles.boldLabel);
        _font = (Font)EditorGUILayout.ObjectField(_font, typeof(Font), true);
        GUILayout.EndHorizontal();

        changeAdvance = EditorGUILayout.Toggle("Change changeAdvance", changeAdvance);
        if (changeAdvance)
            _advance = EditorGUILayout.IntField("Advance", _advance);
        GUILayout.Space(5);

        changeMinX = EditorGUILayout.Toggle("Change minX", changeMinX);
        if (changeMinX)
            _minX = EditorGUILayout.FloatField("minX", _minX);
        GUILayout.Space(5);

        changeMaxX = EditorGUILayout.Toggle("Change maxX", changeMaxX);
        if (changeMaxX)
            _maxX = EditorGUILayout.FloatField("maxX", _maxX);
        GUILayout.Space(5);

        changeMinY = EditorGUILayout.Toggle("Change minY", changeMinY);
        if (changeMinY)
            _minY = EditorGUILayout.FloatField("minY", _minY);
        GUILayout.Space(5);

        changeMaxY = EditorGUILayout.Toggle("Change maxY", changeMaxY);
        if (changeMaxY)
            _maxY = EditorGUILayout.FloatField("maxY", _maxY);

        if (GUILayout.Button("修改自定義字型"))
        {
            Debug.Log("修改字型檔");
            ChangeAdvance(_font);
        }

        GUILayout.EndVertical();
    }

    [MenuItem(itemName + "Create Font")]
    private static void CreatFont(Texture2D _texture2D, string targetPath)
    {
        if (_texture2D == null)
        {
            Debug.LogError("No selected object or sharding atlas");
            return;
        }

        var filePath = Path.Combine(Application.dataPath, targetPath);
        if (!File.Exists(filePath))
        {
            Debug.LogError("Not find path : " + filePath);
            Directory.CreateDirectory(filePath);
        }

        string selectionPath = AssetDatabase.GetAssetPath(_texture2D);
        if (selectionPath.Contains("Resources"))
        {
            string selectionExt = Path.GetExtension(selectionPath);
            if (selectionExt.Length == 0)
            {
                Debug.LogError("Not get extension");
                return;
            }

            string fontPathName = string.Format("Assets/{0}/{1}.fontsettings", targetPath, _texture2D.name);
            string matPathName = string.Format("Assets/{0}/{1}.mat", targetPath, _texture2D.name);

            //抓指定資料夾的所有subSprite
            string spName = _texture2D.name + selectionExt;
            Debug.Log(selectionPath);

            string loadPath = selectionPath.Replace("Assets/Resources/", "");
            Debug.Log(loadPath);
            loadPath = loadPath.Replace(spName, "");
            Debug.Log(loadPath);
            Sprite[] sprites = Resources.LoadAll<Sprite>(loadPath);

            if (sprites.Length > 0)
            {
                //material
                Material mat = new Material(Shader.Find("GUI/Text Shader"));
                mat.SetTexture("_MainTex", _texture2D);
                AssetDatabase.CreateAsset(mat, matPathName);

                //font
                Font font = new Font();
                font.material = mat;
                AssetDatabase.CreateAsset(font, fontPathName);

                CharacterInfo[] characterInfo = new CharacterInfo[sprites.Length];
                float lineSpace = 0.1f;
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].rect.height > lineSpace)
                    {
                        lineSpace = sprites[i].rect.height;
                    }
                }

                for (int i = 0; i < sprites.Length; i++)
                {
                    Sprite spr = sprites[i];
                    CharacterInfo info = new CharacterInfo();

                    if (!int.TryParse(spr.name, out info.index))
                    {
                        Debug.LogErrorFormat("{0} cant parse to Int!", spr.name);
                        info.index = -1;
                        break;
                    }
                    info.index += 48;   //for number

                    Rect rect = spr.rect;
                    float pivot = spr.pivot.y / rect.height - 0.5f;
                    if (pivot > 0)
                    {
                        pivot = -lineSpace / 2 - spr.pivot.y;
                    }
                    else if (pivot < 0)
                    {
                        pivot = -lineSpace / 2 + rect.height - spr.pivot.y;
                    }
                    else
                    {
                        pivot = -lineSpace / 2;
                    }

                    int offsetY = (int)(pivot + (lineSpace - rect.height) / 2);
                    //設定字元對映到材質上的座標  
                    info.uvBottomLeft = new Vector2((float)rect.x / _texture2D.width, (float)(rect.y) / _texture2D.height);
                    info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / _texture2D.width, (float)(rect.y) / _texture2D.height);
                    info.uvTopLeft = new Vector2((float)rect.x / _texture2D.width, (float)(rect.y + rect.height) / _texture2D.height);
                    info.uvTopRight = new Vector2((float)(rect.x + rect.width) / _texture2D.width, (float)(rect.y + rect.height) / _texture2D.height);
                    //設定字元頂點的偏移位置和寬高  
                    info.minX = 0;
                    info.minY = -(int)rect.height - offsetY;
                    info.maxX = (int)rect.width;
                    info.maxY = -offsetY;
                    //設定字元的寬度  
                    info.advance = (int)rect.width;
                    characterInfo[i] = info;
                }

                font.characterInfo = characterInfo;
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("Max Height：" + lineSpace + "  Prefect Height：" + (lineSpace + 2));
            }
            else
            {
                Debug.Log("Sprite must be placed in the Resources folder and selected");
            }
        }
        else
        {
            Debug.LogError("path not contain Resources");
        }
    }

    private void ChangeAdvance(Font font)
    {
        if (font == null)
        {
            Debug.LogError("No selected object or sharding atlas");
            return;
        }

        CharacterInfo[] characterInfo = font.characterInfo;
        for (int i = 0; i < characterInfo.Length; i++)
        {
            if (changeAdvance)
                characterInfo[i].advance = _advance;
            if (changeMinX)
                characterInfo[i].minX = (int)_minX;
        }

        font.characterInfo = characterInfo;
        EditorUtility.SetDirty(font);
    }
}