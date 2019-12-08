using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraEffectBase
{
    protected CameraEffectManager m_Manager { get; private set; }
    public bool m_Supported { get; private set; }
    public virtual bool m_IsPostEffect => false;
    public CameraEffectBase()
    {
        m_Supported=OnCreate();
    }
    protected virtual bool OnCreate()
    {
        return true;
    }
    public virtual void OnSetEffect(CameraEffectManager _manager)
    {
        m_Manager = _manager;
    }
    public virtual void OnRenderObject()
    {

    }
    public virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
    }
    public virtual void OnDestroy()
    {
    }
}
#region PostEffect
public class PostEffectBase: CameraEffectBase
{
    const string S_ParentPath = "Hidden/PostEffect/";
    public Material m_Material { get; private set; }
    public override bool m_IsPostEffect => true;
    protected override bool OnCreate()
    {
        Shader shader = Shader.Find(S_ParentPath + this.GetType().ToString());
        if (shader == null)
            Debug.LogError("Shader:" + S_ParentPath + this.GetType().ToString() + " Not Found");
        if (!shader.isSupported)
            Debug.LogError("Shader:" + S_ParentPath + this.GetType().ToString() + " Is Not Supported");

        m_Material = new Material(shader) { hideFlags = HideFlags.DontSave };
        return shader != null && shader.isSupported;
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, m_Material);
    }
    public override void OnDestroy()
    {
        GameObject.Destroy(m_Material);
    }
}
public class PE_GaussianBlur : PostEffectBase       //Gassuain Blur
{
    float F_BlurSpread;
    int I_Iterations;
    RenderTexture buffer0, buffer1;
    int rtW, rtH;
    public override void OnSetEffect(CameraEffectManager _manager)
    {
        base.OnSetEffect(_manager);
        SetEffect();
    }
    public void SetEffect(float _blurSpread=2f, int _iterations=5, int _downSample = 4)
    {
        F_BlurSpread = _blurSpread;
        I_Iterations = _iterations;
        _downSample = _downSample > 0 ? _downSample : 1;
        rtW = m_Manager.m_Camera.scaledPixelWidth >> _downSample;
        rtH = m_Manager.m_Camera.scaledPixelHeight >> _downSample;
        if (buffer0) RenderTexture.ReleaseTemporary(buffer0);
        if (buffer1) RenderTexture.ReleaseTemporary(buffer1);
        buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
        buffer0.filterMode = FilterMode.Bilinear;
        buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
        buffer0.filterMode = FilterMode.Bilinear;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        RenderTexture.ReleaseTemporary(buffer0);
        RenderTexture.ReleaseTemporary(buffer1);
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, buffer0);
        for (int i = 0; i < I_Iterations; i++)
        {
            m_Material.SetFloat("_BlurSpread", 1 + i * F_BlurSpread);
            Graphics.Blit(buffer0, buffer1, m_Material, 0);
            Graphics.Blit(buffer1, buffer0, m_Material, 1);
        }
        Graphics.Blit(buffer0, destination);
    }
}
public class PE_BloomSpecific : PostEffectBase 
{
    Camera m_RenderCamera;
    RenderTexture m_RenderTexture;
    Shader m_RenderShader;
    public PE_GaussianBlur m_GaussianBlur { get; private set; }
    public override void OnSetEffect(CameraEffectManager _manager)
    {
        base.OnSetEffect(_manager);
        m_GaussianBlur = new PE_GaussianBlur();
        m_GaussianBlur.OnSetEffect(_manager);
        m_RenderShader = Shader.Find("Hidden/PostEffect/PE_BloomSpecific_Render");
        if (m_RenderShader == null)
            Debug.LogError("Null Shader Found!");
        GameObject temp = new GameObject("Render Camera");
        temp.transform.SetParent(m_Manager.m_Camera.transform);
        temp.transform.localPosition = Vector3.zero;
        temp.transform.localRotation = Quaternion.identity;
        m_RenderCamera = temp.AddComponent<Camera>();
        m_RenderCamera.clearFlags = CameraClearFlags.SolidColor;
        m_RenderCamera.backgroundColor = Color.black;
        m_RenderCamera.orthographic = m_Manager.m_Camera.orthographic;
        m_RenderCamera.orthographicSize = m_Manager.m_Camera.orthographicSize;
        m_RenderCamera.nearClipPlane = m_Manager.m_Camera.nearClipPlane;
        m_RenderCamera.farClipPlane = m_Manager.m_Camera.farClipPlane;
        m_RenderCamera.fieldOfView = m_Manager.m_Camera.fieldOfView;
        m_RenderCamera.enabled = false;
        m_RenderTexture = RenderTexture.GetTemporary(m_Manager.m_Camera.scaledPixelWidth, m_Manager.m_Camera.scaledPixelHeight, 1);
        m_RenderCamera.targetTexture = m_RenderTexture;
    }
    public override void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        m_RenderCamera.RenderWithShader(m_RenderShader, "RenderType");
        m_GaussianBlur.OnRenderImage(m_RenderTexture, m_RenderTexture);     //Blur
        m_Material.SetTexture("_RenderTex", m_RenderTexture);
        Graphics.Blit(source, destination, m_Material, 1);        //Mix
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        m_GaussianBlur.OnDestroy();
        RenderTexture.ReleaseTemporary(m_RenderTexture);
    }
}
#endregion