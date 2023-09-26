using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Match3.Model;
using Match3.View;
using PlasticPipe.PlasticProtocol.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Match3.Debugger
{
    public class DebugController : MonoBehaviour
    {
        [SerializeField] TMP_InputField movesInput;
        [SerializeField] GameView gameView;

        [SerializeField] Button simulateButton;
        [SerializeField] TMP_Text simulateButtonText;
        [SerializeField] Button stopButton;

        [SerializeField] Slider timeScaleSlider;

        bool stop = false;

        //Called from UI
        public void SimulateMoves()
        {
            StartCoroutine(SimulateMovesRoutine());
        }

        //Called from UI
        public void StopSimulation()
        {
            stop = true;
        }

        IEnumerator SimulateMovesRoutine()
        {
            if (!int.TryParse(movesInput.text, out var moves))
            {
                Debug.LogError("Invalid number of moves");
                yield break;
            }

            simulateButton.interactable = false;
            stopButton.interactable = true;
            var swappableTiles = new List<TileModel>();

            for (var i = 0; i < moves; i++)
            {
                if (stop)
                {
                    Debug.LogError("Simulation stopped");
                    break;
                }

                Time.timeScale = timeScaleSlider.value;

                simulateButtonText.text = $"Simulating {i}/{moves}";
                swappableTiles.Clear();
                for (var x = 0; x < gameView.Model.Tiles.GetLength(0); x++)
                {
                    for (var y = 0; y < gameView.Model.Tiles.GetLength(1); y++)
                    {
                        var tile = gameView.Model.Tiles[x, y];
                        if (tile != null && tile.IsSwappable)
                            swappableTiles.Add(tile);
                    }
                }

                if (swappableTiles.Count == 0)
                {
                    Debug.LogError("No valid moves available");
                    break;
                }

                var targetTile = swappableTiles[Random.Range(0, swappableTiles.Count)];
                var direction = targetTile.ValidSwaps.First(kvp => kvp.Value != null).Key;
                targetTile.TrySwap(direction);

                yield return null;
                yield return new WaitWhile(() => gameView.IsBoardBusy || gameView.ViewList.Any(tv => tv.IsBusy));
            }

            Time.timeScale = 1;

            simulateButtonText.text = "Simulate";
            simulateButton.interactable = true;
            stopButton.interactable = false;
            stop = false;
        }
    }
}