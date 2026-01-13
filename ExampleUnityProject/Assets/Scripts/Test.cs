using UnityEngine;

public class Test : MonoBehaviour
{
    [ContextMenu("TestAnalysisCode")]
    public void TestAnalysisCode()
    {
        UnityEngine.Debug.Log($"#{UnityEngine.Time.frameCount}: test");
    }
}