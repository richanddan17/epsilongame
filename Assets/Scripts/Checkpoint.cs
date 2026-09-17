using UnityEngine;

namespace EpsilonGame
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private bool _isActivated = false;
        [SerializeField] private Color _inactiveColor = Color.gray;
        [SerializeField] private Color _activeColor = Color.green;
        
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            
            // Ensure collider is trigger
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
            
            UpdateVisual();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isActivated) return;
            
            if (other.CompareTag("Player"))
            {
                ActivateCheckpoint();
            }
        }

        private void ActivateCheckpoint()
        {
            _isActivated = true;
            
            // Call GameManager to save checkpoint position
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCheckpoint(transform.position);
            }
            else
            {
                Debug.LogWarning("[Checkpoint] GameManager.Instance is null. Checkpoint not saved.");
            }
            
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _isActivated ? _activeColor : _inactiveColor;
            }
        }

        // Public method to reset checkpoint (for testing or level restart)
        public void ResetCheckpoint()
        {
            _isActivated = false;
            UpdateVisual();
        }
    }
}