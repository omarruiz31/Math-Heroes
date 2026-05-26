using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de guardado multi-perfil.
/// Cada niño tiene su propia partida identificada por nombre.
///
/// Estructura en PlayerPrefs:
///   "MathHeroes_ProfileList"      → JSON con lista de nombres
///   "MathHeroes_Profile_Omar"     → JSON con PlayerData de Omar
///   "MathHeroes_Profile_Luis"     → JSON con PlayerData de Luis
/// </summary>
public static class SaveSystem
{
    private const string ProfileListKey = "MathHeroes_ProfileList";
    private const string ProfilePrefix = "MathHeroes_Profile_";

    // ─── Perfiles ───

    /// <summary>
    /// Obtiene la lista de todos los nombres de perfil guardados.
    /// </summary>
    public static List<string> GetAllProfileNames()
    {
        if (!PlayerPrefs.HasKey(ProfileListKey))
            return new List<string>();

        string json = PlayerPrefs.GetString(ProfileListKey);
        ProfileList list = JsonUtility.FromJson<ProfileList>(json);
        return list?.names ?? new List<string>();
    }

    /// <summary>
    /// Verifica si un perfil con ese nombre ya existe.
    /// </summary>
    public static bool ProfileExists(string profileName)
    {
        return PlayerPrefs.HasKey(ProfilePrefix + profileName);
    }

    // ─── Guardar / Cargar ───

    /// <summary>
    /// Guarda los datos de un perfil específico.
    /// </summary>
    public static void Save(PlayerData data)
    {
        if (string.IsNullOrEmpty(data.playerName))
        {
            Debug.LogWarning("[SaveSystem] No se puede guardar sin nombre de jugador.");
            return;
        }

        // Guardar datos del perfil
        string key = ProfilePrefix + data.playerName;
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(key, json);

        // Agregar a la lista de perfiles si no existe
        List<string> profiles = GetAllProfileNames();
        if (!profiles.Contains(data.playerName))
        {
            profiles.Add(data.playerName);
            SaveProfileList(profiles);
        }

        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Perfil guardado: {data.playerName} (Nivel {data.level})");
    }

    /// <summary>
    /// Carga los datos de un perfil por nombre. Retorna null si no existe.
    /// </summary>
    public static PlayerData Load(string profileName)
    {
        string key = ProfilePrefix + profileName;

        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log($"[SaveSystem] Perfil '{profileName}' no encontrado.");
            return null;
        }

        string json = PlayerPrefs.GetString(key);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);
        Debug.Log($"[SaveSystem] Perfil cargado: {data.playerName} (Nivel {data.level})");
        return data;
    }

    /// <summary>
    /// Elimina un perfil específico.
    /// </summary>
    public static void DeleteProfile(string profileName)
    {
        string key = ProfilePrefix + profileName;
        PlayerPrefs.DeleteKey(key);

        List<string> profiles = GetAllProfileNames();
        profiles.Remove(profileName);
        SaveProfileList(profiles);

        PlayerPrefs.Save();
        Debug.Log($"[SaveSystem] Perfil eliminado: {profileName}");
    }

    /// <summary>
    /// Verifica si existe al menos un perfil guardado.
    /// </summary>
    public static bool HasSave()
    {
        return GetAllProfileNames().Count > 0;
    }

    // ─── Helpers privados ───

    private static void SaveProfileList(List<string> profiles)
    {
        ProfileList list = new ProfileList { names = profiles };
        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(ProfileListKey, json);
    }

    /// <summary>
    /// Clase auxiliar para serializar la lista de perfiles con JsonUtility.
    /// (JsonUtility no puede serializar List directamente como raíz)
    /// </summary>
    [System.Serializable]
    private class ProfileList
    {
        public List<string> names = new List<string>();
    }
}
