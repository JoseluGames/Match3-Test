
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Match3.Model;
using Match3.View.Action;
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

        Queue<TileAction> pendingActions = new();

        bool hasMoved;
        Vector2 dragStart;

        bool inputActive;
        bool readyToMatch;
        public bool IsBusy { get; private set; }
        GameView gameView;

        public TileModel Model { get; private set; }

        public void Setup(GameView gameView, TileModel model, int spawnY)
        {
            this.gameView = gameView;
            Model = model;

            inputActive = true;
            transform.localPosition = new Vector3(model.X * Size, spawnY * Size);

            if (spawnY == model.Y)
                gameView.ViewsGrid[model.X, model.Y] = this;

            spriteRenderer.sprite = sprites[model.Color];

            model.OnSuccessfulSwap += OnSuccessfulSwap;
            model.OnFailedSwap += OnFailedSwap;

            StartCoroutine(ResolveActions());
        }

        IEnumerator ResolveActions()
        {
            while (true)
            {
                if (pendingActions.Count == 0)
                {
                    yield return null;
                    continue;
                }

                inputActive = false;
                IsBusy = true;
                var action = pendingActions.Dequeue();
                StartCoroutine(ResolveAction(action));
                yield return null;
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
                IsBusy = false;
                inputActive = true;
            }
        }

        IEnumerator ResolveAction(TileAction action)
        {
            switch (action)
            {
                case SwapAction swapAction:
                    if (swapAction.Successful)
                    {
                        animator.SetInteger(SuccessDirectionParam, (int)swapAction.Direction);
                        animator.SetTrigger(SuccessTrigger);
                    }
                    else
                        animator.SetTrigger(FailTrigger);
                    break;
                case MatchAction matchAction:
                    readyToMatch = true;
                    yield return new WaitUntil(() => matchAction.Companions.All(c => c.readyToMatch));
                    animator.SetTrigger(MatchTrigger);
                    break;
                case FallAction _:
                    inputActive = false;
                    gameView.ViewsGrid[Model.X, Model.Y] = this;

                    var startY = transform.localPosition.y;
                    var endY = Model.Y * Size;

                    var distance = startY - endY;
                    var duration = distance / fallSpeed;
                    var t = 0f;
                    while (t < duration)
                    {
                        t += Time.deltaTime;
                        transform.localPosition = new Vector3(Model.X * Size, Mathf.Lerp(startY, endY, t / duration));
                        yield return null;
                    }

                    RefreshPosition();
                    inputActive = true;
                    break;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!inputActive)
                return;

            hasMoved = false;
            dragStart = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!inputActive)
                return;

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

            void TrySwap(Direction direction)
            {
                Model.TrySwap(direction);
            }
        }

        void OnFailedSwap()
        {
            pendingActions.Enqueue(new SwapAction { Successful = false });
        }

        void OnSuccessfulSwap(Direction direction)
        {
            pendingActions.Enqueue(new SwapAction { Successful = true, Direction = direction });
        }

        public void Match(List<TileView> companions)
        {
            pendingActions.Enqueue(new MatchAction { Companions = companions });
        }

        public void Fall()
        {
            pendingActions.Enqueue(new FallAction());
        }

        //Called by animator
        void EndMatch()
        {
            Destroy(gameObject);
            gameView.ViewsGrid[Model.X, Model.Y] = null;
            gameView.ViewList.Remove(this);
        }

        //Called by animator
        void RefreshPosition()
        {
            transform.localPosition = new Vector3(Model.X * Size, Model.Y * Size);
            gameObject.name = $"Tile {Model.X} {Model.Y}";
        }
    }
}