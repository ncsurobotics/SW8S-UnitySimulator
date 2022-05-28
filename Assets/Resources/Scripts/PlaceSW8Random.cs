using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlaceSW8Random : MonoBehaviour
{
    public GameObject sw8;
    public GameObject gate;
    public Camera cam;
    public float updatesPerSecond = 10;

    public float minX = -61.0f;
    public float maxX = 90.0f;
    public float minY = -9.0f;
    public float maxY = 3.3f;
    public float maxZ = -5.0f;
    public float minZ = -55.0f;

    public float minGateForwardOffset = 15.0f;
    public float maxGateForwardOffset = 30.0f;
    public float minGateSideOffset = -4.0f;
    public float maxGateSideOffset = 15.0f;
    public float minGateUpOffset = -4.0f;
    public float maxGateUpOffset = -4.0f;

    private float timeSinceLastUpdate = 0;
    private int FileCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        RenderTexture renderTexture = new RenderTexture((int) 1280, (int)800, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
        renderTexture.Create();
        cam.targetTexture = renderTexture;
    }

    // Update is called once per frame
    void Update()
    { 
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate > (1 / updatesPerSecond))
        { 
            Vector3 sw8Pos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
            sw8.transform.position = sw8Pos;
            sw8.transform.eulerAngles = new Vector3(0, Random.Range(0.0f, 365.0f), 0);
            gate.transform.position = sw8.transform.position;
            gate.transform.rotation = sw8.transform.rotation;

            gate.transform.position += gate.transform.up * -4.0f + gate.transform.forward * Random.Range(minGateForwardOffset, maxGateForwardOffset) + gate.transform.right * Random.Range(minGateSideOffset, maxGateSideOffset);

            CamCapture();
            timeSinceLastUpdate = 0;
        }
    }

    void CamCapture()
    {
 
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;
 
        cam.Render();

        Texture2D Image = new Texture2D(1280, 800);
        Image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
        Image.Apply();
        RenderTexture.active = currentRT;
 
        var Bytes = Image.EncodeToPNG();
        Destroy(Image);
 
        File.WriteAllBytes(Application.dataPath + "/trainData/" + FileCounter + ".png", Bytes);
        FileCounter++;
    }
}
