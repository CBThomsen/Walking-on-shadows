using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Raytracer : MonoBehaviour
{
    public ComputeShader computeShader;
    public SceneGeometry sceneGeometry;

    private ComputeBuffer lightBuffer;
    private ComputeBuffer circleBuffer;

    private int raytracerKI;
    public RenderTexture destTexture;

    private float cpuTimer = 0f;
    private float cpuFPS = 0.0001f;

    public int textureResolution = 1024;
    private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    void Start()
    {
        cpuTimer = 1f / cpuFPS;
        raytracerKI = computeShader.FindKernel("Raytracer");

        SetupRenderTexture();
    }

    private void onDestroy()
    {
        lightBuffer.Release();
    }

    private void SetupRenderTexture()
    {
        destTexture = new RenderTexture(textureResolution, textureResolution, 24, RenderTextureFormat.ARGB32);
        destTexture.enableRandomWrite = true;
        destTexture.Create();

        computeShader.SetTexture(raytracerKI, "destTexture", destTexture);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(destTexture, dest);
    }

    void Update()
    {
        //Husk at transforme positioner! Normalize -> gange resolution på positioner!
        LightData[] lightDataArray = this.sceneGeometry.GetLightDataArray();
        lightBuffer = new ComputeBuffer(1, 2 * sizeof(float));
        lightBuffer.SetData(lightDataArray);
        computeShader.SetBuffer(raytracerKI, "lights", lightBuffer);

        CircleData[] circleDataArray = this.sceneGeometry.GetCircleDatas();
        circleBuffer = new ComputeBuffer(1, 3 * sizeof(float));
        circleBuffer.SetData(circleDataArray);
        computeShader.SetBuffer(raytracerKI, "circles", circleBuffer);

        computeShader.Dispatch(raytracerKI, textureResolution / 32, textureResolution / 32, 1);

        circleBuffer.Release();
        lightBuffer.Release();

        return;

        cpuTimer += Time.deltaTime;
        if (cpuTimer > 1f / cpuFPS)
        {
            Debug.Log("Updating CPU ray tracer!");
            cpuTimer -= 1f / cpuFPS;

            for (int x = -10; x < 10; x++)
            {
                for (int y = -10; y < 10; y++)
                {
                    RayTracePixel(x, y);
                    //Debug.Log("Ray tracing " + x + ", " + y);
                }
            }
        }
    }

    void DoCPUTracing(int FPS)
    {

    }

    void RayTracePixel(int x1, int y1)
    {
        LightData[] lightDataArray = this.sceneGeometry.GetLightDataArray();

        for (var i = 0; i < lightDataArray.Length; i++)
        {
            Vector2 lightPos = lightDataArray[i].position;
            float x2 = lightPos.x;
            float y2 = lightPos.y;

            Vector2 direction = new Vector2(x2 - x1, y2 - y1);
            float dist = Mathf.Sqrt(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.y, 2));
            Vector2 directionNormalized = direction / dist;

            CircleData[] circles = this.sceneGeometry.GetCircleDatas();

            for (int j = 0; j < circles.Length; j++)
            {
                int intersections = CPUIntersections.TestRaySphere(new Vector2(x1, y1), directionNormalized, circles[j].center, circles[i].radius);

                if (intersections > 0)
                {
                    Debug.DrawLine(new Vector2(x1, y1), lightPos, Color.red, 1f / cpuFPS);
                }
                else
                {
                    Debug.DrawLine(new Vector2(x1, y1), lightPos, Color.green, 1 / cpuFPS);
                }
            }
        }
    }
}