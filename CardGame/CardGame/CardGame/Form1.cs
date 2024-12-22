using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
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
        public bool IsPlayerTurn { get; set; } // 是否轮到玩家操作
        private Form2 form2;
        private Thread Th; // 监听线程
        private bool connected = false; // 是否连接到服务器
        private Socket T; // Socket连接对象
        private Random random = new Random(); // 随机数生成器
        private string User; // 当前用户
        public string OpponentName { get; set; }
        // 玩家和对手的状态
        public string send_server_Message { get; set; }
        public int eHealth { get; set; } = 20;
        public int eEnergy { get; set; } = 7;
        public int eShield { get; set; } = 10;

        public int mHealth { get; set; } = 20;
        public int mEnergy { get; set; } = 7;
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
        private bool isPlayerTurn = true; // 默认是玩家回合
        public Form1()
        {
            InitializeComponent();
            
        }


        private void y_value() //
        {

            OnDataSent?.Invoke(mHealth + "," + mShield + "," + mEnergy ); // 觸發事件
        }
        private void card_value() //隊友的資料OnMonsterSent OnMonsterSentDead
        {

            OnCardSent?.Invoke(card_name + "," + card_attact + "," + card_def+","+card_health); // 觸發事件
        }
        public void Updata_game()
        {

        }

        // 卡牌名稱與描述
        private readonly string[] cardNames = { "丟石頭", "斬擊", "星爆氣流斬", "破甲", "蓄力",
            "治癒", "高級治癒", "龍之啟示", "護甲", "九偉人之鎧",
            "丟石頭", "丟石頭", "丟石頭", "斬擊", "斬擊",
            "治癒", "治癒", "治癒", "護甲", "護甲",
            "護甲", "蓄力", "蓄力", "破甲", "我就是力量的花生",
            "龍之啟示" };
        private readonly string[] cardDescriptions =
        {
            "丟石頭 - 消耗1點能量，對敵方造成2點傷害。",
            "斬擊 - 消耗2點能量，對敵方造成3點傷害。",
            "星爆氣流斬 - 消耗5點能量，造成7點傷害。",
            "破甲 - 消耗3點能量，無視護甲造成4點傷害。",
            "蓄力 - 消耗2點能量，下次攻擊傷害提高3。",
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
            "蓄力 - 消耗2點能量，下次攻擊傷害提高3。",
            "蓄力 - 消耗2點能量，下次攻擊傷害提高3。",
            "破甲 - 消耗3點能量，無視護甲造成4點傷害。",
            "我就是力量的花生 - 消耗7點能量，造成15點傷害。",
            "龍之啟示 - 消耗2點能量，能量最大值+1。"
        };

        // 卡牌類別
        public class Card
        {

            public string Name { get; set; }
            public string Description { get; set; }
            public int EnergyCost { get; set; }

            public int Damage { get; set; }
        }

        private readonly List<Card> cards = new List<Card>
        {
            new Card { Name = "丟石頭", Description = "造成2點傷害，能量消耗1", EnergyCost = 1, Damage = 2 },
            new Card { Name = "斬擊", Description = "造成3點傷害，能量消耗2", EnergyCost = 2, Damage = 3 },
            new Card { Name = "星爆氣流斬", Description = "造成7點傷害，能量消耗5", EnergyCost = 5, Damage = 7 },
            new Card { Name = "破甲", Description = "無視護甲造成4點傷害，能量消耗3", EnergyCost = 3, Damage = 4 },
            new Card { Name = "蓄力", Description = "下次攻擊傷害提高3，能量消耗2", EnergyCost = 2, Damage = 0 },
            new Card { Name = "治癒", Description = "回復2點血量，能量消耗1", EnergyCost = 1, Damage = -2 },
            new Card { Name = "高級治癒", Description = "回復10點血量，能量消耗5", EnergyCost = 5, Damage = -10 },
            new Card { Name = "龍之啟示", Description = "能量最大值+1，能量消耗2", EnergyCost = 2, Damage = 0 },
            new Card { Name = "護甲", Description = "增加2點護甲，能量消耗1", EnergyCost = 1, Damage = 0 },
            new Card { Name = "九偉人之鎧", Description = "增加10點護甲，能量消耗5", EnergyCost = 5, Damage = 0 },
            new Card { Name = "我就是力量的花生", Description = "造成15點傷害，能量消耗7", EnergyCost = 7, Damage = 15 }
        };

        // 加载时初始化界面
        private void Form1_Load(object sender, EventArgs e)
        {
            label15.Text = $"{OpponentName}";
            currentHand = DrawRandomCards(5);
            LoadCards();
            UpdateStatusUI();
            UpdateTurnUI(); // 根据 IsPlayerTurn 设置界面状态
        }


        // 根据玩家先后手状态更新界面
        private void UpdateTurnUI()
        {
            if (IsPlayerTurn)
            {
                label19.Text = "你的回合，請操作！";
                EnableGameControls();
            }
            else
            {
                label19.Text = "等待对方操作...";
                DisableGameControls();
            }
        }
        private void EnableGameControls()
        {
            button1.Enabled = true;
            foreach (Control ctrl in Controls)
            {
                if (ctrl is PictureBox)
                {
                    ctrl.Enabled = true;
                }
            }
        }

        // 禁用操作控件
        private void DisableGameControls()
        {
            button1.Enabled = false;
            foreach (Control ctrl in Controls)
            {
                if (ctrl is PictureBox)
                {
                    ctrl.Enabled = false;
                }
            }
        }

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
            label19.Text = "";
        }

        // 點擊卡牌後執行卡牌效果
        private void Card_Click(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            int index = int.Parse(pb.Tag.ToString());
            Card selectedCard = currentHand[index];

            if (mEnergy < selectedCard.EnergyCost)
            {
                MessageBox.Show("能量不足，无法使用此卡牌！");
                return;
            }

            ApplyCardEffect(selectedCard);
            SendCardEffectToServer(selectedCard);
            LogToListBox2($"[INFO] 使用卡牌: {selectedCard.Name}，能量消耗: {selectedCard.EnergyCost}");
            UpdateStatusUI();
        }


        // 应用卡牌效果
        private void ApplyCardEffect(Card card)
        {
            mEnergy -= card.EnergyCost;

            if (card.Damage > 0)
            {
                eHealth = Math.Max(0, eHealth - card.Damage); // 减少敌人血量
            }
            else if (card.Damage < 0)
            {
                mHealth = Math.Min(20, mHealth - card.Damage); // 恢复自己血量
            }

            if (card.Name == "護甲")
            {
                mShield += 2;
            }
            else if (card.Name == "九偉人之鎧")
            {
                mShield += 10;
            }
            else if (card.Name == "蓄力")
            {
                // 蓄力效果：下次攻击加成
                // 实现逻辑略
            }
            else if (card.Name == "龍之啟示")
            {
                mEnergy += 1;
            }
        }

        // 更新界面状态
        private void UpdateStatusUI()
        {
            label13.Text = mHealth.ToString();
            label14.Text = mEnergy.ToString();
            label20.Text = mShield.ToString();

            label16.Text = eHealth.ToString();
            label17.Text = eEnergy.ToString();
            label23.Text = eShield.ToString();
        }
        private void SendCardEffectToServer(Card card)
        {
            string message = $"EFFECT|{card.Name},{card.EnergyCost},{card.Damage}";
            SendMessageToServer(message);
            LogToListBox2($"[SEND] 发送卡牌效果: {message}");
        }

        private void SendMessageToServer(string message)
        {
            try
            {
                // 检查 Socket 是否已初始化并连接
                if (T == null || !T.Connected)
                {
                    LogToListBox2("[ERROR] 发送消息失败：Socket 未连接。");
                    MessageBox.Show("无法连接到服务器，消息发送失败。");
                    return;
                }

                byte[] messageBytes = Encoding.Default.GetBytes(message);
                T.Send(messageBytes, 0, messageBytes.Length, SocketFlags.None);
                LogToListBox2($"[SEND] 消息已发送：{message}");
            }
            catch (SocketException ex)
            {
                LogToListBox2($"[ERROR] 网络错误：{ex.Message}");
            }
            catch (Exception ex)
            {
                LogToListBox2($"[ERROR] 发送消息失败：{ex.Message}");
            }
        }
        private void ProcessOpponentEffect(string effectDetails)
        {
            string[] parts = effectDetails.Split(',');
            string cardName = parts[0];
            int energyCost = int.Parse(parts[1]);
            int damage = int.Parse(parts[2]);

            eHealth = Math.Max(0, eHealth - damage);
            Invoke((MethodInvoker)UpdateStatusUI);
            LogToListBox2($"[INFO] 对手使用卡牌: {cardName}，造成伤害: {damage}");
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
        // 发送状态更新到服务器
        private void SendStatusUpdate()
        {
            string message = $"S{mHealth},{mShield},{mEnergy},{eHealth},{eShield},{eEnergy}";
            Send(message);
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

        private void CardskillSent(string str) //隊友的資料OnMonsterSent OnMonsterSentDead OnMSkillSent OnReStartSent;
        {

            OnCardSkillSent?.Invoke(str); // 觸發事件
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //結束回合輪到對手
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            if (T != null)
            { T.Close(); }
            Application.Exit();  // 關閉整個應用程式    
        }
        // 在控件动态调整或添加后调用
        

    }
}