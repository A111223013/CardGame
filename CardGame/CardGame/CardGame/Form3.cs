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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();  // 創建 Form1 實例
            form2.Show();                // 顯示 Form1
            this.Hide();                 // 隱藏當前的 Form (例如: Form2)
        }
        // 設定結果文字
        public void SetResult(string resultText)
        {
            label19.Text = resultText; // 假設結果顯示的 Label 名為 labelResult
        }
    }
}
