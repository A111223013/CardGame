using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardGameSever
{
    public partial class Form1 : Form
    {
        TcpListener Server;             //伺服端網路監聽器(相當於電話總機)
        Socket Client;                  //給客戶用的連線物件(相當於電話分機)
        Thread Th_Svr;                  //伺服器監聽用執行緒(電話總機開放中)
        Thread Th_Clt;                  //客戶用的通話執行緒(電話分機連線中)
        Hashtable HT = new Hashtable(); //客戶名稱與通訊物件的集合(雜湊表)(key:Name, Socket)
        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            Th_Svr = new Thread(ServerSub);     //宣告監聽執行緒(副程式ServerSub)
            Th_Svr.IsBackground = true;         //設定為背景執行緒
            Th_Svr.Start();                     //啟動監聽執行緒
            button1.Enabled = false;            //讓按鍵無法使用(不能重複啟動伺服器) 
        }
        private void ServerSub()
        {
            //Server IP 和 Port
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));
            Server = new TcpListener(EP);       //建立伺服端監聽器(總機)
            Server.Start(100);                  //啟動監聽設定允許最多連線數100人
            while (true)                        //無限迴圈監聽連線要求
            {
                Client = Server.AcceptSocket(); //建立此客戶的連線物件Client
                Th_Clt = new Thread(Listen);    //建立監聽這個客戶連線的獨立執行緒
                Th_Clt.IsBackground = true;     //設定為背景執行緒
                Th_Clt.Start();                 //開始執行緒的運作
            }
        }
        private void Listen()
        {
            Socket Sck = Client; //複製Client通訊物件到個別客戶專用物件Sck
            Thread Th = Th_Clt;  //複製執行緒Th_Clt到區域變數Th
            while (true)         //持續監聽客戶傳來的訊息
            {
                try                //用 Sck 來接收此客戶訊息，inLen 是接收訊息的 byte 數目
                {
                    byte[] B = new byte[1023];   //建立接收資料用的陣列，長度須大於可能的訊息
                    int inLen = Sck.Receive(B);  //接收網路資訊(byte陣
                    string Msg = Encoding.Default.GetString(B, 0, inLen); //翻譯實際訊息(長度inLen)
                    listBox2.Items.Add("(接收)" + Msg);
                    string Cmd = Msg.Substring(0, 1);                     //取出命令碼 (第一個字)
                    string Str = Msg.Substring(1);                        //取出命令碼之後的訊息
                    switch (Cmd)                                          //依據命令碼執行功能
                    {
                        case "0":                    //有新使用者上線：新增使用者到名單中
                            if(listBox1.Items.IndexOf(Str) == -1)
                            {
                                HT.Add(Str, Sck);        //連線加入雜湊表，Key:使用者，Value:連線物件(Socket)
                                listBox1.Items.Add(Str); //加入上線者名單
                                SendAll(OnlineList());   //將目前上線人名單回傳剛剛登入的人(包含他自己)
                            }
                            else
                            {
                                string reply = "D" + Str + "使用者名稱重複";
                                B = Encoding.Default.GetBytes(reply);
                                listBox2.Items.Add("(傳送)" + reply);
                                Sck.Send(B, 0 , B.Length, SocketFlags.None);
                                Th.Abort();
                            }
                            
                            break;
                        case "9":
                            HT.Remove(Str);             //移除使用者名稱為Name的連線物件
                            listBox1.Items.Remove(Str); //自上線者名單移除Name
                            SendAll(OnlineList());      //將目前上線人名單回傳剛剛登入的人(不包含他自己) 
                            Th.Abort();                 //結束此客戶的監聽執行緒
                            break;
                        case "1":                       //使用者傳送訊息給所有人
                            SendAll(Msg);               //廣播訊息
                            break;
                        default:                        //使用者傳送私密訊息
                            string[] C = Str.Split('|');//切開訊息與收件者
                            SendTo(Cmd + C[0], C[1]);   //C[0]是訊息，C[1]是收件者
                            break;
                    }
                }
                catch (Exception)
                {
                    //有錯誤時忽略，通常是客戶端無預警強制關閉程式，測試階段常發生
                }
            }
        }
        private void SendTo(string Str, string User)
        {
            listBox2.Items.Add("(傳送)" + Str + ":" + User);
            byte[] B = Encoding.Default.GetBytes(Str);  //訊息轉譯為byte陣列
            Socket Sck = (Socket)HT[User];              //取出發送對象User的通訊物件
            Sck.Send(B, 0, B.Length, SocketFlags.None); //發送訊息
        }
        //傳送訊息給所有的線上客戶
        private void SendAll(string Str)
        {
            listBox2.Items.Add("(傳送)" + Str);
            byte[] B = Encoding.Default.GetBytes(Str);   //訊息轉譯為Byte陣列
            foreach (Socket s in HT.Values)              //HT雜湊表內所有的Socket
                s.Send(B, 0, B.Length, SocketFlags.None);//傳送資料
        }
        private string OnlineList()
        {
            string L = "L";             //代表線上名單的命令碼(字頭)
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                L += listBox1.Items[i]; //逐一將成員名單加入L字串
                //不是最後一個成員要加上","區隔
                if (i < listBox1.Items.Count - 1)
                {
                    L += ",";
                }
            }
            return L;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread(); //關閉所有執行緒 
        }
    }
}
