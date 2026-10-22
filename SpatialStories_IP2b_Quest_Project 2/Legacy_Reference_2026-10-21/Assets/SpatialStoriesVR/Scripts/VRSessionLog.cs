using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace SpatialStories.Quest
{
    public sealed class VRSessionLog : MonoBehaviour
    {
        public string SessionId { get; private set; }
        public int Prompts { get; private set; }
        public bool Active { get; private set; }
        public string LastError { get; private set; }
        float start;
        string directory;
        void Awake() { directory = Path.Combine(Application.persistentDataPath, "SpatialStories_IP2a"); }

        public void Begin()
        {
            if (Active) End("incomplete", "restarted");
            SessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
            start = Time.realtimeSinceStartup;
            Prompts = 0; Active = true; LastError = null;
            Event("begin", "", "");
            Debug.Log("[Spatial Stories VR] Session " + SessionId + " | logs: " + directory);
        }
        public void Prompt(string stage)
        {
            if (!Active) return;
            Prompts++; Event("facilitator_prompt", stage, "record exact wording on observation sheet");
        }
        public void Event(string action, string stage, string detail)
        {
            if (!Active) return;
            Append("events.csv", "session_id,elapsed_seconds,event,stage,detail",
                SessionId, Elapsed(), action, stage, detail);
        }
        public void End(string outcome, string detail)
        {
            if (!Active) return;
            Event("session_end", outcome, detail);
            Append("sessions.csv", "session_id,duration_seconds,outcome,prompts,detail",
                SessionId, Elapsed(), outcome, Prompts.ToString(CultureInfo.InvariantCulture), detail);
            Active = false;
        }
        string Elapsed() => (Time.realtimeSinceStartup - start).ToString("F3", CultureInfo.InvariantCulture);
        public static string CsvCell(string value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        void Append(string filename, string header, params string[] values)
        {
            try
            {
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, filename);
                bool empty = !File.Exists(path) || new FileInfo(path).Length == 0;
                using (var writer = new StreamWriter(path, true))
                {
                    if (empty) writer.WriteLine(header);
                    writer.WriteLine(string.Join(",", Array.ConvertAll(values, CsvCell)));
                }
            }
            catch (Exception e)
            {
                LastError = "Log could not be saved. Use the observation sheet.";
                Debug.LogWarning("[Spatial Stories VR] " + LastError + " " + e.Message);
            }
        }
        void OnApplicationPause(bool paused)
        {
            if (Active) Event(paused ? "application_paused" : "application_resumed", "", "timing includes pauses");
        }
        void OnApplicationQuit() { End("incomplete", "application_quit"); }
    }
}
