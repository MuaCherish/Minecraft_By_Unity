using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera)), ExecuteInEditMode]
[AddComponentMenu("Effects/Crepuscular Rays", -1)]
public class Crepuscular : MonoBehaviour
{

	public Material material;
	public GameObject _light;

	// Start is called before the first frame update
	void Start()
    {
        
    }

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetVector("_LightPos", GetComponent<Camera>().WorldToViewportPoint(transform.position - _light.transform.forward));
		Graphics.Blit(source, destination, material);
	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
