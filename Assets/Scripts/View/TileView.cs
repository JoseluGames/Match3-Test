using Match3.Model;
using UnityEngine;

namespace Match3.View
{
    public class TileView : MonoBehaviour
    {
        const float TileSize = 1.5f;

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] sprites;

        TileModel model;

        public void Setup(TileModel model)
        {
            transform.position = new Vector3(model.X * TileSize, model.Y * TileSize);
            this.model = model;

            spriteRenderer.sprite = sprites[model.Color];
        }
    }
}