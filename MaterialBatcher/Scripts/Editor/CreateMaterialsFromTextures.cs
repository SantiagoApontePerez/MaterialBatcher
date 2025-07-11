using System.IO;
using UnityEditor;
using UnityEngine;

namespace MaterialBatcher
{
	public class CreateMaterialsFromTextures : EditorWindow
	{
		#region Private Fields

		private DefaultAsset _textureFolder;
		private string _prefix = "";
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
		    EditorGUILayout.LabelField("Texture Folder To Batch:", EditorStyles.boldLabel);
		    _textureFolder = (DefaultAsset)EditorGUILayout.ObjectField("Texture Folder", _textureFolder, typeof(DefaultAsset), false);
		    
		    EditorGUILayout.LabelField("Starting Prefix:", EditorStyles.boldLabel);
		    _prefix = EditorGUILayout.TextField(_prefix);

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

			EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "1. Choose the texture folder you wish to batch into materials.\n" +
                "2. Optionally add a prefix for the generated material.\n" +
                "3. Pick a target directory to store the generated materials.\n" +
                "4. Click the button below to batch to material.", MessageType.Info);

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

	    #region Helper methods
	    
	    private void CreateFolder(string path)
	    {
		    AssetDatabase.CreateFolder(Path.GetDirectoryName(_targetFolderPath), Path.GetFileName(_targetFolderPath));
	    }

	    private Material CreateMaterial(Texture2D tex)
	    {
		    return new Material(_shader ?? Shader.Find("Standard"))
		    {
			    mainTexture = tex
		    };
	    }

	    private static Texture2D GetTexture(string texPath)
	    {
		    return AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
	    }

	    private string SavePath(string texPath)
	    {
		    var matName = _prefix + Path.GetFileNameWithoutExtension(texPath) + ".mat";
		    return Path.Combine(_targetFolderPath, matName);
	    }

	    private string[] FindTextureGuids(string sourcePath)
	    {
		    return AssetDatabase.FindAssets("t:Texture2D", new[] { sourcePath });
	    }

	    private void CreateAssets(string[] guids)
	    {
		    foreach (var guid in guids)
		    {
			    var texPath = AssetDatabase.GUIDToAssetPath(guid);
			    AssetDatabase.CreateAsset(CreateMaterial(GetTexture(texPath)), SavePath(texPath));
		    }
	    }
	    
	    #endregion

    	private void Generate()
    	{
		    // Ensure output folder exists
		    if (!AssetDatabase.IsValidFolder(_targetFolderPath)) CreateFolder(_targetFolderPath);

		    // Get all textures in source folder and create materials from each.
		    CreateAssets(FindTextureGuids(AssetDatabase.GetAssetPath(_textureFolder)));

		    AssetDatabase.SaveAssets();
		    AssetDatabase.Refresh();
		    EditorUtility.DisplayDialog("Done", "Materials generated in:\n" + _targetFolderPath, "OK");
    	}
	}
}