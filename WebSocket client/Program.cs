using System.Net.WebSockets;
using System.Text;

// instead of http/htpps must be full url with ws/wss, relatively
Console.WriteLine("Please specify the URL of SignalR Hub with WS/WSS protocol");

var url = Console.ReadLine();

try
{
	var ws = new ClientWebSocket();

	await ws.ConnectAsync(new Uri(url), CancellationToken.None);

	// handshaking refers to the initial communication between a client and a server to establish a WebSocket connection
	var handshake = new List<byte>(Encoding.UTF8.GetBytes(@"{""protocol"":""json"", ""version"":1}"))
				{
					0x1e
				};

	// here we init our connection
	await ws.SendAsync(new ArraySegment<byte>(handshake.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);

	Console.WriteLine("WebSockets connection established");
	await ReceiveAsync(ws);
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
	Console.WriteLine("Press any key to exit...");
	Console.ReadKey();
	return;
}

static async Task ReceiveAsync(ClientWebSocket ws)
{
	var buffer = new byte[4096];

	try
	{
		while (true)
		{
			var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
			if (result.MessageType == WebSocketMessageType.Close) // if we close our connection
			{
				await ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
				break;
			}
			else // if we are not closing our connection, we continue decoding bytes, we get from client
			{
				Console.WriteLine(Encoding.Default.GetString(Decode(buffer)));
				buffer = new byte[4096];
			}
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine(ex.Message);
		Console.WriteLine("Press any key to exit...");
		Console.ReadKey();
		return;
	}
}

// trims trailing zeros
static byte[] Decode(byte[] packet)
{
	var i = packet.Length - 1;
	while (i >= 0 && packet[i] == 0)
	{
		--i;
	}

	var temp = new byte[i + 1];
	Array.Copy(packet, temp, i + 1);
	return temp;
}