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
        string User;                                       //使用者

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();  // 創建 Form1 實例
            form1.Show();                // 顯示 Form1
            this.Hide();                 // 隱藏當前的 Form (例如: Form2)
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show("謝謝遊玩!");
            Application.Exit();  // 關閉整個應用程式    
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            try
            {
                string ip = textBox2.Text.Trim();
                int port = int.Parse(textBox1.Text.Trim());
                string playerName = Txt_username.Text.Trim();

                // Validate inputs
                if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(playerName))
                {
                    MessageBox.Show("請填寫伺服器IP和玩家名稱", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Connect to server
                client = new TcpClient();
                client.Connect(ip, port);

                // Update system message
                UpdateSystemMessage("成功連接到伺服器！");

                // Start listening for server messages
                StartListening();

                // Send player info to server
                SendPlayerInfo(playerName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"連接失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateSystemMessage("連接伺服器失敗");
            }
        }



        private void btnInvitePlayer_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("請選擇要邀請的玩家", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selectedPlayer = listBox1.SelectedItem.ToString();
            SendInvitation(selectedPlayer);
            UpdateSystemMessage($"已發送邀請給 {selectedPlayer}");
        }

        private void StartListening()
        {
            // Start async listening for server messages
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                int bytesRead = stream.EndRead(ar);

                if (bytesRead > 0)
                {
                    string message = System.Text.Encoding.UTF8.GetString(ar.AsyncState as byte[], 0, bytesRead);
                    ProcessServerMessage(message);

                    // Continue listening
                    byte[] buffer = new byte[1024];
                    stream.BeginRead(buffer, 0, buffer.Length, ReadCallback, buffer);
                }
            }
            catch (Exception ex)
            {
                UpdateSystemMessage($"接收訊息錯誤: {ex.Message}");
            }
        }

        private void SendPlayerInfo(string playerName)
        {
            if (client != null && client.Connected)
            {
                NetworkStream stream = client.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"PLAYER_INFO:{playerName}");
                stream.Write(data, 0, data.Length);
            }
        }

        private void SendInvitation(string targetPlayer)
        {
            if (client != null && client.Connected)
            {
                NetworkStream stream = client.GetStream();
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"INVITE:{targetPlayer}");
                stream.Write(data, 0, data.Length);
            }
        }


        private void ProcessServerMessage(string message)
        {
            // Invoke UI updates on the main thread
            this.BeginInvoke(new Action(() =>
            {
                if (message.StartsWith("PLAYER_LIST:"))
                {
                    // Update online players list
                    string[] players = message.Substring(12).Split(',');
                    listBox1.Items.Clear();
                    listBox1.Items.AddRange(players);
                }
                else if (message.StartsWith("INVITE_RECEIVED:"))
                {
                    string inviter = message.Substring(15);
                    if (MessageBox.Show($"是否接受來自 {inviter} 的邀請？", "遊戲邀請",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        SendInvitationResponse(inviter, true);
                    }
                    else
                    {
                        SendInvitationResponse(inviter, false);
                    }
                }
            }));
        }
        private void SendInvitationResponse(string inviter, bool accepted)
        {
            if (client != null && client.Connected)
            {
                NetworkStream stream = client.GetStream();
                string response = accepted ? "ACCEPT" : "DECLINE";
                byte[] data = System.Text.Encoding.UTF8.GetBytes($"INVITE_RESPONSE:{inviter}:{response}");
                stream.Write(data, 0, data.Length);
            }
        }

        private void UpdateSystemMessage(string message)
        {
            // Assume there's a TextBox named txtSystemMessage for system messages
            if (Txt_system_message.InvokeRequired)
            {
                Txt_system_message.BeginInvoke(new Action(() => Txt_system_message.Text = message));
            }
            else
            {
                Txt_system_message.Text = message;
            }
        }
    }
}
