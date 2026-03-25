using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SceneCreator
{
    [MenuItem("Tools/Zombie/Create Test Scene")]
    static void CreateTestScene()
    {
        // Create new scene
        string path = "Assets/Zombie/Scenes/RagdollTest.unity";
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

        // Add a plane for ground (remove the default camera and light if needed)
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = Vector3.zero;

        // Add the zombie model
        GameObject zombieModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/chars/Warzombie F Pedroso@Breathing Idle.fbx");
        if (zombieModel != null)
        {
            GameObject zombie = PrefabUtility.InstantiatePrefab(zombieModel) as GameObject;
            zombie.transform.position = new Vector3(0, 1, 0);
        }

        // Save scene
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);
    }
}