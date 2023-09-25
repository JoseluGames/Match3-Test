
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

        bool readyToMatch;
        public bool IsBusy { get; private set; }
        GameView gameView;

        public TileModel Model { get; private set; }

        public void Setup(GameView gameView, TileModel model, int spawnY)
        {
            this.gameView = gameView;
            Model = model;

            transform.localPosition = new Vector3(model.X * Size, spawnY * Size);

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
                    IsBusy = false;
                    yield return null;
                    continue;
                }

                IsBusy = true;
                var action = pendingActions.Dequeue();
                yield return StartCoroutine(ResolveAction(action));
                yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));
            }
        }

        IEnumerator ResolveAction(TileAction action)
        {
            switch (action)
            {
                case SwapAction swapAction:
                    if (swapAction.Successful)
                    {
                        transform.localPosition = new Vector3(swapAction.TargetPos.x * Size, swapAction.TargetPos.y * Size);
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
                    yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Match"));
                    break;
                case FallAction fallAction:
                    var startY = transform.localPosition.y;
                    var endY = fallAction.EndY * Size;

                    var distance = startY - endY;
                    var duration = distance / fallSpeed;
                    var t = 0f;
                    while (t < duration)
                    {
                        t += Time.deltaTime;
                        transform.localPosition = new Vector3(Model.X * Size, Mathf.Lerp(startY, endY, t / duration));
                        yield return null;
                    }

                    transform.localPosition = new Vector3(Model.X * Size, endY);
                    break;
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (IsBusy)
                return;

            hasMoved = false;
            dragStart = eventData.position;
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (IsBusy)
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
            pendingActions.Enqueue(new SwapAction { Successful = true, Direction = direction, TargetPos = new Vector2Int(Model.X, Model.Y) });
        }

        public void Match(List<TileView> companions)
        {
            pendingActions.Enqueue(new MatchAction { Companions = companions });
        }

        public void Fall()
        {
            pendingActions.Enqueue(new FallAction { EndY = Model.Y });
        }

        //Called by animator
        void EndMatch()
        {
            Destroy(gameObject);
            gameView.ViewList.Remove(this);
        }
    }
}