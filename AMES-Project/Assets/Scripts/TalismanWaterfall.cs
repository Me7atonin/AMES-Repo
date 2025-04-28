using UnityEngine;

public class Waterfall : MonoBehaviour
{
    public Talisman[] talismans;  // References to the 3 talismans
    public bool levelComplete = false;

    void Update()
    {
        // Check if all talismans are placed every frame
        CheckTalismanStatus();
    }

    void CheckTalismanStatus()
    {
        // If all talismans are placed, trigger level completion
        if (talismans.Length == 3 &&
            talismans[0].isPlaced &&
            talismans[1].isPlaced &&
            talismans[2].isPlaced)
        {
            levelComplete = true;
            Debug.Log("Level Complete! All talismans placed in the waterfall.");
            EndLevel();
        }
        else
        {
            Debug.Log("Place all the talismans in the waterfall.");
        }
    }

    void EndLevel()
    {
        // Trigger level end actions, such as loading the next scene, showing a victory message, etc.
    }
}
