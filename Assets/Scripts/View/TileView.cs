
using System.Collections;
using Match3.Model;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Match3.View
{
    public class TileView : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public const float Size = 2.56f;
        const string SuccessTrigger = "Success";
        const string FailTrigger = "Fail";
        const string MatchTrigger = "Match";
        const string SuccessDirectionParam = "Success Direction";

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite[] sprites;
        [SerializeField] float dragSensitivity;
        [SerializeField] Animator animator;
        [SerializeField] float fallSpeed;

        TileModel model;
        bool hasMoved;

        Vector2 dragStart;

        public void Setup(TileModel model, int spawnY)
        {
            this.model = model;

            if (spawnY == model.Y)
                transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
            else
            {
                transform.localPosition = new Vector3(model.X * Size, spawnY * Size);
                StartCoroutine(FallRoutine(spawnY, model.Y));
            }

            spriteRenderer.sprite = sprites[model.Color];

            model.OnSuccessfulSwap += OnSuccessfulSwap;
            model.OnFailedSwap += OnFailedSwap;
            model.OnMatch += OnMatch;
            model.OnFall += OnFall;
        }

        void OnSuccessfulSwap(Direction direction)
        {
            animator.SetInteger(SuccessDirectionParam, (int)direction);
            animator.SetTrigger(SuccessTrigger);
        }

        //Called by animator
        void RefreshPosition()
        {
            transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
            gameObject.name = $"Tile {model.X} {model.Y}";

            model.ResolveMatch();
        }

        void OnFailedSwap(Direction direction)
        {
            animator.SetTrigger(FailTrigger);
        }

        void OnMatch()
        {
            animator.SetTrigger(MatchTrigger);
        }

        //Called by animator
        void Destroy()
        {
            Destroy(gameObject);
        }

        void OnFall(int oldY, int newY)
        {
            StartCoroutine(FallRoutine(oldY, newY));
        }

        IEnumerator FallRoutine(int startY, int endY)
        {
            var distance = startY - endY;
            var duration = distance / fallSpeed;
            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                transform.localPosition = new Vector3(model.X * Size, Mathf.Lerp(startY, endY, t / duration) * Size);
                yield return null;
            }

            RefreshPosition();
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