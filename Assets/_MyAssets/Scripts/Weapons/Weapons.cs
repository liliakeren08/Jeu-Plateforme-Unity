using UnityEngine;

public abstract class Weapons : MonoBehaviour
{
    protected Player _player;

    public virtual void Init(Player player)
    {
        _player = player;
    }

    public abstract void Attack();
}