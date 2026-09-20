using UnityEngine;

namespace EpsilonGame
{
    /// <summary>
    /// 2D 패럴랙스 배경 레이어 (무한 래핑).
    /// 카메라 X 이동분에 (1 - parallaxFactor)를 곱해 레이어를 패턴 공간에서 이동시킨다.
    ///  - parallaxFactor = 1  → 월드와 완전 동기 (가장 가까운 레이어)
    ///  - parallaxFactor = 0  → 화면에 고정 (가장 먼 레이어)
    /// 스프라이트(출처)를 가로로 복제하고, 카메라 시야 + 여유 1장씩을 덮도록
    /// 화면 폭에 맞춰 타일 수를 자동 확장한다. 카메라가 어디로 가든 배경이 끊기지 않는다.
    /// Y축은 카메라를 1:1 따라가도록 고정해 배경이 상하 프레임을 유지한다.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax")]
        [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.5f;

        [Header("Sprite")]
        [SerializeField] private SpriteRenderer sourceSprite;
        [SerializeField] private float tileScale = 1f;   // 이미지 1장의 월드 배율 (640x360/PPU100 → 3배면 화면 폭 일치)
        [Tooltip("가로 반복 최소 장수. 화면 폭에 따라 자동으로 늘어난다(양옆 여유 1장 포함).")]
        [SerializeField] private int tileCount = 3;
        [SerializeField] private int sortingOrder = -100;  // 월드보다 뒤에 그리기

        private Transform camTransform;
        private float startCamX;
        private float startY;
        private float tileWidth;
        private float viewHalfWidth;
        private SpriteRenderer[] tiles;
        private int poolSize;

        private void Awake()
        {
            if (sourceSprite == null)
            {
                Debug.LogError("[ParallaxBackground] sourceSprite is null on " + name, this);
                return;
            }

            var cam = Camera.main;
            viewHalfWidth = 9.6f;
            if (cam != null)
            {
                camTransform = cam.transform;
                startCamX = camTransform.position.x;
                viewHalfWidth = cam.orthographicSize * cam.aspect;
            }
            startY = transform.position.y;

            float srcWidth = sourceSprite.sprite != null
                ? sourceSprite.sprite.bounds.size.x
                : 6.4f;
            tileWidth = srcWidth * tileScale;
            if (tileWidth <= 0f) tileWidth = 6.4f;

            // 화면 폭을 덮기에 필요한 장수 + 양옆 여유 포함(+4: floor/ceil 경계에서 ±1 변동 흡수)
            int needed = Mathf.CeilToInt((viewHalfWidth * 2f) / tileWidth) + 4;
            poolSize = Mathf.Max(tileCount, needed);
            if (poolSize < 4) poolSize = 4;

            // 출처 스프라이트를 템플릿으로 가로 복제 (풀)
            tiles = new SpriteRenderer[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject("Tile_" + i, typeof(SpriteRenderer));
                go.transform.SetParent(transform, false);
                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = sourceSprite.sprite;
                sr.sortingOrder = sortingOrder;
                sr.sortingLayerID = sourceSprite.sortingLayerID;
                sr.color = sourceSprite.color;
                go.transform.localScale = new Vector3(tileScale, tileScale, 1f);
                tiles[i] = sr;
            }

            // 템플릿 스프라이트는 숨긴다
            sourceSprite.enabled = false;

            // 시작 위치 정렬
            AlignTiles();
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

            // Y는 카메라를 1:1 따라간다 (상하 프레임 유지)
            transform.position = new Vector3(transform.position.x, camTransform.position.y, transform.position.z);

            AlignTiles();
        }

        /// <summary>
        /// 패턴 공간에서 무한 래핑 정렬.
        /// 타일 k의 월드 X = patternOrigin + k * tileWidth (k 정수).
        /// 카메라 시야 양끝에 여유 1장씩을 항상 포함하도록 k 범위를 계산한다.
        /// </summary>
        private void AlignTiles()
        {
            if (tiles == null || tiles.Length == 0) return;
            if (camTransform == null) return;

            float camX = camTransform.position.x;
            float camDeltaX = camX - startCamX;

            // 패턴 원점: 카메라 시작점 기준, 레이어 속도(1-factor)로 이동 (월드 좌표)
            float patternOrigin = camDeltaX * (1f - parallaxFactor);

            float camLeft = camX - viewHalfWidth;
            float camRight = camX + viewHalfWidth;

            // 첫 타일 왼쪽 끝이 화면 왼쪽 - 타일 1장 보다 왼쪽에 오도록 시작 인덱스
            int startK = Mathf.FloorToInt((camLeft - tileWidth - patternOrigin) / tileWidth);
            // 마지막 타일 오른쪽 끝이 화면 오른쪽 + 타일 1장 보다 오른쪽에 오도록 끝 인덱스
            int endK = Mathf.CeilToInt((camRight + tileWidth - patternOrigin) / tileWidth);

            for (int k = startK; k <= endK; k++)
            {
                int idx = k % poolSize;
                if (idx < 0) idx += poolSize;
                if (tiles[idx] == null) continue;

                float x = patternOrigin + k * tileWidth;
                tiles[idx].transform.position = new Vector3(x, transform.position.y, 0f);
            }
        }
    }
}