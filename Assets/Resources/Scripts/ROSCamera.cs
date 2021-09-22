using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class ROSCamera : MonoBehaviour
{
    private ROSConnection ros;
    private RenderTexture renderTexture;
    private Camera attachedCamera;
    private float timeSinceLastUpdate = 0;

    public string imageTopic;
    public uint imageWidth = 640;
    public uint imageHeight = 480;
    public float updatesPerSecond = 10;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.instance;
        ros.RegisterPublisher<ImageMsg>(imageTopic);
        attachedCamera = this.gameObject.GetComponent<Camera>();

        renderTexture = new RenderTexture((int)imageWidth, (int)imageHeight, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB);
        renderTexture.Create();
        attachedCamera.targetTexture = renderTexture;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate > (1 / updatesPerSecond))
        {
            //send image data
            var oldRT = RenderTexture.active;
            RenderTexture.active = attachedCamera.targetTexture;
            attachedCamera.Render();

            // Copy the pixels from the GPU into a texture so we can work with them
            // For more efficiency you should reuse this texture, instead of creating a new one every time
            Texture2D camText = new Texture2D(renderTexture.width, renderTexture.height,
                                                UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SRGB,
                                                UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            camText.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            camText.Apply();
            RenderTexture.active = oldRT;
            RenderTexture.active = attachedCamera.targetTexture;

            // Encode the texture as a PNG, and send to ROS
            byte[] imageBytes = camText.GetRawTextureData();
            HeaderMsg header = new HeaderMsg();
            ImageMsg message = new ImageMsg(header, (uint)renderTexture.height, (uint)renderTexture.width, "rgba8", 0, imageWidth * 4, imageBytes);
            ros.Send<ImageMsg>(imageTopic, message);
            timeSinceLastUpdate = 0;
        }
    }
}
