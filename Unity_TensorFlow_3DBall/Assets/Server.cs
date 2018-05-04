using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Server : MonoBehaviour
{
    private Socket SeverSocket = null;
    private Thread Socket_Thread = null;
    private bool Socket_Thread_Flag = false;

    private const string IP = "127.0.0.1";
    private const int FORT = 9051;

    void Awake()
    {
        Socket_Thread = new Thread(Dowrk);
        Socket_Thread_Flag = true;
        Socket_Thread.Start();
    }

    private void Dowrk()
    {
        SeverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(IP), FORT);
        SeverSocket.Bind(ipep);
        SeverSocket.Listen(10);

        Debug.Log("소켓 대기중....");
        Socket client = SeverSocket.Accept();//client에서 수신을 요청하면 접속합니다.
        Debug.Log("소켓 연결되었습니다.");

        IPEndPoint clientep = (IPEndPoint)client.RemoteEndPoint;
        NetworkStream recvStm = new NetworkStream(client);

        while (Socket_Thread_Flag)
        {
            byte[] receiveBuffer = new byte[1024 * 80];
            try
            {
                recvStm.Read(receiveBuffer, 0, receiveBuffer.Length);
                string Test = Encoding.Default.GetString(receiveBuffer);
                Debug.Log(Test);
            }

            catch (Exception e)
            {
                Socket_Thread_Flag = false;
                client.Close();
                SeverSocket.Close();
                continue;
            }
        }
    }

    void OnApplicationQuit()
    {
        try
        {
            Socket_Thread_Flag = false;
            Socket_Thread.Abort();
            SeverSocket.Close();
        }
        catch
        {
            Debug.Log("소켓과 쓰레드 종료때 오류가 발생");
        }
    }
}