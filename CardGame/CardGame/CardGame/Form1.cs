using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CardGame.Form1;

namespace CardGame
{
    public partial class Form1 : Form
    {
        public event Action<string> OnDataSent;
        public event Action<string> OnCardSent;
        public event Action<string> OnCardSkillSent;
        public event Action<string> OnTurnSent;
        private Form2 form2;
        private Thread Th; // 监听线程
        private bool connected = false; // 是否连接到服务器
        private Socket T; // Socket连接对象
        private Random random = new Random(); // 随机数生成器
        private string User; // 当前用户
        // 是否輪到玩家操作
        public bool IsPlayerTurn { get; set; } = false;
        private bool isGameOver = false; // 標記遊戲是否結束

        public string OpponentName { get; set; }
        // 玩家和对手的状态
        public string send_server_Message { get; set; }

        public int MaxEnergy = 5 ; // 定義每回合的最大能量

        public int eHealth { get; set; } = 20;
        public int eEnergy { get; set; } = 5;
        public int eShield { get; set; } = 10;

        public int mHealth { get; set; } = 20;
        public int mEnergy { get; set; } = 5;
        public int mShield { get; set; } = 10;
        //伺服器設定
        public int server_port;
        public string server_ip;
        //卡牌效果
        public int card_attact { get; set; }
        public string card_name { get; set; }
        public int card_def { get; set; }
        public int card_health { get; set; }

        private List<Card> currentHand; // 当前手牌
        private bool isCardClicked = false;
        
        public Form1()
        {
            InitializeComponent();
           button1.Enabled = false;
            // 隨機決定是否是先手
            IsPlayerTurn = random.Next(0, 2) == 0;
            SetControlsEnabled(IsPlayerTurn); // 如果是玩家回合，啟用控件
            // 如果是玩家回合，啟用按鈕
            if (IsPlayerTurn)
            {
                MessageBox.Show("你的回合開始！");
                button1.Enabled = true;
            }
            else
            {
                MessageBox.Show("對手的回合開始！");
                button1.Enabled = false;
            }
        }


        private void y_value() //
        {

            OnDataSent?.Invoke(mHealth + "," + mShield + "," + mEnergy ); // 觸發事件
        }
        private void card_value(string card_name, int card_attact, int card_def, int card_health, int mEnergy, int mHealth, int mShield)
        {

            OnCardSent?.Invoke(card_name + "," + card_attact + "," + card_def + "," + card_health + ","+mEnergy + "," + mHealth + "," + mShield); // 觸發事件
        }
        private void End_Turn() 
        {
            OnTurnSent?.Invoke("T");
        }
        public void Updata_game()
        {

        }

        // 卡牌名稱與描述
        private readonly string[] cardNames = { "丟石頭", "斬擊", "星爆氣流斬", "破甲",
            "治癒", "高級治癒", "龍之啟示", "護甲", "九偉人之鎧",
            "丟石頭", "丟石頭", "丟石頭", "斬擊", "斬擊",
            "治癒", "治癒", "治癒", "護甲", "護甲",
            "護甲", "破甲", "我就是力量的花生",
            "龍之啟示" };
        private readonly string[] cardDescriptions =
        {
            "丟石頭 - 消耗1點能量，對敵方造成2點傷害。",
            "斬擊 - 消耗2點能量，對敵方造成3點傷害。",
            "星爆氣流斬 - 消耗5點能量，造成7點傷害。",
            "破甲 - 消耗3點能量，造成4點傷害。",
            
            "治癒 - 消耗1點能量，恢復2點血量。",
            "高級治癒 - 消耗5點能量，恢復10點血量。",
            "龍之啟示 - 消耗2點能量，能量最大值+1。",
            "護甲 - 消耗1點能量，增加2點護甲。",
            "九偉人之鎧 - 消耗5點能量，增加10點護甲。",
            "丟石頭 - 消耗1點能量，對敵方造成2點傷害。",
            "丟石頭 - 消耗1點能量，對敵方造成2點傷害。",
            "丟石頭 - 消耗1點能量，對敵方造成2點傷害。",
            "斬擊 - 消耗2點能量，對敵方造成3點傷害。",
            "斬擊 - 消耗2點能量，對敵方造成3點傷害。",
            "治癒 - 消耗1點能量，恢復2點血量。",
            "治癒 - 消耗1點能量，恢復2點血量。",
            "治癒 - 消耗1點能量，恢復2點血量。",
            "護甲 - 消耗1點能量，增加2點護甲。",
            "護甲 - 消耗1點能量，增加2點護甲。",
            "護甲 - 消耗1點能量，增加2點護甲。",
            
            "破甲 - 消耗3點能量，造成4點傷害。",
            "我就是力量的花生 - 消耗7點能量，造成15點傷害。",
            "龍之啟示 - 消耗2點能量，能量最大值+1。"
        };

        // 卡牌類別
        public class Card
        {

            public string Name { get; set; }
            public string Description { get; set; }
            public int EnergyCost { get; set; }
            public int Health { get; set; }
            public int Damage { get; set; }

            public int Deffence { get; set; }

            public int Energy { get; set; }
        }

        private readonly List<Card> cards = new List<Card>
        {
            new Card { Name = "丟石頭", Description = "造成2點傷害，能量消耗1", EnergyCost = 1, Damage = 2 , Health= 0, Deffence = 0,Energy = 0},
            new Card { Name = "斬擊", Description = "造成3點傷害，能量消耗2", EnergyCost = 2, Damage = 3 , Health= 0, Deffence = 0,Energy = 0},
            new Card { Name = "星爆氣流斬", Description = "造成7點傷害，能量消耗5", EnergyCost = 5, Damage = 7 , Health= 0, Deffence = 0,Energy = 0},
            new Card { Name = "破甲", Description = "造成4點傷害，能量消耗3", EnergyCost = 3, Damage = 4 , Health= 0, Deffence = 0, Energy = 0},
            new Card { Name = "治癒", Description = "回復2點血量，能量消耗1", EnergyCost = 1, Damage = 0, Health= 2, Deffence = 0,Energy = 0 },
            new Card { Name = "高級治癒", Description = "回復10點血量，能量消耗5", EnergyCost = 5, Damage = 0, Health= 10, Deffence = 0,Energy = 0 },
            new Card { Name = "龍之啟示", Description = "能量最大值+1，能量消耗2", EnergyCost = 3, Damage = 0 , Health= 0, Deffence = 0, Energy = 1},
            new Card { Name = "護甲", Description = "增加2點護甲，能量消耗1", EnergyCost = 1, Damage = 0 , Health= 0, Deffence = 2,Energy = 0},
            new Card { Name = "九偉人之鎧", Description = "增加10點護甲，能量消耗5", EnergyCost = 5, Damage = 0, Health= 0, Deffence = 10 ,Energy = 0},
            new Card { Name = "我就是力量的花生", Description = "造成15點傷害，能量消耗7", EnergyCost = 7, Damage = 15, Health= 0, Deffence = 0 ,Energy = 0}
        };

        // 加载时初始化界面
        private void Form1_Load(object sender, EventArgs e)
        {
            label15.Text = $"{OpponentName}";
            currentHand = DrawRandomCards(5);
            LoadCards();
            UpdateStatusUI();

        }


        // 卡牌名稱與描述


        // 随机抽取卡牌
        private List<Card> DrawRandomCards(int count)
        {
            return cards.OrderBy(c => random.Next()).Take(count).ToList();
        }

        // 加载卡牌到界面
        private void LoadCards()
        {
            PictureBox[] cardSlots = { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5 };

            for (int i = 0; i < cardSlots.Length; i++)
            {
                cardSlots[i].Image = GetCardImage(currentHand[i].Name);
                cardSlots[i].Tag = i; // 使用 Tag 标记卡牌索引
                cardSlots[i].MouseEnter += Card_MouseEnter;
                cardSlots[i].MouseLeave += Card_MouseLeave;
                cardSlots[i].Click += Card_Click;
            }
        }

        // 获取卡牌图片
        private Image GetCardImage(string cardName)
        {
            switch (cardName)
            {
                case "丟石頭": return Properties.Resources.No_01;
                case "斬擊": return Properties.Resources.No_13;
                case "護甲": return Properties.Resources.No_09;
                case "治癒": return Properties.Resources.No_05;
                case "蓄力": return Properties.Resources.No_18;
                case "星爆氣流斬": return Properties.Resources.No_25;
                case "破甲": return Properties.Resources.No_16;
                case "高級治癒": return Properties.Resources.No_21;
                case "龍之啟示": return Properties.Resources.NO_22;
                case "九偉人之鎧": return Properties.Resources.No_24;
                case "我就是力量的花生": return Properties.Resources.No_26;
                case "卡背": return Properties.Resources.No_99;
                default: return Properties.Resources.No_01;
            }
        }

        // 當鼠標懸停顯示卡牌描述
        private void Card_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            int index = int.Parse(pb.Tag.ToString());
            label19.Text = currentHand[index].Description;
        }

        // 當鼠標離開隱藏卡牌描述
        private void Card_MouseLeave(object sender, EventArgs e)
        {
            if (isCardClicked) return; // 如果已點擊卡牌，不清空 label19
            label19.Text = "";
        }
        
        // 點擊卡牌後執行卡牌效果
        private void Card_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            if (!pb.Enabled)
            {
                
                return;
            }
            int index = int.Parse(pb.Tag.ToString());
            Card selectedCard = currentHand[index];
            //MessageBox.Show(selectedCard.Damage+"");

            if (MaxEnergy < selectedCard.EnergyCost)
            {
                MessageBox.Show("能量不足，无法使用此卡牌！");
                return;
            }
            // 設置標誌，防止鼠標事件干擾
            isCardClicked = true;
            ApplyCardEffect(selectedCard);
            
            SendCardEffectToServer(selectedCard);
            LogToListBox2($"[INFO] 使用卡牌: {selectedCard.Name}，能量消耗: {selectedCard.EnergyCost}");
            // 更改卡牌圖片並禁用點擊
            pb.Image = Properties.Resources.No_99; // 替換為使用後的圖片資源
            pb.Enabled = false; // 禁用點擊功能

            label19.Text = $"使用卡牌: {selectedCard.Name}，能量消耗: {selectedCard.EnergyCost}\n" +
                   $"傷害: {selectedCard.Damage}，護盾增加: {selectedCard.Deffence}，" +
                   $"血量增加: {selectedCard.Health}，最大能量值增加: {selectedCard.Energy}";
            UpdateStatusUI();
        }
        public void ListboxUpdata(string cmd)
        {
            listBox2.Items.Add("接收傳送的訊息: " + cmd);

            // 確保 eHealth 從 UI 控件中獲取正確值
            //if (int.TryParse(label16.Text, out int currentHealth))
            //{
              //  eHealth = currentHealth;
            //}
            //else
            //{
              //  MessageBox.Show("敵方血量數據無效！");
            //}
            // 更新敵方數據
            string[] parts = cmd.Split(',');
            if (parts.Length >= 3 && int.TryParse(parts[0], out int health) && int.TryParse(parts[1], out int shield) && int.TryParse(parts[2], out int energy))
            {
                label16.Text = parts[4];
                label23.Text = parts[5];
                eEnergy = energy;
            }
            UpdateStatusUI();
        }


        // 应用卡牌效果
        private void ApplyCardEffect(Card card)
        {
            mEnergy -= card.EnergyCost; // 扣除卡牌的能量消耗

            if (card.Name == "護甲")
            {
                mShield += 2; // 增加護甲
            }
            else if (card.Name == "九偉人之鎧")
            {
                mShield += 10; // 增加大量護甲
            }
            else if (card.Name == "丟石頭" || card.Name == "斬擊" || card.Name == "星爆氣流斬"|| card.Name == "破甲" || card.Name == "我就是力量的花生")
            {
                int damage = card.Damage; // 根據卡牌定義的傷害值
                if (eShield >= damage)
                {
                    eShield -= damage; // 優先扣除護盾
                }
                else
                {
                    int remainingDamage = damage - eShield;
                    eShield = 0; // 把護盾扣為 0
                    eHealth = eHealth - remainingDamage;
                }
            }
            else if(card.Name == "治癒" || card.Name == "高級治癒")
            {
                mHealth += card.Health;
            }
            else if (card.Name == "龍之啟示")
            {
                MaxEnergy += 1; // 增加最大能量值
                mEnergy = Math.Min(MaxEnergy, mEnergy + 1); // 增加能量
            }

            
        }

        // 更新界面状态
        public void UpdateStatusUI()
        {
            
            label13.Text = mHealth.ToString();
            label14.Text = mEnergy.ToString();
            label20.Text = mShield.ToString();

            label16.Text = eHealth.ToString();
            label17.Text = eEnergy.ToString();
            label23.Text = eShield.ToString();
            CheckHealth();
        }
       
        private void SendCardEffectToServer(Card card)
        {
            string message = $"EFFECT|{card.Name},{card.EnergyCost},{card.Damage},{mEnergy},{mHealth},{mShield}";
            card_value(card.Name, card.EnergyCost, card.Damage, 0,mEnergy,mHealth,mShield);
            //SendMessageToServer(message);
            LogToListBox2($"[SEND] 发送卡牌效果: {message}");
        }
        
        private void SendTurnEndtoServer()
        {

            string message = $"TURNEND|";
            End_Turn();
            LogToListBox2($"[SEND]回合結束:{message}");
        }
        
        

        private void LogToListBox2(string message)
        {
            if (listBox2.InvokeRequired)
            {
                listBox2.Invoke(new Action(() => listBox2.Items.Add(message)));
            }
            else
            {
                listBox2.Items.Add(message);
            }
        }
       

        // 发送消息到服务器
        private void Send(string message)
        {
            try
            {
                byte[] buffer = Encoding.Default.GetBytes(message);
                T.Send(buffer, 0, buffer.Length, SocketFlags.None);
                listBox2.Items.Add($"[SEND] {message}");
            }
            catch (Exception ex)
            {
                listBox2.Items.Add($"[ERROR] 發送失敗：{ex.Message}");
            }
        }


        

        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsPlayerTurn) return; // 防止非法操作

            mEnergy = MaxEnergy; // 重置能量為最大值
            
            SendTurnEndtoServer();
            
            SetControlsEnabled(false); // 禁用控件
            IsPlayerTurn = false; // 切換狀態
            label19.Text = "回合結束，等待對手操作...";

            // 清空現有手牌
            currentHand.Clear();

            // 抽取新手牌
            currentHand = DrawRandomCards(5);

            // 更新界面上的卡牌顯示
            LoadCards();

        }
        public void SetControlsEnabled(bool enabled)

        {
            button1.Enabled = enabled; // 控制回合結束按鈕
            pictureBox1.Enabled = enabled;
            foreach (Control control in Controls)
            {
                if (control is PictureBox || control is Button)
                {
                    control.Enabled = enabled;
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (T != null)
            { T.Close(); }
            Application.Exit();  // 關閉整個應用程式    
        }
        private void CheckHealth()
        {
            // 避免重複觸發遊戲結束
            if (isGameOver) return;

            // 解析玩家血量和敵人血量
            int playerHealth = int.Parse(label13.Text);
            int enemyHealth = int.Parse(label16.Text);

            if (playerHealth <= 0)
            {
                isGameOver = true; // 標記遊戲結束
                ShowGameResult("敵人獲勝！");
            }
            else if (enemyHealth <= 0)
            {
                isGameOver = true; // 標記遊戲結束
                ShowGameResult("玩家獲勝！");
            }
        }

        private void ShowGameResult(string result)
        {
            // 跳轉到 Form3 並傳遞結果
            Form3 resultForm = new Form3(result);
            resultForm.Show();
            this.Hide(); // 隱藏當前表單
        }



    }
}