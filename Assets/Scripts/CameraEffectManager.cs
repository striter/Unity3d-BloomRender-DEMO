using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraEffectManager :MonoBehaviour
{
    #region Interact
    public T GetOrAddCameraEffect<T>() where T: CameraEffectBase, new()
    {
        T existingEffect = GetCameraEffect<T>();
        if (existingEffect != null)
            return existingEffect;

        T effectBase = new T();
        if(effectBase.m_Supported)
        {
            effectBase.OnSetEffect(this);
            m_PostEffects.Add(effectBase);
            ResetPostEffectParams();
            return effectBase;
        }
        return null;
    }

    public T GetCameraEffect<T>() where T : CameraEffectBase => m_PostEffects.Find(p => p.GetType() ==typeof(T)) as T;
    public void RemoveCameraEffect<T>() where T : CameraEffectBase, new()
    {
        T effect = GetCameraEffect<T>();
        if (effect == null)
            return;

        m_PostEffects.Remove(effect);
        ResetPostEffectParams();
    }
    public void RemoveAllPostEffect()
    {
        for (int i = 0; i < m_PostEffects.Count; i++)
        {
            m_PostEffects[i].OnDestroy();
        }
        m_PostEffects.Clear();
        ResetPostEffectParams();
    }

    public void SetCostyEffectEnable(bool mobileCostEnable,bool highCostEnable)
    {
        ResetPostEffectParams();
    }
    
    #endregion
    List<CameraEffectBase> m_PostEffects=new List<CameraEffectBase>();
    public Camera m_Camera { get; protected set; }
    public bool m_PostEffectEnabled { get; private set; } = false;
    RenderTexture tempTexture1, tempTexture2;
    protected void Awake()
    {
        m_Camera = GetComponent<Camera>();
        m_Camera.depthTextureMode = DepthTextureMode.None;
    }
    
    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if(!m_PostEffectEnabled)
        {
            Graphics.Blit(source, destination);
            return;
        }

        tempTexture1 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        Graphics.Blit(source, tempTexture1);
        for (int i = 0; i < m_PostEffects.Count; i++)
        {
            tempTexture2 = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
            m_PostEffects[i].OnRenderImage(tempTexture1,tempTexture2);
            Graphics.Blit(tempTexture2, tempTexture1);
            RenderTexture.ReleaseTemporary(tempTexture2);
        }
        Graphics.Blit(tempTexture1,destination);
        RenderTexture.ReleaseTemporary(tempTexture1);
    }
    private void OnDestroy()
    {
        RemoveAllPostEffect();
    }
    private void OnRenderObject()
    {
        for (int i = 0; i < m_PostEffects.Count; i++)
            m_PostEffects[i].OnRenderObject();
    }

    void ResetPostEffectParams()
    {
        m_PostEffectEnabled = false;
        m_Camera.depthTextureMode = DepthTextureMode.None;

        for (int i = 0; i < m_PostEffects.Count; i++)
        {
            CameraEffectBase effectBase = m_PostEffects[i];

            m_PostEffectEnabled |= effectBase.m_IsPostEffect;
        }
    }
    
}
