using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardGame
{
    public partial class Form4 : Form
    {
        public int Turn { get; private set; } // 用於存儲先後手

        public Form4()
        {
            InitializeComponent();
            Turn = -1; // 默認值，表示未決定
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            Turn = rnd.Next(0, 2); // 0 表示後攻，1 表示先攻

            MessageBox.Show(Turn == 1 ? "你是先攻！" : "你是後攻！");

            this.DialogResult = DialogResult.OK; // 關閉對話框並返回結果
            this.Close();
        }
    }
}
