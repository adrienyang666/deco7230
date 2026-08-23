#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public static class Week3SceneBuilder
{
    [MenuItem("Tools/DECO7230/Build Week 3 Scenes")]
    public static void Build()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        BuildMain();
        BuildVectors();
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("DECO7230 Week 03", "Scenes created successfully. Open Week3_Activity1 or VectorProducts from Assets/Scenes.", "OK");
    }

    static void ClearScene(string name)
    {
        var scene = SceneManager.CreateScene(name);
        SceneManager.SetActiveScene(scene);
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)) Object.DestroyImmediate(go);
    }

    static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat=null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=pos; go.transform.localScale=scale;
        if(mat) go.GetComponent<Renderer>().sharedMaterial=mat; return go;
    }

    static void CameraAndLight()
    {
        var cam = new GameObject("Main Camera"); cam.tag="MainCamera"; var c=cam.AddComponent<Camera>(); c.transform.position=new Vector3(0,2,-10); c.transform.LookAt(new Vector3(0,0.5f,0));
        var light=new GameObject("Directional Light"); var dl=light.AddComponent<Light>(); dl.type=LightType.Directional; light.transform.rotation=Quaternion.Euler(50,-30,0);
    }

    static TextMeshProUGUI Text(Canvas canvas,string name,string content,Vector2 pos)
    {
        var go=new GameObject(name); go.transform.SetParent(canvas.transform,false); var t=go.AddComponent<TextMeshProUGUI>(); t.text=content; t.fontSize=28; t.color=Color.white; t.alignment=TextAlignmentOptions.Left;
        var rt=t.rectTransform; rt.anchorMin=new Vector2(0,1); rt.anchorMax=new Vector2(0,1); rt.pivot=new Vector2(0,1); rt.anchoredPosition=pos; rt.sizeDelta=new Vector2(650,45); return t;
    }

    static Canvas MakeCanvas()
    {
        var go=new GameObject("Canvas"); var canvas=go.AddComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace; canvas.transform.position=new Vector3(-4.5f,3f,3.5f); canvas.transform.localScale=Vector3.one*.01f; canvas.transform.rotation=Quaternion.Euler(0,0,0);
        var scaler=go.AddComponent<CanvasScaler>(); scaler.dynamicPixelsPerUnit=10; go.AddComponent<GraphicRaycaster>(); return canvas;
    }

    static void BuildMain()
    {
        ClearScene("Week3_Activity1"); CameraAndLight();
        var debug=Cube("DebugCube",Vector3.zero,Vector3.one); var rot=debug.AddComponent<DebugRotator>(); rot.rotationSpeed=45;
        debug.AddComponent<SimpleRaycast>();
        var target=Cube("TargetCube",new Vector3(3,0,0),Vector3.one);
        var platform=Cube("Platform",new Vector3(3,-2,0),new Vector3(2,.5f,2));
        var falling=Cube("FallingCube",new Vector3(-3,2,0),Vector3.one); var rb=falling.AddComponent<Rigidbody>(); rb.mass=1; rb.linearDamping=0;
        var math=debug.AddComponent<UsefulMath>(); math.debugCube=debug.transform; math.fallingCube=falling.transform; math.targetCube=target.transform;
        var canvas=MakeCanvas(); var ui=canvas.gameObject.AddComponent<UIManager>(); ui.targetRotator=rot; ui.usefulMath=math;
        ui.rotationDisplay=Text(canvas,"RotationDisplay","Rotation: 0°",new Vector2(10,-10));
        ui.positionDisplay=Text(canvas,"PositionDisplay","Position: (0, 0, 0)",new Vector2(10,-55));
        ui.speedDisplay=Text(canvas,"SpeedDisplay","Speed: 45",new Vector2(10,-100));
        ui.statusDisplay=Text(canvas,"StatusDisplay","Status: Rotating",new Vector2(10,-145));
        ui.distanceDisplay=Text(canvas,"DistanceDisplay","Distance: 0",new Vector2(10,-190));
        ui.angleDisplay=Text(canvas,"AngleDisplay","Angle: 0°",new Vector2(10,-235));
        var bgo=new GameObject("ToggleRotationButton"); bgo.transform.SetParent(canvas.transform,false); var b=bgo.AddComponent<Button>(); var img=bgo.AddComponent<Image>(); img.color=new Color(.15f,.15f,.15f,.9f); b.targetGraphic=img; var br=bgo.GetComponent<RectTransform>(); br.anchorMin=new Vector2(0,1);br.anchorMax=new Vector2(0,1);br.pivot=new Vector2(0,1);br.anchoredPosition=new Vector2(10,-290);br.sizeDelta=new Vector2(260,55);
        var btGo=new GameObject("Toggle Rotation"); btGo.transform.SetParent(bgo.transform,false); var bt=btGo.AddComponent<TextMeshProUGUI>(); bt.text="Toggle Rotation"; bt.alignment=TextAlignmentOptions.Center; bt.fontSize=24; bt.color=Color.white; bt.rectTransform.anchorMin=Vector2.zero;bt.rectTransform.anchorMax=Vector2.one;bt.rectTransform.offsetMin=Vector2.zero;bt.rectTransform.offsetMax=Vector2.zero;
        var bh=canvas.gameObject.AddComponent<ButtonHandler>(); bh.targetRotator=rot; bh.toggleButton=b;
        var es=new GameObject("EventSystem"); es.AddComponent<UnityEngine.EventSystems.EventSystem>(); es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/Week3_Activity1.unity");
    }

    static void BuildVectors()
    {
        ClearScene("VectorProducts"); CameraAndLight();
        var a=Cube("RotatingCube",Vector3.zero,Vector3.one); var r=a.AddComponent<DebugRotator>(); r.rotationSpeed=30;
        var b=Cube("TargetCube",new Vector3(3,0,0),Vector3.one); var vp=a.AddComponent<VectorProducts>(); vp.vectorA=a.transform;vp.vectorB=b.transform;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/VectorProducts.unity");
    }
}
#endif
