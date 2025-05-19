using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class Tongsin : MonoBehaviour
{
    public static Tongsin inst = null;
    [SerializeField] List<PosePlayer> pp;
    ClientWebSocket ws = new ClientWebSocket();
    private CancellationTokenSource cancellation;

    public Dictionary<string, List<landmarks>> poseData = new Dictionary<string, List<landmarks>>();

    public Dictionary<string, float> GapOfLeg = new Dictionary<string, float>();
    public Dictionary<string, float> CurGap = new Dictionary<string, float>();

    async void Start()
    {
        if (inst == null) inst = this;
        cancellation = new CancellationTokenSource();
        ws = new ClientWebSocket();
        _ = ConnectWebSocket(cancellation.Token); // fire-and-forget
    }

    [SerializeField] string url_sub;
    async Task ConnectWebSocket(CancellationToken token)
    {
        var uri = new Uri($"wss://{url_sub}/ws/pose");
        while (ws.State != WebSocketState.Open)
        {
            if (token.IsCancellationRequested) return;
            try
            {
                await ws.ConnectAsync(uri, CancellationToken.None);
            }
            catch (Exception e)
            {
                if (ws.State != WebSocketState.Open)
                {
                    ws.Dispose();
                    ws = new ClientWebSocket();
                }
            }
            await Task.Delay(1000);

        }

        Dictionary<string, string> j = new Dictionary<string, string>
        { { "type","register"}, { "userId", "user123" }, { "role","unity" } };
        // 등록 메시지 전송
        string json = JsonConvert.SerializeObject(j);
        await ws.SendAsync(
                Encoding.UTF8.GetBytes(json),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

        // 수신 루프
        var buffer = new byte[8192];
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var pose = JsonConvert.DeserializeObject<PoseData_User>(msg);
            poseData[pose.deviceId] = pose.landmarks;
            var cnt = pose.landmarks;
            if (!GapOfLeg.ContainsKey(pose.deviceId))
            {
                GapOfLeg[pose.deviceId] = 0.5f * ((cnt[23].y - cnt[27].y) + (cnt[24].y - cnt[28].y));
            }

            foreach (var jk in pp) if(jk.DeviceId == pose.deviceId)
                {
                    jk.UpdatePose(); 
                }

            CurGap[pose.deviceId] = 0.5f * ((cnt[23].y - cnt[27].y) + (cnt[24].y - cnt[28].y));
        }
    }

    public void MakeGapOfLeg(string id)
    {
        
    }

    private async void OnApplicationQuit()
    {
        cancellation?.Cancel();

        if (ws != null && ws.State == WebSocketState.Open)
        {
            try
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogWarning("WebSocket close error: " + e.Message);
            }
            ws.Dispose();
        }
    }
}
