using TMPro;
using UnityEngine;

namespace Match3.Debugger
{
    public class DebugController : MonoBehaviour
    {
        [SerializeField] TMP_InputField movesInput;

        //Called from UI
        public void SimulateMoves()
        {
            if (!int.TryParse(movesInput.text, out var moves))
            {
                Debug.LogError("Invalid number of moves");
                return;
            }

            
        }
    }
}