public interface LivingEntity
{
    float currentHealth { get; }
    bool dead { get; }
    void OnDamage(float damage);
}