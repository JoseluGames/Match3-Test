
using System.Collections;
using System.Collections.Generic;
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

        bool isMatchMaster;
        List<TileModel> pendingMatches;

        bool inputActive;

        public void Setup(TileModel model, int spawnY)
        {
            this.model = model;

            if (spawnY == model.Y)
            {
                transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
                inputActive = true;
            }
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
        }

        void TrySwap(Direction direction)
        {
            inputActive = false;
            model.TrySwap(direction);
        }

        void OnFailedSwap(Direction direction)
        {
            inputActive = true;
            animator.SetTrigger(FailTrigger);
        }

        void OnSuccessfulSwap(Direction direction)
        {
            inputActive = false;
            animator.SetInteger(SuccessDirectionParam, (int)direction);
            animator.SetTrigger(SuccessTrigger);
        }

        void OnMatch(bool isMasterTile, List<TileModel> matches)
        {
            inputActive = false;
            isMatchMaster = isMasterTile;
            pendingMatches = matches;

            animator.SetTrigger(MatchTrigger);
        }

        //Called by animator
        void EndMatch()
        {
            if (isMatchMaster)
            {
                model.GameModel.ClearTiles(pendingMatches);

                isMatchMaster = false;
                pendingMatches = null;
            }

            Destroy(gameObject);
        }

        //Called by animator
        void RefreshPosition()
        {
            transform.localPosition = new Vector3(model.X * Size, model.Y * Size);
            gameObject.name = $"Tile {model.X} {model.Y}";

            inputActive = true;
            model.ResolveMatch();
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
    }
}