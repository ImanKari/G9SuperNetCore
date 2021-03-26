using UnityEngine;
using UnityEngine.UI;

public class FrameAttackHandler : MonoBehaviour
{

    public GameCharacterHandler Player;

    public Image AttackImage;

    public float Countdown = 9;

    public AudioSource Attack;

    private float _tempTime;

    private bool enableFillAttack;

    // Update is called once per frame
    private void Update()
    {
        if (enableFillAttack)
        {
            var coef = 1 / Countdown;
            var timeCalc = Time.time - _tempTime;
            AttackImage.fillAmount = timeCalc * coef;
            if (timeCalc > Countdown)
            {
                AttackImage.color = Color.yellow;
                enableFillAttack = false;
            }
        }

    }

    public void UseAttack()
    {
        if (enableFillAttack || !Player.Attack()) return;
        Attack.Play();
        AttackImage.fillAmount = 0;
        AttackImage.color = Color.gray;
        _tempTime = Time.time;
        enableFillAttack = true;
    }

}