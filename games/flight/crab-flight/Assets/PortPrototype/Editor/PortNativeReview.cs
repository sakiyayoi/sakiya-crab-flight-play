using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Port prototype native review: fixed-input replay with the port camera ON and OFF,
// per-beat captures, role receipts, and result/retry evidence. Verification only.
public static class PortNativeReview {
  [Serializable] public class Receipt { public bool cameraOn; public float distance; public int breaks, ticks; public string phase; public bool retryReady; }
  [Serializable] public class ReceiptsWrap { public Receipt[] runs; }
  [Serializable] public class ActionReceipt { public string kind; public Vector3 position; public float time; }
  [Serializable] public class Roles { public PortRoleReceipt[] roles; }

  public static void Run() {
    var dir = Environment.GetEnvironmentVariable("PORT_REVIEW_OUTPUT");
    var inputPath = Environment.GetEnvironmentVariable("PORT_REVIEW_INPUT");
    if (string.IsNullOrEmpty(dir) || Directory.Exists(dir)) throw new Exception("Use a fresh PORT_REVIEW_OUTPUT directory");
    if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath)) throw new Exception("PORT_REVIEW_INPUT must point at accepted-input.json");
    Directory.CreateDirectory(dir);
    var receipts = new List<Receipt>();
    BlenderFamilyOverride.Enabled = true;
    BlenderPilotImport.Prepare();
    EditorSceneManager.OpenScene("Assets/FlightField.unity", OpenSceneMode.Single);
    var game = UnityEngine.Object.FindFirstObjectByType<CrabFlightGame>();
    if (!game) throw new Exception("Saved game scene missing");
    UnityEngine.Random.InitState(42043);
    game.Initialize();
    game.enabled = false;
    var input = File.ReadAllText(inputPath);
    RunOne(game, dir + "/camon", true, input, receipts);
    // camoff skipped for the opening review
    File.WriteAllText(dir + "/receipts.json", JsonUtility.ToJson(new ReceiptsWrap { runs = receipts.ToArray() }, true));
    if (game) game.ShutdownReviewPhysics();
  }

  static void RunOne(CrabFlightGame game, string dir, bool cameraOn, string input, List<Receipt> receipts) {
    Directory.CreateDirectory(dir);
    var receipt = new Receipt { cameraOn = cameraOn };
    game.usePortCamera = cameraOn;
    var camera = game.ReviewCamera;
    game.RefreshReviewCamera();
    if (cameraOn) { Capture(camera, dir + "/aim.png", 1280, 800); CapturePortrait(game, dir + "/aim-portrait.png"); }
    game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));
    UnityEngine.Random.InitState(42043);
    game.ReviewReplayAcceptedCalmR3(input);
    var marks = new[] { 8f, 20f, 40f, 80f, 120f };
    int mark = 0;
    var route = new List<string> { "tick,time,x,y,speed,breaks" };
    for (int k = 0; k < 18000 && !game.Finished; k++) {
      game.Tick(1f / 60); receipt.ticks++;
      var s = game.State();
      if (k % 30 == 0) route.Add(k + "," + s.time + "," + s.worldX + "," + s.height + "," + s.speed + "," + s.destructionEvent);
      if (cameraOn && mark < marks.Length && s.worldX >= marks[mark]) {
        Capture(camera, dir + "/flight-" + marks[mark] + ".png", 1280, 800);
        if (marks[mark] == 20f || marks[mark] == 80f) CapturePortrait(game, dir + "/flight-" + marks[mark] + "-portrait.png");
        File.WriteAllText(dir + "/flight-" + marks[mark] + ".json", JsonUtility.ToJson(s, true));
        mark++;
      }
    }
    var sim = game.ReviewSimulation;
    var state = game.State();
    receipt.distance = state.distance;
    receipt.phase = state.phase;
    receipt.breaks = sim.BreakEvents.Count;
    File.WriteAllLines(dir + "/route.csv", route);
    File.WriteAllLines(dir + "/break-events.jsonl", sim.BreakEvents.Select(e => JsonUtility.ToJson(e)));
    WriteActionEvents(dir, sim.ObjectHits);
    File.WriteAllText(dir + "/roles.json", JsonUtility.ToJson(new Roles { roles = sim.PortRoleReceipts.ToArray() }, true));
    if (cameraOn) Capture(camera, dir + "/result.png", 1280, 800);
    game.Prepare("");
    receipt.retryReady = game.State().phase == "ready";
    if (cameraOn) Capture(camera, dir + "/retry.png", 1280, 800);
    receipts.Add(receipt);
  }

  // Read the authoritative per-throw consumed-hit list before Prepare clears it.
  // Empty/legacy throws still create an empty file; no ring-buffer loss or re-emission.
  public static void WriteActionEvents(string dir, IEnumerable<RigidCrabFlight.ObjectHit> hits) {
    File.WriteAllLines(Path.Combine(dir, "action-events.jsonl"), hits.Select(e => JsonUtility.ToJson(
      new ActionReceipt { kind = e.kind.ToString().ToLowerInvariant(), position = e.position, time = e.time })));
  }

  public static void Capture(Camera camera, string path, int w, int h) {
    var prev = camera.targetTexture; var active = RenderTexture.active; var aspect = camera.aspect;
    var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32); var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
    try { camera.aspect = (float)w / h; camera.targetTexture = rt; camera.Render(); RenderTexture.active = rt; tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply(); File.WriteAllBytes(path, tex.EncodeToPNG()); }
    finally { camera.targetTexture = prev; camera.aspect = aspect; RenderTexture.active = active; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt); }
  }

  static void CapturePortrait(CrabFlightGame game, string path) {
    var camera = game.ReviewCamera; var position = camera.transform.position; var rotation = camera.transform.rotation; var size = camera.orthographicSize; var aspect = camera.aspect;
    var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
    var fields = new[] { "cameraOffsetR29", "cameraFollowingR29", "steadyFlightSpeedR3" }.Select(n => typeof(CrabFlightGame).GetField(n, flags)).ToArray();
    var values = fields.Select(f => f.GetValue(game)).ToArray();
    try { camera.aspect = 390f / 844; game.RefreshReviewCamera(); Capture(camera, path, 390, 844); }
    finally { for (int i = 0; i < fields.Length; i++) fields[i].SetValue(game, values[i]); camera.transform.SetPositionAndRotation(position, rotation); camera.orthographicSize = size; camera.aspect = aspect; }
  }
}
