using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;


public class Tongsin : MonoBehaviour
{
	ClientWebSocket ws = new ClientWebSocket();
	async void Start()
	{
		test();
	}
	async void test()
	{
		var uri = new Uri("wss://driven-goldfish-needlessly.ngrok-free.app/ws/pose");
		try
		{
			await ws.ConnectAsync(uri, CancellationToken.None);
			Debug.Log(" Spring ������ WebSocket �����");
		}
		catch (Exception e)
		{
			Debug.Log(e);
		}

		Dictionary<string, string> j = new Dictionary<string, string>
		{ { "type","register"}, { "userId", "user123" }, { "role","unity" } };
		// ��� �޽��� ����
		string json = JsonConvert.SerializeObject(j);
		await ws.SendAsync(
				Encoding.UTF8.GetBytes(json),
				WebSocketMessageType.Text,
				true,
				CancellationToken.None
			);

		// ���� ����
		var buffer = new byte[8192];
		try
		{
			while (ws.State == WebSocketState.Open)
			{
				var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
				PosePlayer.pp.UpdatePose(msg);
			}
		}
		catch (Exception e) { }
	}

    private void OnApplicationQuit()
    {
        ws.Dispose();
    }
}
