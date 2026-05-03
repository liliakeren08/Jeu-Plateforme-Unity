using System.Collections;
using UnityEngine;

public class UI : MonoBehaviour
{
    // Appelé lorsque le bouton "Quitter" est cliqué pour quitter le jeu
    public void OnQuitClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        StartCoroutine(QuitterBorne());
#endif
    }

    private IEnumerator QuitterBorne()
    {
        try
        {
            System.Diagnostics.Process.Start(System.IO.Path.Combine(Application.dataPath, "../../Portail/Portail.exe"));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Impossible de lancer le portail : " + e.Message);
        }
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}

