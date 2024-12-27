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
            //Turn = -1; // 默認值，表示未決定
        }
        //公用變數
        Socket T;                                          //通訊物件
        Thread Th;                                         //網路監聽執行緒
        string User;    //使用者
        string my;    
        Form1 form1 = new Form1();
        private string send_sever_message;
        public bool IsPlayerTurn; // 是否是玩家回合
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
            // 隨機決定先後手
            

            form1.OnDataSent += test_send;
            form1.OnCardSent += card_send;// 實例
            form1.OpponentName = listBox1.SelectedItem?.ToString(); // 設置對手名稱
            form1.OnCardSkillSent += CardSkill;
            form1.OnTurnSent += turn_send;
            form1.Updata_game();
            Send($"START|{User}|{form1.OpponentName}");
            form1.Show();                // 顯示 Form1
            this.Hide();                 // 隱藏當前的 Form (例如: Form2)
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
                btn_start.Enabled = true;
            }
            btn_in_user.Enabled = true;
            btn_start.Enabled = false;

        }
        public void Send(string Str)
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
        public void turn_send(string cmd)
        { 
            Send("TURNEND"+cmd+"|"+listBox1.SelectedItem);
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
                    case "M":
                        string[] C = Str.Split(',');
                        MessageBox.Show("st:" + St + "str" + Str + "A" + C[2] + C[3]); // 第一個 卡牌名稱0 第二個 卡牌消耗1 第三個是 傷害2 第四個 不知道是啥3
                        if (form1.mShield > int.Parse(C[2]))
                        {
                            form1.mShield -= int.Parse(C[2]);
                        }
                        else if (form1.mShield < int.Parse(C[2]))
                            {
                                int damage = int.Parse(C[2]) - form1.mShield;
                                form1.mHealth -= damage;
                            }
                            
                        MessageBox.Show(form1.mHealth + " ");
                        form1.ListboxUpdata(form1.mHealth + Str);
                        form1.UpdateStatusUI();
                        break;

                    case "TURNEND":
                        try
                        {
                            string[] parts = Str.Split('|');
                            if (parts.Length < 2)
                            {
                                MessageBox.Show("[ERROR] 消息格式错误: " + Str);
                                return;
                            }


                            string receiver = parts[1];

                            MessageBox.Show($"接收到 TURNEND 消息: , 接收者 {receiver}");

                            if (receiver == User) // 确保是发给自己的消息
                            {
                                IsPlayerTurn = true;
                                form1.mEnergy = form1.MaxEnergy;
                                MessageBox.Show("现在是你的回合！");

                                Invoke(new Action(() =>
                                {
                                    button1.Enabled = true;
                                    foreach (Control control in Controls)
                                    {
                                        if (control is PictureBox || control is Button)
                                        {
                                            control.Enabled = true;
                                        }
                                    }
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("[ERROR] 处理 TURNEND 消息时出错: " + ex.Message);
                        }
                        break;



                    case "L":
                        listBox1.Items.Clear();
                        string[] M = Str.Split(',');
                        for (int i = 0; i < M.Length; i++) listBox1.Items.Add(M[i]);
                        break;
                    case "5":
                        DialogResult result = MessageBox.Show("是排遊玩卡牌對戰" , "重玩訊息", MessageBoxButtons.YesNo);
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



                }
            }
        }
    }
}
