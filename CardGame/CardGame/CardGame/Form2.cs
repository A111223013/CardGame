using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;         //匯入網路通訊協定相關函數
using System.Net.Sockets; //匯入網路插座功能函數
using System.Threading;   //匯入多執行緒功能函數

namespace CardGame
{
    public partial class Form2 : Form
    {
        private TcpClient client;
        private List<string> onlinePlayers = new List<string>();

        public Form2()
        {
            InitializeComponent();
        }
        //公用變數
        Socket T;                                          //通訊物件
        Thread Th;                                         //網路監聽執行緒
        string User;    //使用者
        string my;
        Form1 form1 = new Form1();
        private string send_sever_message;
        private string MyIP()
        {
            string hn = Dns.GetHostName();
            IPAddress[] ip = Dns.GetHostEntry(hn).AddressList;
            foreach (IPAddress it in ip)
                if (it.AddressFamily == AddressFamily.InterNetwork)
                    return it.ToString();
            return "";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string message = $"W|{Txt_username.Text.Trim()}"; // 發送玩家名稱請求
            Send(message);
            StartGame(isPlayerTurn: true);
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show("謝謝遊玩!");
            if (T != null)
            { T.Close(); }
            Application.Exit();  // 關閉整個應用程式    
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            form1.send_server_Message = "伺服器ip" + textBox2.Text + "\n 伺服器port" + textBox1.Text + "\n玩家名稱" + Txt_username.Text;
            form1.OpponentName = Txt_username.Text;

            Control.CheckForIllegalCrossThreadCalls = false;
            User = Txt_username.Text;
            if (string.IsNullOrWhiteSpace(User))
            {
                MessageBox.Show("使用者名稱不可為空！");
                return;
            }
            string IP = textBox2.Text;
            int Port = int.Parse(textBox1.Text);
            form1.server_ip = IP;
            form1.server_port = Port;
            try
            {
                IPEndPoint EP = new IPEndPoint(IPAddress.Parse(IP), Port);
                T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                T.Connect(EP);
                Th = new Thread(Listen);
                Th.IsBackground = true;
                Th.Start();


                Txt_system_message.Text = "以連線至伺服器" + "\r\n;";
                Send("0" + User);
                btn_start.Enabled = false;
                btn_in_user.Enabled = true;
                button1.Enabled = false;


            }
            catch (Exception)
            {
                Txt_system_message.Text = "無法連線上伺服器" + "\r\n;";

            }
            btn_in_user.Enabled = true;
            btn_start.Enabled = false;

        }
        private void Send(string Str)
        {

            byte[] B = Encoding.Default.GetBytes(Str);
            T.Send(B, 0, B.Length, SocketFlags.None);
        }
        public void test_send(string cmd)
        {
            string[] F = cmd.Split(',');
            MessageBox.Show(F[0] + F[1]);
            Send("T" + cmd + "|" + listBox1.SelectedItem);
        }
        public void card_send(string cmd)
        {
            Send("M" + cmd + "|" + listBox1.SelectedItem);
        }
        public void CardSkill(string cmd)
        {
            Send("S" + cmd + "|" + listBox1.SelectedItem);
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            this.Text += " " + MyIP(); // 顯示本機 IP
            button1.Enabled = false;
        }

        private void btn_in_user_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                if (listBox1.SelectedItem.ToString() != User)
                {

                    Send("I" + User + "," + listBox1.Text + "|" + listBox1.SelectedItem);
                }
                else
                {
                    MessageBox.Show("不可以邀請自己");
                }
            }
            else
            {
                MessageBox.Show("沒有選取的邀請對象!");
            }
        }

        private void Listen()
        {
            EndPoint ServerEP = (EndPoint)T.RemoteEndPoint;
            byte[] B = new byte[1023];
            int inLen = 0;
            string MSG;
            string St;
            string Str;
            while (true) // 使用 connected 控制迴圈
            {
                try
                {

                    inLen = T.ReceiveFrom(B, ref ServerEP);
                }
                catch (Exception)
                {
                    T.Close();
                    listBox1.Items.Clear();
                    MessageBox.Show("伺服器斷線了");
                    btn_start.Enabled = true;
                    Th.Abort();

                }
                MSG = Encoding.Default.GetString(B, 0, inLen);
                St = MSG.Substring(0, 1);
                Str = MSG.Substring(1);
                switch (St)
                {
                    case "L":
                        listBox1.Items.Clear();
                        string[] M = Str.Split(',');
                        for (int i = 0; i < M.Length; i++) listBox1.Items.Add(M[i]);
                        break;
                    case "5":
                        DialogResult result = MessageBox.Show("是排遊玩卡牌對戰", "重玩訊息", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            listBox1.Text = Str;
                            listBox1.Enabled = false;
                            Send("P" + "Y" + "|" + listBox1.SelectedItem);

                        }
                        else
                        {

                            Send("P" + "N" + "|" + listBox1.SelectedItem);
                        }

                        break;
                    case "P":
                        if (Str == "Y")
                        {
                            MessageBox.Show(listBox1.SelectedItem.ToString() + "接受您的邀請，開始重玩遊戲ㄌ!");
                        }
                        else
                        {

                            MessageBox.Show("抱歉" + listBox1.SelectedItem.ToString() + "覺得你太爛!");
                        }


                        break;

                    case "D":
                        Txt_system_message.Text = Str;
                        MessageBox.Show("使用者名稱重複了");
                        btn_start.Enabled = true;
                        btn_in_user.Enabled = false;
                        T.Close();
                        Th.Abort();
                        break;
                    case "I":
                        string[] F = Str.Split(',');
                        DialogResult res = MessageBox.Show(F[0] + "邀請和你卡牌對戰是否接受", "邀請訊息", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes)
                        {
                            int I = listBox1.Items.IndexOf(F[0]);
                            listBox1.SetSelected(I, true);
                            listBox1.Enabled = false;
                            btn_in_user.Enabled = false;
                            Send("R" + "Y" + "|" + F[0]);
                            button1.Enabled = true;
                        }
                        else
                        {
                            Send("R" + "N" + "|" + F[0]);
                        }
                        break;
                    case "R":
                        if (Str == "Y")
                        {
                            MessageBox.Show(listBox1.SelectedItem.ToString() + "接受您的邀請，可以開始遊戲了");
                            listBox1.Enabled = false;
                            button1.Enabled = true;
                            btn_in_user.Enabled = false;

                        }
                        else
                        {
                            MessageBox.Show("抱歉!" + listBox1.SelectedItem.ToString() + "拒絕你的邀請!");

                        }
                        break;

                    case "W":
                        string[] A = Str.Split('|');
                        string status = A[0];             // "OK" 或 "FAIL"
                        string playerName = A.Length > 1 ? A[1] : ""; // 提取玩家名稱

                        if (status == "OK")
                        {
                            MessageBox.Show($"成功連接伺服器！玩家：{playerName}");
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("ok");
                            }));
                        }
                        else if (status == "FAIL")
                        {
                            MessageBox.Show($"無法連接伺服器，請檢查玩家名稱：{playerName}");
                            Invoke(new Action(() =>
                            {
                                MessageBox.Show("fail");
                            }));
                        }
                        break;

                }
            }
        }
        private void StartGame(bool isPlayerTurn)
        {
            Form1 form1 = new Form1();  // 创建游戏界面
            form1.OnDataSent += test_send;
            form1.OnCardSent += card_send;  // 绑定事件
            form1.OnCardSkillSent += CardSkill;
            form1.IsPlayerTurn = isPlayerTurn;  // 设置玩家先后手状态
            form1.OpponentName = listBox1.SelectedItem?.ToString();  // 设置对手名称

            form1.Updata_game();
            form1.Show();  // 显示游戏界面
            this.Hide();   // 隐藏当前登录界面
        }
        private void HandleTurnMessage(string turnPlayer)
        {
            // 确保 turnPlayer 和本地用户名比较正确
            if (turnPlayer.Trim() == Txt_username.Text.Trim())
            {
                // 玩家是先手
                Invoke(new Action(() =>
                {
                    StartGame(isPlayerTurn: true); // 启动游戏界面，并设置为先手
                }));
            }
            else
            {
                // 玩家是后手
                Invoke(new Action(() =>
                {
                    StartGame(isPlayerTurn: false); // 启动游戏界面，并设置为后手
                }));
            }
        }
    }
}
