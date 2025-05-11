using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FollowCamera : MonoBehaviour
{
    public Transform player;
    private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField]
    private float smoothSpeed = 0.125f;
    private Vector3 velocity = Vector3.zero;

    /// <summary>
    /// Probably the most cringe thing that I have ever done
    /// Please do not judge me
    /// </summary>
    public FullScreenPassRendererFeature RenderFeature;

    [SerializeField]
    private Material baseDistortMaterial;

    private void Awake()
    {
        // Do not tell anyone about this, if you do I will find out and end you
        FullScreenPassRendererFeature renderFeature = GetComponent<FollowCamera>().RenderFeature;
        renderFeature.passMaterial = baseDistortMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.SmoothDamp(transform.position, player.position + offset, ref velocity, smoothSpeed);
    }
}
