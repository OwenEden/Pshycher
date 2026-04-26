using UnityEngine;
public interface LivingEntity
{
    public void InitialSet();
    public void CheckHp();
    public void OnDamage(float damage);

}