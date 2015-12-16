using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using UnityEngine.UI;

public class KinectListener : MonoBehaviour {
	private const float timeout = 1f;
	private const int queueMaxLenght = 10;

	public GUIText statusText;

	public static Vector3 leftArm { get; private set; } 
	public static Vector3 rightArm { get; private set; }
	public static bool isPainting { get; private set; }

	private Thread thread;
	private bool listening = true;
	private float lastContactTime;
	private bool userDetected = false;
	private TcpClient clientSocket = null;
	TcpListener serverSocket = null;
	private float time = 0;
	private ConcurrentQueue<Vector3[]> vecQueue = new ConcurrentQueue<Vector3[]>();
	private int frameCount = 0;
	private int fps = 0;
	private int ups = 0;
	private int upsCount = 0;
	private float lastFpsUpdate = 0;
	private int mCount = 0;
	private int mps = 0;

	// Use this for initialization
	void Start () {
		thread = new Thread(new ThreadStart(listen));
		thread.Start();
		leftArm = Vector3.zero;
		rightArm = Vector3.zero;
	}

	void Update() {
		time = Time.time;

		// get fps & mps

		if (time > lastFpsUpdate + 1)
		{
			lastFpsUpdate = time;
			fps = frameCount;
			frameCount = 0;
			ups = upsCount;
			upsCount = 0;
			mps = mCount;
			mCount = 0;
		}

		// update status
		if (clientSocket != null && clientSocket.Connected) {
			statusText.text = userDetected ? "fps: "+fps+" ups: "+ups+" mps: "+mps+" ql: "+vecQueue.count : "Waiting for user: please be a tree on the cross";
		} else {
			statusText.text = "Waiting for Max's computer";
		}

		// update vectors
		Vector3[] vecs;
		if (vecQueue.tryDequeue(out vecs)) {
			leftArm = vecs[0];
			rightArm = vecs[1];
			upsCount++;
		}
		frameCount++;
	}

	void OnApplicationQuit() {
		listening = false;
		if (clientSocket != null)
			thread.Interrupt();
	}
	
	void listen() {
		try {
			serverSocket = new TcpListener(IPAddress.Any, 8888);
			serverSocket.Start();
			Debug.Log(" >> Server Started");

			while (listening) {
				// get a client
				Debug.Log(" >> Waiting for new connection");
				while (clientSocket == null) {
					if (serverSocket.Pending()) {
						lastContactTime = time;
						clientSocket = serverSocket.AcceptTcpClient();
						Debug.Log(" >> New connection :D");
					} else {
						Thread.Sleep(100);
					}
				}

				// deal with the client
				while (listening && clientSocket != null) {
					// check that is alive
					while (clientSocket != null && clientSocket.Available == 0) {
						if (time > lastContactTime + timeout) {
							Debug.Log(" >> Disconnected by timeout");
							clientSocket.Close();
							clientSocket = null;
						}
						Thread.Sleep(100);
					}

					// speak with him
					if (clientSocket != null) {
						lastContactTime = time;
						NetworkStream networkStream = clientSocket.GetStream();
						int bufferSize = (int)clientSocket.ReceiveBufferSize;
						byte[] bytesFrom = new byte[bufferSize];
						networkStream.Read(bytesFrom, 0, bufferSize);
						treatData( System.Text.Encoding.ASCII.GetString(bytesFrom) );
						mCount++;
						/*string serverResponse = "Last Message from client" + dataFromClient;
						Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
						networkStream.Write(sendBytes, 0, sendBytes.Length);
						networkStream.Flush();
						Console.WriteLine(" >> " + serverResponse);*/
					}
				}
			}
		} catch (ThreadInterruptedException) {
			// expected :)
			//Debug.LogException(ex);
		} finally {
			if (clientSocket != null)
				clientSocket.Close();
			if (serverSocket != null)
				serverSocket.Stop();
			Debug.Log(" >> Server terminated :)");
		}
	}

	private const int nbMsgWord = 8;
	private void treatData(string data) {
		for (int msgEndIdx = data.IndexOf("$"); msgEndIdx > -1; msgEndIdx = data.IndexOf("$")) {
			string msg = data.Substring(0, msgEndIdx);
			data = data.Substring(msgEndIdx+1);

			string[] floatsStr = msg.Split(' ');
			float[] floats = new float[nbMsgWord];
			if (floatsStr.Length == nbMsgWord) {
				for (int i=0; i<nbMsgWord; i++) {
					floats[i] = float.Parse(floatsStr[i]);
				}
				if (vecQueue.count >= queueMaxLenght)
					vecQueue.dequeue();
				vecQueue.enqueue(new Vector3[] {
					new Vector3(floats[0], floats[1], floats[2]),
					new Vector3(floats[3], floats[4], floats[5]) });
				if (userDetected != floats[6] > 0) { // if user detected changed
					userDetected = !userDetected;
				}
				isPainting = floats[7] > 0;
			}
		}
	}
}