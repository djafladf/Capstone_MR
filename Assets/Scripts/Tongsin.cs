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
    [SerializeField] List<PosePlayer> pp;
    ClientWebSocket ws = new ClientWebSocket();
    private CancellationTokenSource cancellation;
    async void Start()
    {
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
            print(ws.State);
            if (token.IsCancellationRequested) return;
            try
            {
                await ws.ConnectAsync(uri, CancellationToken.None);
            }
            catch (Exception e)
            {
                if (ws.State != WebSocketState.Open)
                {
                    print("Connection Fail! Retry...");
                    ws.Dispose();
                    ws = new ClientWebSocket();
                }
            }
            await Task.Delay(1000);

        }
        print("Success!");

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
            foreach(var jk in pp) jk.UpdatePose(msg);
        }
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
