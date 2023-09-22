using Match3.Model;
using UnityEngine;

namespace Match3.View
{
    public class TileView : MonoBehaviour
    {
        public const float Size = 2.56f;

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] sprites;

        TileModel model;

        public void Setup(TileModel model)
        {
            transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
            this.model = model;

            spriteRenderer.sprite = sprites[model.Color];
        }
    }
}