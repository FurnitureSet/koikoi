using UnityEngine;

public class CardRenderer : MonoBehaviour {
    
    [SerializeField] private MeshRenderer content;
    [SerializeField] private MeshRenderer outline;
    [SerializeField] private ParticleSystemRenderer particles;

    private MaterialPropertyBlock contentMaterialProperties;
    private MaterialPropertyBlock outlineMaterialProperties;
    private MaterialPropertyBlock particleMaterialProperties;
    private ParticleSystem particleSystem;

    void Start() {
        contentMaterialProperties = new MaterialPropertyBlock();
        outlineMaterialProperties = new MaterialPropertyBlock();
        particleMaterialProperties = new MaterialPropertyBlock();
    }

    public void SetOutlineEnabled(bool enabled) {
        outline.enabled = enabled;
        if (enabled) particleSystem.Play();
        else particleSystem.Pause();
    }

    public void SetOutlineColor(Color color) {
        outlineMaterialProperties.SetColor("_EmissionColor", color);
        particleMaterialProperties.SetColor("_EmissionColor", color);
        
        outline.SetPropertyBlock(outlineMaterialProperties);
        particles.SetPropertyBlock(particleMaterialProperties);
    }
}
