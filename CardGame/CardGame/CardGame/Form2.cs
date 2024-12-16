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

        }
    }
}
