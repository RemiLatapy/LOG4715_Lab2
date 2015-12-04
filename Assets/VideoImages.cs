using UnityEngine;
using System.Collections;

public class VideoImages : MonoBehaviour {

	public string imageFolderName;
	public bool loop = true;
	
	bool MakeTexture = true;
	ArrayList pictures;
	int counter = 0;
	float nextPic = 0;
	float PictureRateInSeconds = 0.03333f;

	Object[] textures;

	// Use this for initialization
	void Start () {
		textures = Resources.LoadAll(imageFolderName, typeof(Material));
	}
	
	// Update is called once per frame
	void Update () {

		if (counter >= textures.Length) {
			if (loop) {
				counter = 0;
			}
		}

		if (Time.time > nextPic && counter < textures.Length) {
			if (MakeTexture) {
				renderer.material = (Material)textures [counter];
				renderer.material.shader = Shader.Find("Self-Illumin/Diffuse");
			}
			nextPic = Time.time + PictureRateInSeconds;
			counter ++;
		}
	}
}
	