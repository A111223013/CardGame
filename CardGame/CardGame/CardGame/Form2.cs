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
            Form4 form4 = new Form4();
            if (form4.ShowDialog() == DialogResult.OK) // 等待 Form4 返回結果
            {
                int turn = form4.Turn; // 獲取先後手值
                Form1 form1 = new Form1(turn); // 將先後手值傳遞給 Form1 的構造函數
                form1.Show(); // 顯示 Form1
                this.Hide(); // 隱藏當前的 Form2
            }
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
            Control.CheckForIllegalCrossThreadCalls = false;
            User = Txt_username.Text;
            if (string.IsNullOrWhiteSpace(User))
            {
                MessageBox.Show("使用者名稱不可為空！");
                return;
            }
            string IP = textBox2.Text;
            int Port = int.Parse(textBox1.Text);
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
                        DialogResult result = MessageBox.Show("是否遊玩卡排對戰" , "重玩訊息", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes)
                        {
                            listBox1.Text = Str;
                            listBox1.Enabled = false;
                            Send("P" + "Y" + "|" + listBox1.SelectedItem);
                            Random rnd = new Random();
                            int[] mark = new int[25];
                            int num;

                            for (int i = 0; i < 25; i++)
                            {
                                Button D = (Button)this.Controls["B" + i.ToString()];
                                D.Enabled = true;
                                D.Tag = "_";




                            }

                            for (int i = 0; i < 25; i++) mark[i] = 0;
                            for (int i = 0; i < 25; i++)
                            {
                                do
                                {
                                    num = rnd.Next(0, 25);
                                } while (mark[num] != 0);
                                mark[num] = 1;
                                this.Controls["B" + i.ToString()].Text = (num + 1).ToString();
                            }

                            Txt_system_message.Text = "輪我下";
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
                           
                            Random rnd = new Random();
                            int[] mark = new int[25];
                            int num;
                            for (int i = 0; i < 25; i++)
                            {
                                Button D = (Button)this.Controls["B" + i.ToString()];
                                D.Enabled = true;

                                D.Tag = "_";
                            }
                            for (int i = 0; i < 25; i++) mark[i] = 0;
                            for (int i = 0; i < 25; i++)
                            {
                                do
                                {
                                    num = rnd.Next(0, 25);
                                } while (mark[num] != 0);
                                mark[num] = 1;
                                this.Controls["B" + i.ToString()].Text = (num + 1).ToString();
                            }

                            Txt_system_message.Text = "輪我下";
                            btn_in_user.Enabled = false;


                        }
                        else
                        {
                            
                            MessageBox.Show("抱歉" + listBox1.SelectedItem.ToString() + "覺得你太爛!");
                        }


                        break;
                    case "6":
                        string[] A = Str.Split(':');
                        if (A[1] != "-1")
                        {
                            for (int i = 0; i < 25; i++)
                            {
                                if (this.Controls["B" + i.ToString()].Text == A[1])
                                {
                                    this.Controls["B" + i.ToString()].Tag = "0";
                                    this.Controls["B" + i.ToString()].Enabled = false;
                                    this.Controls["B" + i.ToString()].BackColor = Color.Red;
                                    break;
                                }
                            }


                            my = "";
                            for (int i = 0; i < 25; i++)
                            {
                                my += this.Controls["B" + i.ToString()].Tag;
                            }






                            byte[] K = Encoding.Default.GetBytes(my + ":" + "-1");
                            Send("6" + my + ":" + "-1" + "|" + listBox1.SelectedItem);

                        }
                        break;
                        //Txt_system_message.Text = "";
                        //bool iwin = Chk(my);
                        //bool youwin = Chk(A[0]);

                        //if (!iwin && !youwin)
                        //{
                        //    if (A[1] != "-1")
                        //    {
                               
                        //        Txt_system_message.Text = "輪我下";
                        //    }
                        //    else
                        //    {
                               
                        //        Txt_system_message.Text = "輪到對手下";
                        //    }
                        //}
                        //else
                        //{
                            
                        //    Txt_system_message.Text = "以分出勝負";

                        //    if (iwin && youwin)
                        //    {
                        //        Txt_system_message.Text = "平手";
                        //    }
                        //    else if (youwin)
                        //    {
                        //        Txt_system_message.Text = "你輸了";
                        //    }
                        //    else if (iwin)
                        //    {
                        //        Txt_system_message.Text = "你贏了";
                        //    }
                        //    listBox1.Enabled = true;
                            
                        //    btn_in_user.Enabled = true;
                        //}


                        //break;
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
