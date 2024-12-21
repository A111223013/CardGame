using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardGame
{
    public partial class Form1 : Form
    {
        private Thread Th; // 監聽執行緒
        private bool connected = false; // 是否連接
        public static bool Turn = true; // 共享回合狀態
        Socket T;
        string User;
        bool ive = false;

        string my;

        public Form1()
        {
            InitializeComponent();
        }
        int shield = 10;
        int heart = 20;
        int energy = 5;
        // 定義卡牌手牌
        // 選擇卡牌 (點擊 PictureBox)
        private string selectedCard = "";
        // 定義卡牌手牌
        private List<Card> currentHand; // 當前手牌
        private Random random = new Random();
        private bool isPlayerTurn = true; // 預設玩家回合
                                          // 玩家類別與主要屬性
        public class Player
        {
            public int Health { get; set; } = 20;
            public int Shield { get; set; } = 0;
            public int Energy { get; set; } = 10;

            public void TakeDamage(int damage)
            {
                if (Shield > 0)
                {
                    int remainingDamage = damage - Shield;
                    Shield = Math.Max(0, Shield - damage);
                    Health = Math.Max(0, Health - Math.Max(0, remainingDamage));
                }
                else
                {
                    Health = Math.Max(0, Health - damage);
                }
            }

            public void BuffAttack(int value)
            {
                // 假設增加玩家攻擊力的狀態（未實現完全邏輯）
            }
        }

        private void UpdateButtonStates()
        {
            // 假設 button1 是用於結束回合的按鈕，永遠應該啟用
            button1.Enabled = true;

            // 根據當前回合切換按鈕啟用狀態
            foreach (var control in this.Controls)
            {
                if (control is Button btn)
                {
                    if (Turn) // 玩家1的回合
                    {
                        if (btn.Name.StartsWith("Player1"))
                            btn.Enabled = true; // 啟用玩家1的按鈕
                        else if (btn.Name.StartsWith("Player2"))
                            btn.Enabled = false; // 禁用玩家2的按鈕
                    }
                    else // 玩家2的回合
                    {
                        if (btn.Name.StartsWith("Player2"))
                            btn.Enabled = true; // 啟用玩家2的按鈕
                        else if (btn.Name.StartsWith("Player1"))
                            btn.Enabled = false; // 禁用玩家1的按鈕
                    }
                }
            }
        }


        // 敵人類別與行為
        public class Enemy
        {
            public int Health { get; set; } = 20;
            public int Shield { get; set; } = 5;

            public void TakeDamage(int damage)
            {
                if (Shield > 0)
                {
                    int remainingDamage = damage - Shield;
                    Shield = Math.Max(0, Shield - damage);
                    Health = Math.Max(0, Health - Math.Max(0, remainingDamage));
                }
                else
                {
                    Health = Math.Max(0, Health - damage);
                }
            }

            public void TakeTrueDamage(int damage)
            {
                Health = Math.Max(0, Health - damage);
            }

            public void PerformAction(Player player)
            {
                MessageBox.Show("敵人攻擊你，造成3點傷害！");
                player.TakeDamage(3);
            }
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
            public int CardID { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int EnergyCost { get; set; }
        }

        private List<Card> cards = new List<Card>
        {
            new Card { CardID = 1, Name = "丟石頭", Description = "造成2點傷害，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 2, Name = "斬擊", Description = "造成3點傷害，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 3, Name = "星爆氣流斬", Description = "造成7點傷害，能量消耗5", EnergyCost = 5 },
            new Card { CardID = 4, Name = "破甲", Description = "無視護甲造成4點傷害，能量消耗3", EnergyCost = 3 },
            new Card { CardID = 5, Name = "蓄力", Description = "下次攻擊傷害提高3，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 6, Name = "治癒", Description = "回2點血量，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 7, Name = "高級治癒", Description = "回10點血量，能量消耗5", EnergyCost = 5 },
            new Card { CardID = 8, Name = "龍之啟示", Description = "能量最大值+1，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 9, Name = "護甲", Description = "護甲+2，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 10, Name = "九偉人之鎧", Description = "護甲+10，能量消耗5", EnergyCost = 5 },
            new Card { CardID = 11, Name = "丟石頭", Description = "造成2點傷害，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 12, Name = "丟石頭", Description = "造成2點傷害，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 13, Name = "丟石頭", Description = "造成2點傷害，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 14, Name = "斬擊", Description = "造成3點傷害，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 15, Name = "斬擊", Description = "造成3點傷害，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 16, Name = "治癒", Description = "回2點血量，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 17, Name = "治癒", Description = "回2點血量，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 18, Name = "治癒", Description = "回2點血量，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 19, Name = "護甲", Description = "護甲+2，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 20, Name = "護甲", Description = "護甲+2，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 21, Name = "護甲", Description = "護甲+2，能量消耗1", EnergyCost = 1 },
            new Card { CardID = 22, Name = "蓄力", Description = "下次攻擊傷害提高3，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 23, Name = "蓄力", Description = "下次攻擊傷害提高3，能量消耗2", EnergyCost = 2 },
            new Card { CardID = 24, Name = "破甲", Description = "無視護甲造成4點傷害，能量消耗3", EnergyCost = 3 },
            new Card { CardID = 25, Name = "我就是力量的花生", Description = "給予15點傷害，能量消耗71", EnergyCost = 7 },
            new Card { CardID = 26, Name = "龍之啟示", Description = "能量最大值+1，能量消耗2", EnergyCost = 2 }
        };


        // 卡牌名稱與描述

        private void Form1_Load(object sender, EventArgs e)
        {
            currentHand = DrawRandomCards(5); // 初始抽取五張卡牌
            LoadCards();
            UpdateStatus();
        }
        // 隨機抽取卡牌
        private List<Card> DrawRandomCards(int count)
        {
            return cards.OrderBy(c => random.Next()).Take(count).ToList();
        }

        // 根據卡牌名稱取得圖片
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

        // 顯示卡牌到畫面
        private void LoadCards()
        {
            PictureBox[] cardSlots = { pictureBox1, pictureBox2, pictureBox3, pictureBox4, pictureBox5 };

            for (int i = 0; i < cardSlots.Length; i++)
            {
                cardSlots[i].Image = GetCardImage(currentHand[i].Name);
                cardSlots[i].Tag = i; // 使用 Tag 來標記卡牌位置
                cardSlots[i].MouseEnter += Card_MouseEnter;
                cardSlots[i].MouseLeave += Card_MouseLeave;
                cardSlots[i].Click += Card_Click;
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
            Card selectedCard = currentHand[index]; // 從手牌中選取卡牌
            PlayCardEffect(selectedCard);
        }

        // 執行卡牌效果
        private void PlayCardEffect(Card selectedCard)
        {
            if (energy < selectedCard.EnergyCost)
            {
                MessageBox.Show("能量不足！無法使用此卡牌。");
                return;
            }

            energy -= selectedCard.EnergyCost;

            switch (selectedCard.Name)
            {
                case "丟石頭":
                    MessageBox.Show("你丟出石頭，對敵方造成2點傷害！");

                    break;
                case "斬擊":
                    MessageBox.Show("你使用斬擊，造成3點傷害！");
                    break;
                case "星爆氣流斬":
                    MessageBox.Show("你使出星爆氣流斬，造成7點傷害！");
                    break;
                case "破甲":
                    MessageBox.Show("破甲發動！無視護甲造成4點傷害。");
                    break;
                case "蓄力":
                    MessageBox.Show("你蓄力了！下次攻擊傷害提高3點。");
                    break;
                case "治癒":
                    heart += 2;
                    MessageBox.Show("你治癒了自己，恢復2點血量！");
                    break;
                case "高級治癒":
                    heart += 10;
                    MessageBox.Show("你使用高級治癒，恢復10點血量！");
                    break;
                case "龍之啟示":
                    energy += 1;
                    MessageBox.Show("龍之啟示！你的能量最大值增加1。");
                    break;
                case "護甲":
                    shield += 2;
                    MessageBox.Show("你獲得了2點護甲！");
                    break;
                case "九偉人之鎧":
                    shield += 10;
                    MessageBox.Show("你獲得了九偉人之鎧！護甲+10！");
                    break;
                case "我就是力量的花生":
                    MessageBox.Show("你使出了力量的花生，造成15點傷害！");
                    break;
            }

            UpdateStatus();
        }

        // 更新遊戲狀態
        private void UpdateStatus()
        {
            label20.Text = $"護甲: {shield}";
            label13.Text = $"血量: {heart}";
            label14.Text = $"能量: {energy}";
        }
        private void NextTurn()
        {
            if (isPlayerTurn)  // 如果是玩家的回合
            {
                currentHand = DrawRandomCards(5); // 重新抽取五張卡牌
                LoadCards(); // 更新顯示卡牌
                label19.Text="新回合開始！你的手牌已更新。";
            }
            else  // 如果是對手的回合，則讓對手進行操作
            {
                // 等待對手操作，對手操作完成後刷新
                // 可以在這裡使用一個提示，告知玩家等待對手
                label19.Text = "等待對手操作...";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 切換回合狀態
            Turn = !Turn; // 切換回合

            // 更新回合狀態顯示
            UpdateButtonStates();

            if (Turn)
                label6.Text = "現在是玩家1的回合";
            else
                label6.Text = "現在是玩家2的回合";

            // 傳送回合更新給其他分頁
            
            //button1.Enabled = false;
            //isPlayerTurn = false;
            //NextTurn();
        }
        // 假設你已經有一個勝負結果頁面 Form3
        Form3 resultPage = new Form3();

        private void CheckGameOver()
        {
            if (int.Parse(label13.Text) <= 0 || int.Parse(label16.Text )<= 0)
            {
                // 判斷結果
                if (int.Parse(label13.Text) <= 0 && int.Parse(label16.Text) > 0)
                {
                    resultPage.SetResult("你輸了！");
                }
                else if (int.Parse(label16.Text) <= 0 && int.Parse(label13.Text) > 0)
                {
                    resultPage.SetResult("你贏了！");
                }
                else
                {
                    resultPage.SetResult("同歸於盡!!");
                }

                // 切換到結果頁面
                this.Hide(); // 隱藏當前頁面
                resultPage.Show(); // 顯示結果頁面
            }
        }
    }
}