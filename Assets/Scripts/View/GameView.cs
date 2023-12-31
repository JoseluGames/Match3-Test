using System.Collections.Generic;
using Match3.Model;
using System.Linq;
using UnityEngine;
using System.Collections;
using Match3.Data;

namespace Match3.View
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] TileView tilePrefab;
        [SerializeField] Transform tilesContainer;

        [SerializeField] SpriteRenderer background;
        [SerializeField] Transform maskTransform;

        [SerializeField] Camera mainCamera;
        [SerializeField] TileColors tileColors;

        public List<TileView> ViewList { get; } = new();

        public GameModel Model { get; set; }
        public bool IsBoardBusy { get; private set; }

        void Start()
        {
            NewBoard(5, 5, 3);
        }

        public void NewBoard(int width, int height, int colors)
        {
            if (colors > tileColors.TileSprites.Length)
            {
                colors = tileColors.TileSprites.Length;
                Debug.LogWarning($"The number of colors was reduced to {colors} to match the number of sprites in the 'Data/Tile Colors' asset.");
            }

            foreach (var view in ViewList)
                Destroy(view.gameObject);

            ViewList.Clear();

            if (Model != null)
            {
                Model.OnTileSpawned -= SpawnTileView;
                Model.OnBoardEvaluated -= OnBoardEvaluated;
            }

            Model = new GameModel(width, height, colors);
            Model.OnTileSpawned += SpawnTileView;
            Model.OnBoardEvaluated += OnBoardEvaluated;

            background.size = new Vector2(width * TileView.Size, height * TileView.Size);
            tilesContainer.transform.position = new Vector3(-background.size.x / 2 + TileView.Size / 2, -background.size.y / 2 + TileView.Size / 2);
            maskTransform.localScale = new Vector3(width, height);

            mainCamera.orthographicSize = (Mathf.Max(width, height) * 1.2f) * TileView.Size; //TODO: Make it take into account the screen aspect ratio

            Model.PopulateGrid();
        }

        void SpawnTileView(TileModel tileModel, int spawnY)
        {
            var tileView = Instantiate(tilePrefab, tilesContainer);
            tileView.gameObject.name = $"Tile {tileModel.X} {tileModel.Y}";
            tileView.Setup(this, tileModel, spawnY);
            ViewList.Add(tileView);
        }

        void OnBoardEvaluated(List<List<TileModel>> matches, List<TileModel> fallingTiles)
        {
            StartCoroutine(EvaluateBoardRoutine(matches, fallingTiles));
        }

        IEnumerator EvaluateBoardRoutine(List<List<TileModel>> matches, List<TileModel> fallingTiles)
        {
            IsBoardBusy = true;
            if (matches.Count > 0)
            {
                yield return null;
                yield return new WaitWhile(() => ViewList.Any(tv => tv.IsBusy));

                var matchingViews = new List<TileView>();
                foreach (var match in matches)
                {
                    foreach (var tile in match)
                    {
                        var view = ViewList.FirstOrDefault(tv => tv.Model == tile);
                        if (view != null)
                            matchingViews.Add(view);
                    }
                }

                foreach (var view in matchingViews)
                    view.Match(matchingViews);
            }

            if (fallingTiles.Count > 0)
            {
                yield return null;
                yield return new WaitWhile(() => ViewList.Any(tv => tv.IsBusy));

                foreach (var tile in fallingTiles)
                {
                    var view = ViewList.FirstOrDefault(tv => tv.Model == tile);
                    if (view != null)
                        view.Fall();
                }
            }

            IsBoardBusy = false;

            if (matches.Count > 0)
                Model.EvaluateBoard();
        }
    }
}