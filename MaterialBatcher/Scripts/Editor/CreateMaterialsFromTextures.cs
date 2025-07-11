using UnityEngine;
using UnityEditor;

namespace MaterialsBatcher
{
	public class CreateMaterialsFromTextures : EditorWindow
	{
    	[MenuItem("Tools/Material Batcher")]
    	public static void ShowWindow()
    	{
        	GetWindow<CreateMaterialsFromTextures>("Material Batcher");
    	}

    	private DefaultAsset folder;
    	Shader shader = Shader.Find("Standard");

    	private void OnGUI()
    	{
        	EditorGUILayout.LabelField("Select texture folder:", EditorStyles.boldLabel);
        	folder =(DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false);
        	shader = (Shader)EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false);

        	if (EditorGUILayout.Button("Generate Materials")) Generate();
    	}

    	private void Generate()
    	{
        	if (folder == null || shader == null)
        	{
            	Debug.LogError("Assign both a folder and a shader.");
            	return;
        	}

        	var path = AssetDatabase.GetAssetPath(folder);
        	var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });

        	foreach (var guid in guids)
        	{
            	var texPath = AssetDatabase.GUIDToAssetPath(guid);
            	var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

            	var mat = new Material(shader)
            	{
                	name = "M_" + tex.name
            	};
            	mat.SetTexture("_MainTex", tex);

            	var matPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(texPath), mat.name + ".mat");
            	AssetDatabase.CreateAsset(mat, AssetDatabase.GenerateUniqueAssetPath(matPath))
        	}

        	AssetDatabase.SaveAssets();
        	AssetDatabase.Refresh();
        	Debug.LogError($"Generated {guids.Length} materials.");
    	}
	}
}