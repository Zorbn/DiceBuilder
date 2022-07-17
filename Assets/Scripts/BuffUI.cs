using TMPro;
using UnityEngine;

public class BuffUI : MonoBehaviour
{
    public World world;
    public TMP_Text text;
    
    private void Update()
    {
        text.text = $"+{world.BuffData.CoreHealth}\n" +
                    $"+{world.BuffData.Damage}\n" +
                    $"+{world.BuffData.AttackSpeed}\n" +
                    $"+{world.BuffData.EnemyHealth}\n" +
                    $"+{world.BuffData.EnemyAttackSpeed}";
    }
}
