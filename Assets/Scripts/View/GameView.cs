using Match3.Model;
using UnityEngine;

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

        GameModel model;

        TileView[,] tiles;

        void Start()
        {
            model = new GameModel(width, height, colors);
            model.OnTileSpawned += SpawnTileView;

            tiles = new TileView[width, height];
            background.size = new Vector2(width * TileView.Size, height * TileView.Size);
            tilesContainer.transform.position = new Vector3(-background.size.x / 2 + TileView.Size / 2, -background.size.y / 2 + TileView.Size / 2);

            mainCamera.orthographicSize = (Mathf.Max(width, height) * 1.2f) * TileView.Size; //TODO: Make it take into account the screen aspect ratio

            for (var x = 0; x < model.Tiles.GetLength(0); x++)
            {
                for (var y = 0; y < model.Tiles.GetLength(1); y++)
                {
                    var tileModel = model.Tiles[x, y];
                    SpawnTileView(tileModel, y);
                }
            }
        }

        void SpawnTileView(TileModel tileModel, int spawnY)
        {
            var tileView = Instantiate(tilePrefab, tilesContainer);
            tileView.gameObject.name = $"Tile {tileModel.X} {tileModel.Y}";
            tileView.Setup(tileModel, spawnY);
            tiles[tileModel.X, tileModel.Y] = tileView;
        }
    }
}