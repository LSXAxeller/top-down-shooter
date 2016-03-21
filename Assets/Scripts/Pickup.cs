using UnityEngine;
using Deftly;
using System.Collections;

public class Pickup : MonoBehaviour {

    public GameObject WeaponPrefab;
    public GameObject WeaponDroppedPrefab;
    public float respawnTime = 5.0f;

    private SpriteRenderer spr;
    private bool isPickable = false;

    private void Start()
    {
        spr = GetComponentInChildren<SpriteRenderer>();
        spr.sprite = WeaponDroppedPrefab.GetComponentInChildren<SpriteRenderer>().sprite;
        isPickable = true;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isPickable != true) return;

        var subject = col.gameObject.GetComponentInParent<Subject>();
        subject.PickupWeapon(WeaponPrefab);
        subject.ChangeWeaponToSlot(0);

        StartCoroutine(Timeout());
    }

    private IEnumerator Timeout()
    {
        isPickable = false;
        spr.color = new Color(1, 1, 1, 0);
        yield return new WaitForSeconds(respawnTime);
        spr.color = Color.white;
        isPickable = true;
    }
}
