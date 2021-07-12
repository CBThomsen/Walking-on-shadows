using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct Agent
{
    public Vector2 position;
    public float angle;
}

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader drawAgentsComputeShader;
    public ComputeShader clearTextureComputeShader;

    private RenderTexture renderTexture;

    private int drawAgentsKI;
    private int clearTextureKI;

    private Agent[] agentDataArray;
    private ComputeBuffer agentBuffer;
    private int agents = 50000;

    private void Start()
    {
        drawAgentsKI = drawAgentsComputeShader.FindKernel("DrawAgents");
        clearTextureKI = clearTextureComputeShader.FindKernel("ClearTexture");

        renderTexture = new RenderTexture(1024, 1024, 32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        agentDataArray = new Agent[agents];
        for (var i = 0; i < agentDataArray.Length; i++)
        {
            agentDataArray[i].position = new Vector2(renderTexture.width / 2, renderTexture.height / 2) + Random.insideUnitCircle * 1f;
            agentDataArray[i].angle = Random.Range(0f, Mathf.PI * 2f);
        }

        agentBuffer = new ComputeBuffer(agents, 3 * sizeof(float));
        agentBuffer.SetData(agentDataArray);
        drawAgentsComputeShader.SetBuffer(drawAgentsKI, "Agents", agentBuffer);
        drawAgentsComputeShader.SetInt("Width", renderTexture.width);
        drawAgentsComputeShader.SetInt("Height", renderTexture.height);
        drawAgentsComputeShader.SetInt("NumAgents", agents);
        drawAgentsComputeShader.SetFloat("DeltaTime", Time.deltaTime);

        clearTextureComputeShader.SetTexture(clearTextureKI, "tex", renderTexture);
        drawAgentsComputeShader.SetTexture(drawAgentsKI, "tex", renderTexture);
    }

    private void OnDestroy()
    {
        if (agentBuffer != null)
            agentBuffer.Release();
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        clearTextureComputeShader.Dispatch(clearTextureKI, renderTexture.width / 8, renderTexture.height / 8, 1);
        drawAgentsComputeShader.Dispatch(drawAgentsKI, agents / 16, 1, 1);

        Graphics.Blit(renderTexture, dest);
    }


}