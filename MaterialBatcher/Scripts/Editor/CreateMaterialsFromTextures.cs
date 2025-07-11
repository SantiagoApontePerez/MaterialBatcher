using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialBatcher.MaterialBatcher.MaterialBatcher.Scripts.Editor
{
	public class CreateMaterialsFromTextures : EditorWindow
	{
		#region Private Fields

		private DefaultAsset _textureFolder;
		private string _targetFolderPath = "Assets/";
		private Shader _shader;

		#endregion

    	[MenuItem("Tools/Material Batcher")]
    	public static void ShowWindow()
    	{
        	GetWindow<CreateMaterialsFromTextures>("Material Batcher");
    	}

    	private void OnGUI()
	    {
		    EditorGUILayout.LabelField("Source Textures:", EditorStyles.boldLabel);
		    _textureFolder = (DefaultAsset)EditorGUILayout.ObjectField("Texture Folder", _textureFolder, typeof(DefaultAsset), false);

		    EditorGUILayout.Space();

		    EditorGUILayout.LabelField("Target Directory:", EditorStyles.boldLabel);
		    EditorGUILayout.BeginHorizontal();
		    _targetFolderPath = EditorGUILayout.TextField(_targetFolderPath);
		    if (GUILayout.Button("...", GUILayout.Width(24)))
		    {
			    var picked = EditorUtility.OpenFolderPanel("Select Target Folder", Application.dataPath, "");

			    if (!string.IsNullOrEmpty(picked))
			    {
				    if (picked.StartsWith(Application.dataPath))
				    {
					    _targetFolderPath = "Assets" + picked.Substring(Application.dataPath.Length);
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
		    _shader = (Shader)EditorGUILayout.ObjectField("Base Shader", _shader, typeof(Shader), false);

		    EditorGUILayout.Space();

		    if (GUILayout.Button("Batch to Material"))
		    {
			    if (_textureFolder == null)
			    {
				    EditorUtility.DisplayDialog("Error", "Please assign a source texture folder first.", "OK");
				    return;
			    }
			    
			    if (string.IsNullOrEmpty(_targetFolderPath))
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
		    if (!AssetDatabase.IsValidFolder(_targetFolderPath))
			    AssetDatabase.CreateFolder(Path.GetDirectoryName(_targetFolderPath), Path.GetFileName(_targetFolderPath));

		    // Get all textures in source folder
		    var sourcePath = AssetDatabase.GetAssetPath(_textureFolder);
		    var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { sourcePath });

		    foreach (var guid in guids)
		    {
			    var texPath = AssetDatabase.GUIDToAssetPath(guid);
			    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

			    // Create material
			    var mat = new Material(_shader ?? Shader.Find("Standard"));
			    mat.mainTexture = tex;

			    // Save material asset
			    var matName = Path.GetFileNameWithoutExtension(texPath) + ".mat";
			    var savePath = Path.Combine(_targetFolderPath, matName);
			    AssetDatabase.CreateAsset(mat, savePath);
		    }

		    AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();
		    EditorUtility.DisplayDialog("Done", "Materials generated in:\n" + _targetFolderPath, "OK");
    	}
	}
}