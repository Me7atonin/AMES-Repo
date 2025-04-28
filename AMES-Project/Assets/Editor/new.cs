using UnityEditor;

public static class ForcePackageManager
{
    [MenuItem("Tools/Fix Package Manager")]
    public static void FixPackageManager()
    {
        UnityEditor.PackageManager.Client.Add("com.unity.inputsystem");
    }
}
