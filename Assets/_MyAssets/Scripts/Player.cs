using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [SerializeField] private float _playerSpeed = 8f;
    public float PlayerSpeed => _playerSpeed;
    private SpriteRenderer _spriteRenderer;
    
}
