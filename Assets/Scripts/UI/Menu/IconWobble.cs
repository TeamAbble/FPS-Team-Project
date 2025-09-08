using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconWobble : MonoBehaviour
{
    public Vector3 angle;
    public Vector3 speed;
    public bool useCosForZ;
    public Vector3 posScale, eulerScale;

    public bool useRandomStart;

    Vector3 startPos;

    public Vector3 waveOutput;

    private void Awake()
    {
        startPos = transform.localPosition;
        SetInitAngle();
    }
    void SetInitAngle()
    {
        if (useRandomStart)
        {
            angle = new Vector3()
            {
                x = Random.Range(-360, 360),
                y = Random.Range(-360, 360),
                z = Random.Range(-360, 360),
            };
        }
        else
        {
            angle = Vector3.zero;
        }
    }

    private void Update()
    {
        angle += speed * Time.deltaTime;
        angle = new(angle.x % 360, angle.y % 360, angle.z % 360);
        waveOutput.x = Mathf.Sin(angle.x * Mathf.Deg2Rad);
        waveOutput.y = Mathf.Cos(angle.y * Mathf.Deg2Rad);
        waveOutput.z = useCosForZ ? Mathf.Cos(angle.z * Mathf.Deg2Rad) : Mathf.Sin(angle.z * Mathf.Deg2Rad);
        transform.SetLocalPositionAndRotation(startPos + new Vector3(waveOutput.x * posScale.x, waveOutput.y * posScale.y, 0),
            Quaternion.Euler(waveOutput.x * eulerScale.x, waveOutput.y * eulerScale.y, waveOutput.z * eulerScale.z));
    }
    private void OnValidate()
    {
        SetInitAngle();
    }
}
