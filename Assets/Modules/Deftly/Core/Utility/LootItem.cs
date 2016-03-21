using Deftly;
using UnityEngine;

public class LootItem : MonoBehaviour
{
    public LayerMask CanUseThis;
    public bool Animation;
    public AudioSource Sound;
    public int CoinRewardMin;
    public int CoinRewardMax;
    public bool DestroyOnPickup;

    private bool _expended;
    private Subject _pirate;

    void OnTriggerEnter(Collider col)
    {
        if (!StaticUtil.LayerMatchTest(CanUseThis, col.gameObject) | _expended) return;
        _expended = true;

        _pirate = col.gameObject.GetComponent<Subject>();
        if (Animation) GetComponent<Animation>().Play();
        if (Sound) Sound.Play();
        int reward = Random.Range(CoinRewardMin, CoinRewardMax);
        _pirate.Stats.Coin += reward;
        StaticUtil.SpawnFloatingText(gameObject, transform.position, reward.ToString());

        if (DestroyOnPickup) Destroy(gameObject);
    }
}