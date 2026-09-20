using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 2D 패럴랙스 배경 레이어.
    /// 카메라 X 이동분에 (1 - parallaxFactor)를 곱해 레이어를 월드에서 이동시킨다.
    ///  - parallaxFactor = 1  → 월드와 완전 동기 (가장 가까운 레이어, 플레이어처럼 보임)
    ///  - parallaxFactor = 0  → 화면에 고정 (가장 먼 레이어, 무한히 먼 하늘 느낌)
    /// 스프라이트(출처)를 tileCount장 복제해 가로로 늘어놓고,
    /// 카메라 위치에 따라 타일을 랩핑(wrap)해 항상 화면을 덮게 한다.
    /// Y축은 카메라를 1:1 따라가도록 고정해 배경이 상하 프레임을 유지한다.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax")]
        [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.5f;

        [Header("Sprite")]
        [SerializeField] private SpriteRenderer sourceSprite;
        [SerializeField] private float tileScale = 1f;   // 이미지 1장의 월드 배율 (640x360/PPU100 → 3배면 화면 폭 일치)
        [SerializeField] private int tileCount = 3;        // 가로 반복 장수 (화면 폭 19.2 / 타일 폭 6.4 → 최소 3)
        [SerializeField] private int sortingOrder = -100;  // 월드보다 뒤에 그리기

        private Transform camTransform;
        private float startCamX;
        private float startY;
        private float tileWidth;
        private SpriteRenderer[] tiles;

        private void Awake()
        {
            if (sourceSprite == null)
            {
                Debug.LogError("[ParallaxBackground] sourceSprite is null on " + name, this);
                return;
            }

            var cam = Camera.main;
            if (cam != null)
            {
                camTransform = cam.transform;
                startCamX = camTransform.position.x;
            }
            startY = transform.position.y;

            float srcWidth = sourceSprite.sprite != null
                ? sourceSprite.sprite.bounds.size.x
                : 6.4f;
            tileWidth = srcWidth * tileScale;
            if (tileWidth <= 0f) tileWidth = 6.4f;

            // 출처 스프라이트를 템플릿으로 가로 복제
            tiles = new SpriteRenderer[tileCount];
            for (int i = 0; i < tileCount; i++)
            {
                var go = new GameObject("Tile_" + i, typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = sourceSprite.sprite;
                sr.sortingOrder = sortingOrder;
                sr.sortingLayerID = sourceSprite.sortingLayerID;
                sr.color = sourceSprite.color;
                go.transform.localPosition = new Vector3(i * tileWidth, 0f, 0f);
                go.transform.localScale = new Vector3(tileScale, tileScale, 1f);
                tiles[i] = sr;
            }

            // 템플릿 스프라이트는 숨긴다
            sourceSprite.enabled = false;

            // 시작 위치 정렬
            AlignTiles(0f);
        }

        private void LateUpdate()
        {
            if (camTransform == null)
            {
                var cam = Camera.main;
                if (cam == null) return;
                camTransform = cam.transform;
                startCamX = camTransform.position.x;
                return;
            }

            float camDeltaX = camTransform.position.x - startCamX;

            // Y는 카메라를 1:1 따라간다 (상하 프레임 유지)
            transform.position = new Vector3(transform.position.x, camTransform.position.y, transform.position.z);

            AlignTiles(camDeltaX);
        }

        /// <summary>
        /// 카메라 이동량에 (1 - parallaxFactor)를 반영한 오프셋으로 타일들을 랩핑 정렬한다.
        /// 타일 블록의 왼쪽 끝을 카메라 기준으로 정수 배수에 맞춰 이음새를 숨긴다.
        /// </summary>
        private void AlignTiles(float camDeltaX)
        {
            if (tiles == null || tiles.Length == 0) return;

            float layerOffset = camDeltaX * (1f - parallaxFactor);

            // 블록 시작을 카메라 시점(레이어 오프셋 반영) 기준 타일 폭 정수 배수로 맞춘다.
            float anchorX = Mathf.Floor(layerOffset / tileWidth) * tileWidth;

            for (int i = 0; i < tiles.Length; i++)
            {
                float x = anchorX + i * tileWidth;
                tiles[i].transform.position = new Vector3(x, transform.position.y, 0f);
            }
        }
    }
}