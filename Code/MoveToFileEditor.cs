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
        if (path.Length == 0) return; // 취소

        bool assetMoved = false; // 에셋이 성공적으로 이동되었는지 확인

        foreach (var asset in selectedAssets)
        {
            string sourcePath = AssetDatabase.GetAssetPath(asset);
            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Path.Combine(path, fileName);

            // 이동하려는 경로가 소스 경로와 동일한지 검사
            if (Path.GetFullPath(sourcePath).Equals(Path.GetFullPath(destinationPath), StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning($"Moving to the same location skipped: {sourcePath}");
                EditorUtility.DisplayDialog("Move Skipped", $"The move operation for [{fileName}] to the same location was skipped.", "OK");
                continue; // 같은 위치로의 이동 시도는 건너뛰기
            }

            if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
            {
                bool overwrite = EditorUtility.DisplayDialog("Overwrite File?", $"The file [{fileName}] already exists. Do you want to overwrite it?", "Yes", "No");
                if (!overwrite)
                {
                    // no 선택 시, 현재 파일/폴더의 이동을 취소하고 다음 중복된 파일/폴더 덮어쓰기 선택
                    continue;
                }
                else
                {
                    DeleteAsset(destinationPath); // 덮어쓰기를 위해 기존 파일/폴더 삭제
                }
            }
            // 파일 및 폴더 이동
            if (MoveAsset(sourcePath, destinationPath))
            {
                assetMoved = true; 
            }
        }

        if (assetMoved) // 하나 이상의 파일 및 폴더가 이동되었다면 다이알로그 표시
        {
            // 이동된 폴더의 이름을 다이알로그에서 표시
            string finalFolderName = new DirectoryInfo(path).Name;
            EditorUtility.DisplayDialog("Move Complete", $"Selected files/folders have been moved to [{finalFolderName}].", "OK"); 
        }
    }

    static bool MoveAsset(string sourcePath, string targetPath) // 파일 및 폴더 이동
    {
        string assetPath = sourcePath.Replace(Application.dataPath, "Assets");
        string targetAssetPath = targetPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");

        string error = AssetDatabase.MoveAsset(assetPath, targetAssetPath);
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"Error moving asset: {error}");
            return false; // 이동 실패
        }

        AssetDatabase.Refresh();
        return true; // 이동 성공
    }

    static void DeleteAsset(string targetPath) //지정된 경로에 같은 이름의 파일 및 폴더 삭제
    {
        string targetAssetPath = targetPath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        if (!AssetDatabase.DeleteAsset(targetAssetPath))
        {
            Debug.LogError($"Failed to delete existing asset at {targetAssetPath}.");
        }
    }
}
