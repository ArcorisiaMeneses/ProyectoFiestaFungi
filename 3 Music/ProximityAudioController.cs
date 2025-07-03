using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityAudioController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject musicaObject;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioReverbFilter reverbFilter;
    
    [Header("Configuración de Volumen")]
    [SerializeField] private float maxVolumeDistance = 2f;
    [SerializeField] private float minVolumeDistance = 15f;
    [SerializeField] private float maxVolume = 1f;
    [SerializeField] private float minVolume = 0f;
    
    [Header("Configuración de Reverb")]
    [SerializeField] private float minReverbDistance = 3f;
    [SerializeField] private float maxReverbDistance = 20f;
    [SerializeField] private float minReverbLevel = -10000f; // Sin reverb
    [SerializeField] private float maxReverbLevel = 0f;     // Máximo reverb
    
    [Header("Configuración Visual")]
    [SerializeField] private Color volumeGizmoColor = Color.green;
    [SerializeField] private Color reverbGizmoColor = Color.blue;
    [SerializeField] private Color zeroVolumeGizmoColor = Color.red;
    
    private float currentDistance;
    private Vector3 musicaPosition;
    
    void Start()
    {
        // Buscar el objeto "musica" si no está asignado
        if (musicaObject == null)
        {
            musicaObject = GameObject.Find("musica");
            if (musicaObject == null)
            {
                Debug.LogError("No se encontró el GameObject 'musica'. Asegúrate de que existe en la escena.");
                return;
            }
        }
        
        // Obtener componentes si no están asignados
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (reverbFilter == null)
        {
            reverbFilter = GetComponent<AudioReverbFilter>();
            if (reverbFilter == null)
            {
                reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
            }
        }
        
        // Configurar reverb inicial
        reverbFilter.reverbPreset = AudioReverbPreset.User;
    }
    
    void Update()
    {
        if (musicaObject == null) return;
        
        // Calcular distancia
        musicaPosition = musicaObject.transform.position;
        currentDistance = Vector3.Distance(transform.position, musicaPosition);
        
        // Actualizar volumen y reverb
        UpdateVolume();
        UpdateReverb();
    }
    
    void UpdateVolume()
    {
        float volumeRatio;
        
        if (currentDistance <= maxVolumeDistance)
        {
            // Volumen máximo cuando está muy cerca
            volumeRatio = 1f;
        }
        else if (currentDistance >= minVolumeDistance)
        {
            // Volumen mínimo cuando está lejos
            volumeRatio = 0f;
        }
        else
        {
            // Interpolación lineal entre las distancias
            volumeRatio = 1f - ((currentDistance - maxVolumeDistance) / (minVolumeDistance - maxVolumeDistance));
        }
        
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, volumeRatio);
    }
    
    void UpdateReverb()
    {
        float reverbRatio;
        
        if (currentDistance <= minReverbDistance)
        {
            // Sin reverb cuando está cerca
            reverbRatio = 0f;
        }
        else if (currentDistance >= maxReverbDistance)
        {
            // Máximo reverb cuando está lejos
            reverbRatio = 1f;
        }
        else
        {
            // Interpolación lineal entre las distancias
            reverbRatio = (currentDistance - minReverbDistance) / (maxReverbDistance - minReverbDistance);
        }
        
        reverbFilter.reverbLevel = Mathf.Lerp(minReverbLevel, maxReverbLevel, reverbRatio);
        reverbFilter.room = Mathf.Lerp(-10000f, -1000f, reverbRatio); // CORREGIDO: era roomLevel
    }
    
    void OnDrawGizmos()
    {
        if (musicaObject == null) return;
        
        Vector3 musicaPos = musicaObject.transform.position;
        
        // Gizmo para distancia de volumen máximo (verde)
        Gizmos.color = volumeGizmoColor;
        Gizmos.DrawWireSphere(musicaPos, maxVolumeDistance);
        
        // Gizmo para distancia de volumen 0 (rojo)
        Gizmos.color = zeroVolumeGizmoColor;
        Gizmos.DrawWireSphere(musicaPos, minVolumeDistance);
        
        // Gizmo para distancia mínima de reverb (azul claro)
        Gizmos.color = reverbGizmoColor;
        Gizmos.DrawWireSphere(musicaPos, minReverbDistance);
        
        // Gizmo para distancia máxima de reverb (azul oscuro)
        Gizmos.color = new Color(reverbGizmoColor.r, reverbGizmoColor.g, reverbGizmoColor.b, 0.5f);
        Gizmos.DrawWireSphere(musicaPos, maxReverbDistance);
        
        // Línea de conexión entre player y objeto música
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, musicaPos);
            
            // Mostrar distancia actual
            Vector3 midPoint = (transform.position + musicaPos) / 2f;
            Gizmos.DrawWireCube(midPoint, Vector3.one * 0.2f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Información adicional cuando el objeto está seleccionado
        if (musicaObject != null && Application.isPlaying)
        {
            Vector3 musicaPos = musicaObject.transform.position;
            
            // Mostrar información de debug (solo funciona en el editor)
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Distancia: {currentDistance:F2}m\nVolumen: {(audioSource != null ? audioSource.volume : 0):F2}\nReverb: {(reverbFilter != null ? reverbFilter.reverbLevel : 0):F0}");
            #endif
        }
    }
}