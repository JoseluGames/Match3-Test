using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Match3.Model;
using Match3.View;
using TMPro;
using UnityEngine;

namespace Match3.Debugger
{
    public class DebugController : MonoBehaviour
    {
        [SerializeField] TMP_InputField movesInput;
        [SerializeField] GameView gameView;

        //Called from UI
        void SimulateMoves()
        {
            StartCoroutine(SimulateMovesRoutine());
        }

        IEnumerator SimulateMovesRoutine()
        {
            if (!int.TryParse(movesInput.text, out var moves))
            {
                Debug.LogError("Invalid number of moves");
                yield break;
            }

            var swappableTiles = new List<TileModel>();
            Time.timeScale = 1;

            for (var i = 0; i < moves; i++)
            {
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

                var targetTile = swappableTiles[Random.Range(0, swappableTiles.Count)];
                var direction = targetTile.ValidSwaps.First(kvp => kvp.Value != null).Key;
                targetTile.TrySwap(direction);

                yield return null;
                yield return new WaitWhile(() => gameView.IsBoardBusy || gameView.ViewList.Any(tv => tv.IsBusy));
            }

            Time.timeScale = 1;
        }
    }
}