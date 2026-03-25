using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RagdollSetup : EditorWindow
{
    [MenuItem("Tools/Zombie/Ragdoll Setup")]
    static void Init()
    {
        RagdollSetup window = (RagdollSetup)EditorWindow.GetWindow(typeof(RagdollSetup));
        window.Show();
    }

    void OnGUI()
    {
        if (GUILayout.Button("Setup Ragdoll on Selected"))
        {
            SetupRagdoll();
        }
    }

    void SetupRagdoll()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("No GameObject selected!");
            return;
        }

        // Define bone patterns - supports both standard names and Mixamo naming
        Dictionary<string, BoneInfo> boneInfos = new Dictionary<string, BoneInfo>
        {
            {"Hips", new BoneInfo { mass = 10f, colliderType = ColliderType.Box, size = new Vector3(0.3f, 0.2f, 0.2f) }},
            {"Spine", new BoneInfo { mass = 8f, colliderType = ColliderType.Capsule, radius = 0.15f, height = 0.3f }},
            {"Chest", new BoneInfo { mass = 6f, colliderType = ColliderType.Capsule, radius = 0.12f, height = 0.25f }},
            {"Spine1", new BoneInfo { mass = 6f, colliderType = ColliderType.Capsule, radius = 0.12f, height = 0.25f }},
            {"Neck", new BoneInfo { mass = 2f, colliderType = ColliderType.Capsule, radius = 0.05f, height = 0.1f }},
            {"Head", new BoneInfo { mass = 5f, colliderType = ColliderType.Sphere, radius = 0.1f }},
            {"LeftShoulder", new BoneInfo { mass = 2f, colliderType = ColliderType.Sphere, radius = 0.05f }},
            {"LeftArm", new BoneInfo { mass = 3f, colliderType = ColliderType.Capsule, radius = 0.05f, height = 0.3f }},
            {"LeftForeArm", new BoneInfo { mass = 2f, colliderType = ColliderType.Capsule, radius = 0.04f, height = 0.25f }},
            {"LeftHand", new BoneInfo { mass = 1f, colliderType = ColliderType.Sphere, radius = 0.05f }},
            {"LeftUpLeg", new BoneInfo { mass = 4f, colliderType = ColliderType.Capsule, radius = 0.06f, height = 0.4f }},
            {"LeftLeg", new BoneInfo { mass = 3f, colliderType = ColliderType.Capsule, radius = 0.05f, height = 0.35f }},
            {"LeftFoot", new BoneInfo { mass = 2f, colliderType = ColliderType.Box, size = new Vector3(0.1f, 0.05f, 0.2f) }},
            {"RightShoulder", new BoneInfo { mass = 2f, colliderType = ColliderType.Sphere, radius = 0.05f }},
            {"RightArm", new BoneInfo { mass = 3f, colliderType = ColliderType.Capsule, radius = 0.05f, height = 0.3f }},
            {"RightForeArm", new BoneInfo { mass = 2f, colliderType = ColliderType.Capsule, radius = 0.04f, height = 0.25f }},
            {"RightHand", new BoneInfo { mass = 1f, colliderType = ColliderType.Sphere, radius = 0.05f }},
            {"RightUpLeg", new BoneInfo { mass = 4f, colliderType = ColliderType.Capsule, radius = 0.06f, height = 0.4f }},
            {"RightLeg", new BoneInfo { mass = 3f, colliderType = ColliderType.Capsule, radius = 0.05f, height = 0.35f }},
            {"RightFoot", new BoneInfo { mass = 2f, colliderType = ColliderType.Box, size = new Vector3(0.1f, 0.05f, 0.2f) }}
        };

        // Find bones
        Transform[] bones = selected.GetComponentsInChildren<Transform>();
        int setupCount = 0;
        int jointCount = 0;
        string foundBones = "";

        foreach (Transform bone in bones)
        {
            string cleanName = bone.name;
            // Remove Mixamo prefix if present
            if (cleanName.Contains(":"))
                cleanName = cleanName.Substring(cleanName.IndexOf(":") + 1);

            foundBones += cleanName + ", ";

            if (boneInfos.ContainsKey(cleanName))
            {
                BoneInfo info = boneInfos[cleanName];
                SetupBone(bone, info);
                setupCount++;
            }
        }

        Debug.Log($"Found bones: {foundBones}");
        Debug.Log($"Set up {setupCount} bones with Rigidbodies and Colliders");

        // Setup joints (simplified, connect to parent)
        foreach (Transform bone in bones)
        {
            string cleanName = bone.name;
            if (cleanName.Contains(":"))
                cleanName = cleanName.Substring(cleanName.IndexOf(":") + 1);

            if (boneInfos.ContainsKey(cleanName) && bone.parent != null)
            {
                string parentCleanName = bone.parent.name;
                if (parentCleanName.Contains(":"))
                    parentCleanName = parentCleanName.Substring(parentCleanName.IndexOf(":") + 1);

                if (boneInfos.ContainsKey(parentCleanName))
                {
                    ConfigurableJoint joint = bone.gameObject.AddComponent<ConfigurableJoint>();
                    joint.connectedBody = bone.parent.GetComponent<Rigidbody>();
                    
                    // Configure joint for active ragdoll with constraints
                    joint.xMotion = ConfigurableJointMotion.Limited;
                    joint.yMotion = ConfigurableJointMotion.Limited;
                    joint.zMotion = ConfigurableJointMotion.Limited;
                    joint.angularXMotion = ConfigurableJointMotion.Limited;
                    joint.angularYMotion = ConfigurableJointMotion.Limited;
                    joint.angularZMotion = ConfigurableJointMotion.Limited;
                    
                    // Set linear limits
                    SoftJointLimit linearLimit = new SoftJointLimit { limit = 0.1f, bounciness = 0f };
                    joint.linearLimit = linearLimit;
                    
                    // Set angular limits
                    SoftJointLimit angularLimit = new SoftJointLimit { limit = 45f, bounciness = 0f };
                    joint.lowAngularXLimit = angularLimit;
                    joint.highAngularXLimit = angularLimit;
                    joint.angularYLimit = angularLimit;
                    joint.angularZLimit = angularLimit;
                    
                    // Add spring to return to pose
                    JointDrive drive = new JointDrive { positionSpring = 1000f, positionDamper = 100f, maximumForce = Mathf.Infinity };
                    joint.xDrive = drive;
                    joint.yDrive = drive;
                    joint.zDrive = drive;
                    joint.angularXDrive = drive;
                    joint.angularYZDrive = drive;
                    
                    jointCount++;
                }
            }
        }

        Debug.Log($"Set up {jointCount} joints");
        Debug.Log("Ragdoll setup complete!");
    }

    void SetupBone(Transform bone, BoneInfo info)
    {
        // Add Rigidbody
        Rigidbody rb = bone.gameObject.AddComponent<Rigidbody>();
        rb.mass = info.mass;
        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.05f;

        // Add Collider
        switch (info.colliderType)
        {
            case ColliderType.Box:
                BoxCollider box = bone.gameObject.AddComponent<BoxCollider>();
                box.size = info.size;
                break;
            case ColliderType.Capsule:
                CapsuleCollider cap = bone.gameObject.AddComponent<CapsuleCollider>();
                cap.radius = info.radius;
                cap.height = info.height;
                cap.direction = 1; // Y-axis
                break;
            case ColliderType.Sphere:
                SphereCollider sph = bone.gameObject.AddComponent<SphereCollider>();
                sph.radius = info.radius;
                break;
        }
    }

    enum ColliderType { Box, Capsule, Sphere }

    class BoneInfo
    {
        public float mass;
        public ColliderType colliderType;
        public float radius;
        public float height;
        public Vector3 size;
    }
}