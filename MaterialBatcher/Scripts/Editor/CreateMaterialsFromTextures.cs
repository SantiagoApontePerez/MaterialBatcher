using UnityEngine;
using UnityEditor;

namespace MaterialsBatcher
{
	public class CreateMaterialsFromTextures : EditorWindow
	{
		#region Private Fields

		private DefaultAsset textureFolder;
		private string targetFolderPath = "Assets/";
		private Shader shader;

		#endregion

    	[MenuItem("Tools/Material Batcher")]
    	public static void ShowWindow()
    	{
        	GetWindow<CreateMaterialsFromTextures>("Material Batcher");
    	}

    	private void OnGUI()
	    {
		    EditorGUILayout.LabelField("Source Textures:", EditorStyles.boldLabel);
		    sourceFolder = (DefaultAsset)EditorGUILayout.ObjectField("Texture Folder", sourceFolder, typeof(DefaultAsset), false);

		    EditorGUILayout.Space();

		    EditorGUILayout.LabelField("Target Directory:", EditorStyles.boldLabel);
		    EditorGUILayout.BeginHorizontal();
		    targetFolderPath = EditorGUILayout.TextField(targetFolderPath);
		    if (GUILayout.Button("...", GUILayout.Width(24)))
		    {
			    var picked = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");

			    if (!string.IsNullOrEmpty(picked))
			    {
				    if (picked.StartsWith(Application.dataPath))
				    {
					    targetFolderPath = "Assets" + picked.SubString(Application.dataPath.Length);
				    }
				    else
				    {
					    Debug.LogWarning("Please choose a folder inside this project's Assets directory.");
				    }
			    }
		    }
		    EditorGUILayout.EndHorizontal();

		    EditorGUILayout.Space();

		    EditorGUILayout.LabelField("Material Settings", EditorStyles.boldLabel);
		    shader = (Shader)EditorGUILayout.ObjectField("Base Shader", shader, typeof(Shader), false);

		    EditorGUILayout.Space();

		    if (GUILayout.Button("Batch to Material"))
		    {
			    if (sourceFolder == null)
			    {
				    EditorUtility.DisplayDialog("Error", "Please assign a source texture folder first.", "OK");
				    return;
			    }
			    
			    if (string.IsNullOrEmpty(targetFolderPath))
			    {
				    EditorUtility.DisplayDialog("Error", "Please select a valid target directory.", "OK");
				    return;
			    }
			    Generate();
		    }
	    }

    	private void Generate()
    	{
		    // Ensure output folder exists
		    if (!AssetDatabase.IsValidFolder(targetFolderPath))
			    AssetDatabase.CreateFolder(Path.GetDirectoryName(targetFolderPath), Path.GetFileName(targetFolderPath));

		    // Get all textures in source folder
		    var sourcePath = AssetDatabase.GetAssetPath(sourceFolder);
		    var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { sourcePath });

		    foreach (var guid in guids)
		    {
			    var texPath = AssetDatabase.GUIDToAssetPath(guid);
			    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

			    // Create material
			    var mat = new Material(shader ?? Shader.Find("Standard"));
			    mat.mainTexture = tex;

			    // Save material asset
			    var matName = Path.GetFileNameWithoutExtension(texPath) + ".mat";
			    var savePath = Path.Combine(targetFolderPath, matName);
			    AssetDatabase.CreateAsset(mat, savePath);
		    }

		    AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();
		    EditorUtility.DisplayDialog("Done", "Materials generated in:\n" + targetFolderPath, "OK");
    	}
	}
}