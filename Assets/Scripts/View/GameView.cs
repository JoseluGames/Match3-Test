using System.Collections.Generic;
using Match3.Model;
using System.Linq;
using UnityEngine;
using System.Collections;

namespace Match3.View
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] int width;
        [SerializeField] int height;
        [SerializeField] int colors;
        [SerializeField] TileView tilePrefab;
        [SerializeField] Transform tilesContainer;

        [SerializeField] SpriteRenderer background;

        [SerializeField] Camera mainCamera;
        [SerializeField] float timeScale;

        public List<TileView> ViewList { get; } = new();

        public GameModel Model { get; set; }

        public TileView[,] ViewsGrid { get; private set; }

        void Update()
        {
            Time.timeScale = timeScale;
        }

        void Start()
        {
            Model = new GameModel(width, height, colors);
            Model.OnTileSpawned += SpawnTileView;
            Model.OnMatchesResolved += OnMatchesResolved;
            Model.OnTilesFalling += OnTilesFalling;

            ViewsGrid = new TileView[width, height];
            background.size = new Vector2(width * TileView.Size, height * TileView.Size);
            tilesContainer.transform.position = new Vector3(-background.size.x / 2 + TileView.Size / 2, -background.size.y / 2 + TileView.Size / 2);

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

        void OnMatchesResolved(List<List<TileModel>> matches)
        {
            StartCoroutine(ResolveMatches(matches));
        }

        IEnumerator ResolveMatches(List<List<TileModel>> matches)
        {
            yield return null;
            yield return new WaitUntil(() => ViewList.All(tv => !tv.IsBusy));

            foreach (var match in matches)
            {
                var views = new List<TileView>();
                foreach (var tile in match)
                {
                    var view = ViewList.FirstOrDefault(tv => tv.Model == tile);
                    views.Add(view);
                }

                foreach (var view in views)
                    view.Match(views);
            }
        }

        void OnTilesFalling(List<TileModel> fallingTiles)
        {
            StartCoroutine(FallTiles(fallingTiles));
        }

        IEnumerator FallTiles(List<TileModel> fallingTiles)
        {
            if (fallingTiles.Count == 0)
                yield break;

            yield return null;
            yield return new WaitUntil(() => ViewList.All(tv => !tv.IsBusy));

            foreach (var tile in fallingTiles)
            {
                var view = ViewList.FirstOrDefault(tv => tv.Model == tile);
                view.Fall();
            }

            Model.EvaluateMatches();
        }
    }
}