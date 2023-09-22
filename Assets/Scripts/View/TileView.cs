using Match3.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Match3.View
{
    public class TileView : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public const float Size = 2.56f;

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] sprites;
        [SerializeField] float dragSensitivity;

        TileModel model;
        bool hasMoved;

        Vector2 dragStart;

        public void Setup(TileModel model)
        {
            transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
            this.model = model;

            spriteRenderer.sprite = sprites[model.Color];

            model.OnSuccessfulSwap += OnSuccessfulSwap;
            model.OnFailedSwap += OnFailedSwap;
        }

        void OnSuccessfulSwap(Direction direction)
        {
            transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
            gameObject.name = $"Tile {model.X} {model.Y}";
        }

        void OnFailedSwap(Direction direction)
        {
        }

        void TrySwap(Direction direction)
        {
            model.TrySwap(direction);
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            hasMoved = false;
            dragStart = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (hasMoved)
                return;

            var delta = eventData.position - dragStart;
            if (delta.magnitude > dragSensitivity)
            {
                hasMoved = true;
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    TrySwap(delta.x > 0 ? Direction.Right : Direction.Left);
                else
                    TrySwap(delta.y > 0 ? Direction.Top : Direction.Down);
            }
        }
    }
}