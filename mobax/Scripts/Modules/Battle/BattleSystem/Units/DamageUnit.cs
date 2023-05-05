using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.Core;
using HitType = BattleEngine.Logic.HitType;

public class DamageUnit : MonoBehaviour
{
    public Text damageText;
    public Outline damageOutLine;
    public Animation anim;
    public GameObject weak_image;
    public Image element_image;
    public GameObject block_image;
    public GameObject break_image;

    public Font normalFont;
    public Font greenFont;
    public Font redFont;
    public Font yellowFont;

    public Text weak;
    /* private static Vector3 Vector40 = new Vector3(4f, 4f, 4f);
       private static Vector3 VectorM03 = new Vector3(-3f, -3f, -3f);*/
    private static Color ColorSelfCrit = new Color(255 / 255f, 67 / 255f, 45 / 255f, 1f);
    private static Color ColorNormal = Color.white;
    private static Color ColorCrit = Color.red;
    private static Color ColorTreat = Color.green;
    private static Color OutLineC1 = ColorUtil.HexToColor("195040");
    private static Color OutLineC2 = ColorUtil.HexToColor("3c123d");
    private static Color OutLineC3 = ColorUtil.HexToColor("444444");
    private static Color OutLineC4 = ColorUtil.HexToColor("3d2b12");
    private static Color OutLineC5 = ColorUtil.HexToColor("195014");

    public async Task PlayAnim(bool isSelfHero, BDamageAction data, Creature target, bool isLeft)
    {
        if (target == null)
            return;

        //职业克制
        if (data.HasState(HitType.Weak))
        {
            weak_image.SetActive(true);
        }
        else
        {
            weak_image.SetActive(false);
        }
        block_image.SetActive(false);
        break_image.SetActive(false);
        element_image.SetActive(false);
        if (data.HasState(HitType.Fire))
        {
            element_image.SetActive(true);
            UiUtil.SetSpriteInBackground(this.element_image, () => "hero_attr_1.png", null, 1, null, false, false);
        }
        if (data.HasState(HitType.Electric))
        {
            element_image.SetActive(true);
            UiUtil.SetSpriteInBackground(this.element_image, () => "hero_attr_2.png", null, 1, null, false, false);
        }
        if (data.HasState(HitType.Water))
        {
            element_image.SetActive(true);
            UiUtil.SetSpriteInBackground(this.element_image, () => "hero_attr_3.png", null, 1, null, false, false);
        }
        if (data.HasState(HitType.Wind))
        {
            element_image.SetActive(true);
            UiUtil.SetSpriteInBackground(this.element_image, () => "hero_attr_4.png", null, 1, null, false, false);
        }
        if (data.HasState(HitType.Light))
        {
            element_image.SetActive(true);
            UiUtil.SetSpriteInBackground(this.element_image, () => "hero_attr_5.png", null, 1, null, false, false);
        }
        if (data.HasState(HitType.Dark))
        {
            element_image.SetActive(true);
            UiUtil.SetSpriteInBackground(this.element_image, () => "hero_attr_6.png", null, 1, null, false, false);
        }
        string damage = "";
        if (data.damage != 0)
            damage = data.damage.ToString();
        if (data.HasState(HitType.Break))
        {
            break_image.SetActive(true);
            /*if(target.IsSelf())
                await this.PlayAnimAsync(damage, Color.red, "RecoverAnim");
            else
                await this.PlayAnimAsync(damage, Color.red, "HeroDamageAnim");*/
            await this.PlayAnimAsync(damage, "RecoverAnim");
            return;
        }

        //治疗
        /*if (data.HasState(HitType.Cure))
        {
            await this.PlayAnimAsync(damage, Color.green, "DamageMyself");
            return;
        }*/

        //格挡
        if (data.HasState(HitType.Block))
        {
            damage = damage;
            block_image.SetActive(true);
        }
        if (data.HasState(HitType.XiShou))
            damage = LocalizationManager.Stuff.GetText("M4_state_shield");
        if (data.HasState(HitType.Suck))
            damage = LocalizationManager.Stuff.GetText("M4_state_absorb");
        if (data.HasState(HitType.AntDamage))
            damage = LocalizationManager.Stuff.GetText("M4_state_feedback");
        if (data.HasState(HitType.Angry))
            damage = LocalizationManager.Stuff.GetText("M4_state_rage");
        if (data.HasState(HitType.Dun))
            damage = LocalizationManager.Stuff.GetText("M4_state_stop");
        if (data.HasState(HitType.Stun))
            damage = LocalizationManager.Stuff.GetText("M4_state_dizz");
        if (data.HasState(HitType.MinusDamage))
            damage = LocalizationManager.Stuff.GetText("M4_state_def");
        if (target.IsSelf())
        {
            if (data.HasState(HitType.Cure))
            {
                //c = ColorUtil.HexToColor("6ef56e");
                //damageOutLine.effectColor = OutLineC2;
                //this.damageText.font = greenFont;
                this.damageText.color = ColorUtil.HexToColor("A1FF97");
                await this.PlayAnimAsync(damage, "RecoverAnim");
            }
            else
            {
                //c = Color.red;
                //this.damageText.font = redFont;
                this.damageText.color = ColorUtil.HexToColor("FD6D68");
                await this.PlayAnimAsync(damage, "DamageMyself");
            }
            return;
        }
        else
        {
            string anim = "DamageAnimTest";
            if (UnityEngine.Random.Range(0, 100) > 50)
                anim = "DamageAnimTest";
            if (data.HasState(HitType.Skill))
            {
                //c = Color.yellow;
                //damageOutLine.effectColor = OutLineC4;
                //this.damageText.font = yellowFont;
                this.damageText.color = ColorUtil.HexToColor("FFB067");
            }
            else if (data.HasState(HitType.Cure))
            {
                //c = ColorUtil.HexToColor("6ef56e");
                //damageOutLine.effectColor = OutLineC1;
                //this.damageText.font = greenFont;
                this.damageText.color = ColorUtil.HexToColor("A1FF97");
                await this.PlayAnimAsync(damage, "RecoverAnim");
                return;
            }
            else
            {
                //damageOutLine.effectColor = OutLineC3;
                //c = Color.white;
                //this.damageText.font = normalFont;
                this.damageText.color = Color.white;
            }

            //暴击
            if (!target.IsSelf()
                && data.HasState(HitType.Crit))
            {
                damage = damage + "!";
                //damageText.color = ColorUtil.HexToColor("FDBB0B");
                /*if(isLeft)
                    await this.PlayAnimAsync(damage, "DamageAnimCritTest", true);
                else
                    await this.PlayAnimAsync(damage, "DamageAnimCritTest", true);*/
                await this.PlayAnimAsync(damage, "DamageAnimCritTest", true);
                return;
            }

            //await this.PlayAnimAsync(damage, c, anim);
            /*if(isLeft)
                await this.PlayAnimAsync(damage, "DamageAnimTest", true);
            else
                await this.PlayAnimAsync(damage, "DamageAnimTest", true);*/
            await this.PlayAnimAsync(damage, "DamageAnimTest", true);
        }
    }

    private async Task PlayAnimAsync(string info, string anim, bool isMonster = false)
    {
        this.damageText.text = info;
        //this.damageText.color = c;
        this.anim.Play(anim);
        float s = UnityEngine.Random.Range(0.9f, 1.1f);
        float xoffset = 10f;
        float yoffset = 10f;
        float x = UnityEngine.Random.Range(-xoffset, xoffset);
        float y = UnityEngine.Random.Range(-yoffset, yoffset);
        //this.transform.localScale =  s * Vector3.one * 0.7f;
        if (isMonster)
        {
            var pos = DamageManager.Instance.GetNextOffset();
            this.transform.localPosition += new Vector3(pos.x + x, pos.y + y, 0);
        }
        else
        {
            this.transform.localPosition += new Vector3(x, y, 0);
        }
        var clip = this.anim.GetClip(anim);
        if (clip != null)
            await Task.Delay(Mathf.FloorToInt(this.anim.GetClip(anim).averageDuration * 1000));
        /*   var tcs = new TaskCompletionSource<bool>();
           Transform tmp_instance = this.damageText.transform;
           Sequence seq = DOTween.Sequence();
           Tweener move = tmp_instance.DOBlendableMoveBy(new Vector3(1f * (2f + 1f * UnityEngine.Random.Range(-1f, 1f)),
               2f + 0.5f * UnityEngine.Random.Range(0f, 1.5f), 0f), 0.7f).SetEase(Ease.OutQuart);
           Tweener scale = tmp_instance.DOBlendableScaleBy(Vector40, 0.2f).SetEase(Ease.InQuart);
           Tweener scale2 = tmp_instance.DOBlendableScaleBy(VectorM03, 0.1f).SetEase(Ease.OutQuart).SetDelay(0.2f);
   
           Tweener fade = damageText.DOFade(0, 0.2f).SetDelay(0.5f);
   
           seq.Append(move);
           seq.Join(scale);
           seq.Join(scale2);
           seq.Join(fade);
   
           seq.OnComplete(() =>
           {
               tmp_instance.transform.localScale = Vector3.one;
               tcs.SetResult(true);
           });
           return tcs.Task;*/
    }
}