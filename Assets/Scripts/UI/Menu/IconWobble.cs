using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconWobble : MonoBehaviour
{
    public float angle;
    public Vector3 speed;

    public Vector3 posScale, eulerScale;

    RectTransform rect;
    Vector3 startPos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.localPosition;
        angle = Random.Range(0, 90);
    }

    private void Update()
    {
        angle += Time.deltaTime;
        rect.SetLocalPositionAndRotation(startPos + new Vector3(Mathf.Sin(angle * speed.x) * posScale.x, Mathf.Cos(angle * speed.y) * posScale.y, 0), 
            Quaternion.Euler(Mathf.Sin(angle * speed.y) * eulerScale.x, Mathf.Cos(angle * speed.y) * eulerScale.y, Mathf.Sin(angle * speed.z) * eulerScale.z));
        angle %= 90;
    }
}
