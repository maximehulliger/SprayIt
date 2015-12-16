using UnityEngine;
using System.Collections;
using System;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.UI;

/// sends kinect data to the main program
public class KinectSender : MonoBehaviour {
	private const int port = 8888;

	public GUIText debugText;
	public RawImage kinectImg;
	public bool yesDetect = false;
	[Space(10)]
	public string ip1 = "130.237.228.172";
	public string ip2 = "127.0.0.1";
	public bool useFirstIp = false;

	private System.Net.Sockets.TcpClient clientSocket = null;
	private NetworkStream serverStream;
	private Thread thread;
	private bool userDetected = false;
	private int frameCount = 0;
	private int fps = 0;
	private float lastStatUpdate = 0;

	// Use this for initialization
	void Start () {
		//debugText.text = "Disconnected";
		//thread = new Thread(new ThreadStart(send));
		//thread.Start();
	}

	void OnApplicationQuit() {
		if (clientSocket != null) {
			clientSocket.Close();
			clientSocket = null;
		}
		//sending = false;
		//thread.Interrupt();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space))
			yesDetect = !yesDetect;
		if (Input.GetKeyDown(KeyCode.I))
			useFirstIp = !useFirstIp;

		send();

		// get fps/mps
		frameCount++;
		if (Time.time > lastStatUpdate + 1) {
			lastStatUpdate = Time.time;
			fps = frameCount;
			frameCount = 0;
		}

		if (KinectManager.Instance != null) {
			//kinectImg.texture = KinectManager.Instance.GetUsersLblTex();
			userDetected = KinectManager.Instance.IsUserDetected();
		}

		if (clientSocket != null && clientSocket.Connected) {
			debugText.text = "Connected :D ";
		} else {
			debugText.text = "Disconnected :( ";
		}

		debugText.text += "\nfps/mps: "+fps;
	}
	
	private void send () {
		if (clientSocket == null || !clientSocket.Connected) {
			try {
				clientSocket = new System.Net.Sockets.TcpClient();
				clientSocket.Connect(useFirstIp ? ip1 : ip2, port);
			} catch (SocketException) {
				clientSocket = null;
				//Thread.Sleep(100);
			}
		} else {
			try {
				//send Kinect info
				serverStream = clientSocket.GetStream();

				bool isPainting = Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.PageUp);
				byte[] outStream = System.Text.Encoding.ASCII.GetBytes(encode(KinectInput.leftArm, KinectInput.rightArm, 
				                                                              userDetected, isPainting));
				serverStream.Write(outStream, 0, outStream.Length);
				serverStream.Flush();
				/*byte[] inStream = new byte[10025];
				serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
				string returndata = System.Text.Encoding.ASCII.GetString(inStream);*/
			} catch (SocketException) {
				//ex.ToString();
				clientSocket = null;
			}
		}
	}

	private string encode(Vector3 v1, Vector3 v2, bool userD, bool isPainting) {
		return ""+v1.x+" "+v1.y+" "+v1.z+" "+v2.x+" "+v2.y+" "+v2.z+" "+(userD||yesDetect ? 1 : 0)+" "+(isPainting ? 1 : 0)+"$";
	}
}
