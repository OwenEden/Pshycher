using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour, LivingEntity
{
    [Header("체력")]
    public Image currentHealthBar;
    public float maxHealth = 100f;
    public float currentHealth { get; protected set; } 
    public bool dead { get; protected set; }

    [Header("마나")]
    public Image currentManaBar;
    public float maxMana = 100f;
    public float currentMana { get; protected set; }

    private void Start()
    {
        InitialSet();
    }

    public void InitialSet()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        dead = false;
    }
    public void CheckHp()
    {
        if (currentHealthBar != null)
            currentHealthBar.fillAmount = currentHealth / maxHealth;
    }
    public void OnDamage(float damage)
    {
        currentHealth -= damage; // health = health - damage;
        CheckHp();
        Debug.Log(this.gameObject.name + " take Damage.");
    }
}
