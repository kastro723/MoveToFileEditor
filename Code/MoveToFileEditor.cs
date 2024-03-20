using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class MoveToFileEditor : ScriptableObject
{
    private const string Version = "1.0.0";

    [MenuItem("Assets/Move to File", false, 19)]
    static void MoveSelectedFilesOrFolders()
    {
        var selectedAssets = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
        var path = EditorUtility.OpenFolderPanel("Select Destination", Application.dataPath, "");
        if (path.Length == 0) return; // ���

        bool assetMoved = false; // ������ ���������� �̵��Ǿ����� Ȯ��

        foreach (var asset in selectedAssets)
        {
            string sourcePath = AssetDatabase.GetAssetPath(asset);
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Path.Combine(path, fileName);

            // �̵��Ϸ��� ��ΰ� �ҽ� ��ο� �������� �˻�
            if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"Moving to the same location skipped: {sourcePath}");
                EditorUtility.DisplayDialog("Move Skipped", $"The move operation for [{fileName}] to the same location was skipped.", "OK");
                continue; // ���� ��ġ���� �̵� �õ��� �ǳʶٱ�
            }

            if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Overwrite File?", $"The file [{fileName}] already exists. Do you want to overwrite it?", "Yes", "No");
                if (!overwrite)
                {
                    // no ���� ��, ���� ����/������ �̵��� ����ϰ� ���� �ߺ��� ����/���� ����� ����
                    continue;
                }
                else
                {
                    DeleteAsset(destinationPath); // ����⸦ ���� ���� ����/���� ����
                }
            }
            // ���� �� ���� �̵�
            if (MoveAsset(sourcePath, destinationPath))
            {
                assetMoved = true; 
            }
        }

        if (assetMoved) // �ϳ� �̻��� ���� �� ������ �̵��Ǿ��ٸ� ���̾˷α� ǥ��
        {
            // �̵��� ������ �̸��� ���̾˷α׿��� ǥ��
            string finalFolderName = new DirectoryInfo(path).Name;
            EditorUtility.DisplayDialog("Move Complete", $"Selected files/folders have been moved to [{finalFolderName}].", "OK"); 
        }
    }

    static bool MoveAsset(string sourcePath, string targetPath) // ���� �� ���� �̵�
    {
        string assetPath = sourcePath.Replace(Application.dataPath, "Assets");
        string targetAssetPath = targetPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");

        string error = AssetDatabase.MoveAsset(assetPath, targetAssetPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"Error moving asset: {error}");
            return false; // �̵� ����
        }

        AssetDatabase.Refresh();
        return true; // �̵� ����
    }

    static void DeleteAsset(string targetPath) //������ ��ο� ���� �̸��� ���� �� ���� ����
    {
        string targetAssetPath = targetPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        if (!AssetDatabase.DeleteAsset(targetAssetPath))
        {
            Debug.LogError($"Failed to delete existing asset at {targetAssetPath}.");
        }
    }
}
