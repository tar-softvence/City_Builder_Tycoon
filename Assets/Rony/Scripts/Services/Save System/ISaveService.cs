using UnityEditor.Build;
using UnityEngine;

public interface ISaveService
{
    void SaveGame();
    void LoadGame();
    void ResetGame();
    bool HasSaveFile();
}
