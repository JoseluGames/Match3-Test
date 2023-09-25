using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async void SimulateMoves()
        {
            if (!int.TryParse(movesInput.text, out var moves))
            {
                Debug.LogError("Invalid number of moves");
                return;
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

                await Task.Delay(Mathf.CeilToInt(1000 / Time.timeScale));
            }

            Time.timeScale = 1;
        }
    }
}