using UnityEngine;

[CreateAssetMenu(fileName = "NewMusic", menuName = "Audio/Music")]
public class MusicBase : ScriptableObject
{
    [Header("Ƭ���б�")] public AudioClip[] Clips;
}

