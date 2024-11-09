using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class BuildProtoMsgWindow : EditorWindow
{
    [MenuItem("Tools/1. 构建ProtoBuf消息",false,1)]
    public static void ShowExample()
    {
        BuildProtoMsgWindow wnd = GetWindow<BuildProtoMsgWindow>();
        wnd.titleContent = new GUIContent("BuildProtoMsgWindow");
    }
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    #region

    private TextField unityPath;
    private TextField serverPath;
    private Button unityPathBtn;
    private Button serverPathBtn;
    private Button startExportBtn;
    private TreeView treeView;
    private VisualElement rootPanel;
    private TextField AddAGroup_TextField;
    private Button AddAGroup_Button;
    private Label ChooseGroupName;
    private Button RemoveGroupBtn;
    
    #endregion
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
        
        rootPanel = root.Q<VisualElement>("RootPanel");

        rootPanel.visible = true;

        

    }
}
