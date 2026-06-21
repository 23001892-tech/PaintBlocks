using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [SerializeField] private float dragScale = 1.15f;
    [SerializeField] private float snapDistance = 100f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private BlockView blockView;

    private BlockSpawner spawner;
    private GridManager gridManager;
    private Canvas rootCanvas;
    private RectTransform canvasRect;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalScale;

    private bool isInitialized;
    private bool isDragging;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        blockView = GetComponent<BlockView>();
    }

    public void Init(BlockSpawner ownerSpawner, GridManager targetGrid, Canvas canvas)
    {
        spawner = ownerSpawner;
        gridManager = targetGrid;
        rootCanvas = canvas;

        if (rootCanvas != null)
        {
            canvasRect = rootCanvas.GetComponent<RectTransform>();
        }

        isInitialized = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isInitialized || blockView == null || blockView.Data == null)
            return;

        isDragging = true;

        // Block đang kéo luôn giữ asset màu thật
        blockView.SetInvalidDragVisual(false);

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalScale = transform.localScale;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        transform.localScale = originalScale * dragScale;

        MoveToPointer(eventData);
        UpdatePlacementPreview(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInitialized)
            return;

        MoveToPointer(eventData);
        UpdatePlacementPreview(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (!isInitialized || blockView == null || blockView.Data == null)
        {
            ReturnToSlot(true);
            return;
        }

        if (!blockView.TryGetReferencePiece(out Vector2Int referenceOffset, out RectTransform referencePiece))
        {
            ReturnToSlot(true);
            return;
        }

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : eventData.pressEventCamera;

        Vector2 referenceScreenPosition = RectTransformUtility.WorldToScreenPoint(
            eventCamera,
            referencePiece.position
        );

        bool hasOrigin = gridManager.TryGetPlacementOriginFromBlockPiece(
            blockView.Data,
            referenceOffset,
            referenceScreenPosition,
            eventCamera,
            snapDistance,
            out int originRow,
            out int originCol
        );

        gridManager.ClearPlacementPreview();

        if (!hasOrigin)
        {
            ReturnToSlot(true);
            return;
        }

        if (gridManager.CanPlaceBlock(blockView.Data, originRow, originCol))
        {
            blockView.SetInvalidDragVisual(false);

            gridManager.PlaceBlock(blockView.Data, blockView.SpawnedColorState, originRow, originCol);
            spawner.NotifyBlockPlaced(blockView);
        }
        else
        {
            ReturnToSlot(true);
        }
    }

    private void UpdatePlacementPreview(PointerEventData eventData)
    {
        if (gridManager == null || blockView == null || blockView.Data == null)
            return;

        // Luôn giữ block đang kéo là asset màu thật
        blockView.SetInvalidDragVisual(false);

        if (!blockView.TryGetReferencePiece(out Vector2Int referenceOffset, out RectTransform referencePiece))
        {
            gridManager.ClearPlacementPreview();
            return;
        }

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : eventData.pressEventCamera;

        Vector2 referenceScreenPosition = RectTransformUtility.WorldToScreenPoint(
            eventCamera,
            referencePiece.position
        );

        bool hasOrigin = gridManager.TryGetPlacementOriginFromBlockPiece(
            blockView.Data,
            referenceOffset,
            referenceScreenPosition,
            eventCamera,
            snapDistance,
            out int originRow,
            out int originCol
        );

        if (!hasOrigin)
        {
            gridManager.ClearPlacementPreview();
            return;
        }

        gridManager.ShowPlacementPreview(
            blockView.Data,
            blockView.SpawnedColorState,
            originRow,
            originCol
        );
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (canvasRect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    private void ReturnToSlot(bool playInvalidSound)
    {
        if (playInvalidSound)
        {
            GameAudioSystem.Instance?.PlayInvalidDrop();
        }

        // Khi quay về khay, block chắc chắn trở lại màu asset thật
        blockView.SetInvalidDragVisual(false);

        if (gridManager != null)
        {
            gridManager.ClearPlacementPreview();
        }

        canvasGroup.blocksRaycasts = true;

        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        rectTransform.anchoredPosition = Vector2.zero;
        transform.localScale = originalScale;
    }
}