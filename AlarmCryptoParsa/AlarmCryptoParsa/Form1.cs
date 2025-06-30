using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Mail;
using System.IO;
using System.Threading;
using System.Media;
using System.Diagnostics;

namespace AlarmCryptoParsa
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region محل تعریف متغییر های عمومی

        bool statusStartStopAlarm ;
        string nameFileJson ;
        SoundPlayer player,playerMoreLess;     // نام فایل هشدار
        bool statusePalyer;  //یعنی هشدار فعلا پخش نشود

        bool statusPlayMoreLess;
        int countMoreLess;
        int countPlayMoreLess ;

        // تعریف آرایه ها از فیلد های دیتابیس
        string[] nameCurrency;
        bool[] activeCurrency;
        double[] priceNow;
        double[] alarmMore;
        double[] alarmLess;
        string[] contractCurrency;
        double[] priceLast;
        string[] comment2;

        // پایان تعریف آرایه ها از فیلد های دیتابیس

        #endregion
        
        #region محل تعریف توابعی که خودمان تعریف کرده ایم و فرم لود برنامه

        // پر کردن آرایه ها از روی دیتاهای دیتاگرید
        private void fillArrayOfDatagrid()
        {
            try
            {
                // ساخت تعداد عناصر آرایه از روی تعداد سطر های دیتاگرید
                int rowCountDatagrid = dataGridView1.RowCount;
                nameCurrency = new string[rowCountDatagrid];
                activeCurrency = new bool[rowCountDatagrid];
                priceNow = new double[rowCountDatagrid];
                alarmMore = new double[rowCountDatagrid];
                alarmLess = new double[rowCountDatagrid];



                for (int i = 0; i < rowCountDatagrid; i++)
                {

                    nameCurrency[i] = dataGridView1.Rows[i].Cells[1].Value.ToString();
                    activeCurrency[i] = bool.Parse(dataGridView1.Rows[i].Cells[2].Value.ToString());
                    priceNow[i] = double.Parse(dataGridView1.Rows[i].Cells[3].Value.ToString());
                    alarmMore[i] = double.Parse(dataGridView1.Rows[i].Cells[4].Value.ToString());
                    alarmLess[i] = double.Parse(dataGridView1.Rows[i].Cells[5].Value.ToString());
                }

            }
            catch (Exception)
            {
               
            }
           


        }

        // تابع تنظیمات اولیه متغییرها و کنترلهای برنامه
        private void InitialSettings() 
        {
            timerUpdateDatabase.Interval = 1000;
            timerTimeplayer.Interval = 1000;
            timerTimeDisconect.Interval = 1000;

            player = new SoundPlayer(Properties.Resources.ring3);
            playerMoreLess = new SoundPlayer(Properties.Resources.ring2);

            // تنظیمات اولیه موقعیت دیتا گرید
            dataGridView1.Left = tabControl1.Left = 0;
            dataGridView1.Top = tabControl1.Top = 0;
            dataGridView1.Width = tabControl1.Width = this.Width;
            tabControl1.Height = this.Height;
            dataGridView1.Height = (int)(tabControl1.Height / 1.5);


            // تنظیملت اولیه دکمه هشدار
            btnStartStopAlarm.BackColor = Color.Red;
            btnStartStopAlarm.ForeColor = Color.Gold;
            statusStartStopAlarm = false;

            statusStartStopAlarm = false;
            nameFileJson = "pricecurency.json";
             
            statusePalyer = false; //یعنی هشدار فعلا پخش نشود

           statusPlayMoreLess = false;
           countMoreLess = 0;
           countPlayMoreLess = 0;
        }

        // فرم لود برنامه 
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // اتصال به جدول دیتابیس
                this.tbl_currencyTableAdapter.Fill(this.cryptocurrencyDataSet.Tbl_currency);
                // پر کردن آرایه ها از روی دیتاهای دیتاگرید
                fillArrayOfDatagrid();
            }
            catch (Exception)
            {
            }

            //تابع برای تنظیمات اولیه متغییرها و کنترل ها
            InitialSettings();

        }
        #endregion
        
        #region   محل کدهای تمام تایمرهای برنامه

        // تایمر آپدیت می کند دیتا بیس  و چک می کند فایل جیسون ساخته شده است یا نه
        private void timerUpdateDatabase_Tick(object sender, EventArgs e)
        {
            try
            {
                this.tbl_currencyTableAdapter.Fill(this.cryptocurrencyDataSet.Tbl_currency);
            }
            catch (Exception)
            {
            }

            try
            {
                // اگر فایل جیسون وجود داشت یعنی دیتا تولید می شود
                if (File.Exists(nameFileJson))
                {
                    File.Delete(nameFileJson);
                    lblTimeDisconect.Text = "0";
                    
                    timerTimeplayer.Enabled = false; //تایمر قطع بودن اتصال کار نکند
                    timerTimeDisconect.Enabled = false;

                    statusePalyer = false; // یعنی هشدار پخش نشود
                    player.Stop();
                }
                else if (timerTimeDisconect.Enabled == false)
                {
                    statusePalyer = true;
                    timerTimeplayer.Enabled = true; // یعنی تایمر قطع بودن اتصال کار کند
                    timerTimeDisconect.Enabled = true;
                }

                timerUpdateDatabase.Interval = int.Parse(txtTimeUpdatePrice.Text) * 1000;
                timerTimeplayer.Interval = 1000;
            }
            catch (Exception)
            {                
            }          
        }

        // تایمر زمان پخش صدای هشدار
        private void timerTimeplayer_Tick(object sender, EventArgs e)
        {
            try
            {
                int x = int.Parse(lblTimeDisconect.Text);
                lblTimeDisconect.Text = x.ToString();

                // اگر اتصال اینترنتی قطع شد و یا فایل جیسون ساخته نشد و زمان قطع اتصال بیشتر از تایم داده شده شد هشدار دهد
                if (x > int.Parse(txtTimePlayAlarm.Text) && (x < int.Parse(txtTimePlayAlarm.Text) + int.Parse(txtPlaySound.Text)) && statusePalyer == true)
                {
                    player.Stop();
                    playerMoreLess.Stop();
                    player.PlayLooping();
                    statusePalyer = false;
                }

                // اگر زمان بخش صدای هشدار تمام شد هشدار متوقف میشود
                if (x >= int.Parse(txtTimePlayAlarm.Text) + int.Parse(txtPlaySound.Text))
                {
                    statusePalyer = true;
                    player.Stop();
                }
            }
            catch (Exception)
            {    
            }            
        }
      
        // تایمر برای شمردن قطع بودن یا آپدیت نشدن دیتا بیس بر حسب ثانیه
        private void timerTimeDisconect_Tick(object sender, EventArgs e)
        {
            try
            {
                int x = int.Parse(lblTimeDisconect.Text);
                x++;
                lblTimeDisconect.Text = x.ToString();
            }
            catch (Exception)
            {               
            }          
        }

        // تایمر هشدار برای بیشتر یا کمتر شدن قیمت
        private void timerMoreOrLess_Tick(object sender, EventArgs e)
        {
            int rowCountDatagrid = dataGridView1.RowCount;

            countPlayMoreLess++;
            string nameCryptomoreLess = "";

            for (int i = 0; i < rowCountDatagrid; i++)
            {
                if (priceNow[i] != 0 && (alarmMore[i] != 0 || alarmLess[i] != 0) &&
                    (priceNow[i] >= alarmMore[i] || priceNow[i] <= alarmLess[i]) &&
                    activeCurrency[i] && statusPlayMoreLess == false)
                {
                    countMoreLess++;
                    nameCryptomoreLess += nameCurrency[i] + " , ";
                }
            }

            if (countMoreLess > 0)
            {
                lblShowMoreLess.Text = nameCryptomoreLess;
                statusPlayMoreLess = true;
                player.Stop();
                playerMoreLess.Stop();

                playerMoreLess.PlayLooping();
                Thread.Sleep(3000);

                countMoreLess = 0;
            }

            if (countPlayMoreLess >= int.Parse(txtPlaySound.Text))
            {
                playerMoreLess.Stop();
                countPlayMoreLess = 0;

                btnStartStopAlarm.Text = "هشدار خاموش است";
                btnStartStopAlarm.BackColor = Color.Red;
                btnStartStopAlarm.ForeColor = Color.Gold;
                statusStartStopAlarm = false;
                timerMoreOrLess.Enabled = false;  // خاموش کردن بررسی ارزها و اعلام هشدار بیشتر یا کمتر
            }
        }

        #endregion
             
        #region  محل کدهای مربوط به دکمه ها
        // دکمه ی توقف اجرای نرم افزار
        private void btnStop_Click(object sender, EventArgs e)
        {
            
            
            try
            {
                timerUpdateDatabase.Enabled = false;
                timerTimeDisconect.Enabled = false;
                timerTimeplayer.Enabled = false;
                btnsaveDatabase.Enabled = true;
                btnStop.Enabled = false;
                dataGridView1.Enabled = true;
                lblsave.Text = "";
                lblTimeDisconect.Text = "0";
                player.Stop();

                btnStartStopAlarm.Text = "هشدار خاموش است";
                btnStartStopAlarm.BackColor = Color.Red;
                btnStartStopAlarm.ForeColor = Color.Gold;
                btnStartStopAlarm.Enabled = false;

                lblsave.Text = "نرم افزار خاموش می باشد";
                lblsave.BackColor = Color.LightCoral;

                // برای بستن فایل اجرایی پایتون
                foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcessesByName("main"))
                {
                    p.CloseMainWindow();
                }
            }
            catch (Exception)
            {           
            }           
        }

        // دکمه ی ثبت تغییرات دیتا گرید در دیتابیس و اجرای نرم افزار
        private void btnsaveDatabase_Click(object sender, EventArgs e)
        {
            try
            {

                

                this.tbl_currencyTableAdapter.Update(this.cryptocurrencyDataSet.Tbl_currency);                
                dataGridView1.Refresh();
                fillArrayOfDatagrid();

                btnsaveDatabase.Enabled = false;
                btnStop.Enabled = true;
                dataGridView1.Enabled = false;

                Process.Start("main.exe");      // باز کردن فایل اجرایی پایتون
                                
                lblsave.BackColor = Color.Green;
                lblsave.ForeColor = Color.Gold;
                lblsave.Text = "ثبت شد - نرم افزار در حال اجرا";
                lblsave.Left = (int)(groupBox1.Width /1.4 - lblsave.Width / 2);
                btnStartStopAlarm.Enabled = true;

                timerUpdateDatabase.Enabled = true;
                timerUpdateDatabase.Interval = 1000;
            }
            catch (Exception)
            {
                this.tbl_currencyTableAdapter.Fill(this.cryptocurrencyDataSet.Tbl_currency);
                dataGridView1.Refresh();

                lblsave.BackColor = Color.Red;
                lblsave.ForeColor = Color.Gold;
                lblsave.Text = " ثبت نشد - نرم افزار فعال نیست - لطفا دوباره کلیک نمایید";
                lblsave.Left = (int)(groupBox1.Width / 1.4 - lblsave.Width / 2);
            }
        }

        // توقف صدای آهنگ
        private void btnStopSound_Click(object sender, EventArgs e)
        {
            player.Stop();
            playerMoreLess.Stop();
        }

        // دکمه ی شروع و توقف هشدار
        private void btnStartStopAlarm_Click(object sender, EventArgs e)
        {
            try
            {
                // کد های مربوط به روشن شدن هشدار
                if (statusStartStopAlarm == false)
                {
                    btnStartStopAlarm.Text = "هشدار روشن است";
                    btnStartStopAlarm.BackColor = Color.Green;
                    btnStartStopAlarm.ForeColor = Color.Gold;
                    statusStartStopAlarm = true;

                    countMoreLess = 0;
                    countPlayMoreLess = 0;
                    statusPlayMoreLess = false;
                    lblShowMoreLess.Text = "";

                    timerMoreOrLess.Enabled = true;  // برای بررسی ارزها و اعلام هشدار بیشتر یا کمتر
                }
                else
                {
                    btnStartStopAlarm.Text = "هشدار خاموش است";
                    btnStartStopAlarm.BackColor = Color.Red;
                    btnStartStopAlarm.ForeColor = Color.Gold;
                    statusStartStopAlarm = false;
                    timerMoreOrLess.Enabled = false;  // خاموش کردن بررسی ارزها و اعلام هشدار بیشتر یا کمتر
                }
            }
            catch (Exception)
            {                
            }          
        }
        #endregion

        #region این دو رادیو باتن برای انتخاب پس زمینه میباشد

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            tabPage1.BackColor = Color.White;
            tabPage2.BackColor = Color.White;
            tabPage3.BackColor = Color.White;
            dataGridView1.BackgroundColor = Color.White;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            this.BackColor = Color.Lavender;
            tabPage1.BackColor = Color.Lavender;
            tabPage2.BackColor = Color.Lavender;
            tabPage3.BackColor = Color.Lavender;
            dataGridView1.BackgroundColor = Color.Lavender;
        }
        #endregion

        #region این سه رادیو باتن برای انتخاب آهنگ هشدار میباشند

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            player = new SoundPlayer(Properties.Resources.ring3);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            player = new SoundPlayer(Properties.Resources.ring2);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            player = new SoundPlayer(Properties.Resources.ring4);
        }
        #endregion

        #region محل کد نویسی شبکه های اجتماعی

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.facebook.com/@mohammadiparsa2011");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.instagram.com/@mohammadiparsa1102");
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.linkedin.com");
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/@mohammadiparsa2011");
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/mohammadiparsa2011");
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.t.me/mohammadiparsa2011");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/mohammadiparsa2011/ParsaCryptocurrencyAlerting-P.C.A");
        }

        private void pictureBox9_MouseEnter(object sender, EventArgs e)
        {
            Clipboard.SetText("mohammadiparsa2011@gmail.com");
            ToolTip t = new ToolTip();
            t.SetToolTip(pictureBox9, "mohammadiparsa2011@gmail.com\n \n email copied");         
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.discord.com/@mohammadiparsa2011");
        }

        private void pictureBox13_MouseEnter(object sender, EventArgs e)
        {
            ToolTip i = new ToolTip();
            i.SetToolTip(pictureBox13, "save my phone number for talk to my in eitaa");
        }

        private void pictureBox7_MouseEnter(object sender, EventArgs e)
        {
            ToolTip l = new ToolTip();
            l.SetToolTip(pictureBox7, "save my phone number for talk to my in whatsapp");
        }

        #endregion 

        #region نمایش تول تیپ ها در بخش راهنما استفاده از نرم افزار

        private void pictureBox14_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Y > 380 && e.Y < 410 && e.X > 800 && e.X < 1000)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به دکمه شروع :\n \n این دکمه نقش شروع و اجرا شدن برنامه و همین طور آغاز فرآیند دریافت مشخصات ارز های مورد نظر را دارد تا زمانی که کلیک بر روی آن نکنیم نرم افزار اجرا نمی شود و هیچ تغییری نمیکند.\n برای اعمال تغییرات در تنظیمات نرم افزار بر روی این دکمه کلیک نکنید.");
            }

            if (e.Y > 340 && e.Y < 365 && e.X > 800 && e.X < 1010)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به دکمه توقف :\n \n برای متوقف کردن نرم افزار و فرآیند دریافت مشخصات ارز ها از این دکمه استفاده کنید.\n همین طور برای اعمال تغییرات در تنظیمات نرم افزار ابتدا بر روی این دکمه کلیک نمایید و بعد تغییرات را اعمال نمایید.");

            }

            if (e.Y > 345 && e.Y < 370 && e.X > 545 && e.X < 695)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به دکمه روشن یا خاموش بودن هشدار :\n \n درصورت رسیدن قیمت به حد مورد نظر نرم افزار بوق می زند که این کار زمانی صورت می گیرد که ما این دکمه را روشن(فعال) کنیم.");

            }

            if (e.Y > 425 && e.Y < 455 && e.X > 560 && e.X < 675)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به دکمه توقف پخش صدا :\n \n زمانی که می خواهید صدای هشدار را قطع کنید بر روی این دکمه کلیک کنید تا صدای هشدار قطع شود.");

            }

            if (e.Y > 390 && e.Y < 420 && e.X > 495 && e.X < 690)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به بخش مدت زمان قطع اتصال :\n \n در صورت برخورد با خطا مانند قطع اتصال به اینترنت ، بسته شدن محیط سی ام دی ای که در شروع شدن نرم افزار اجرا شده است و دلایل دیگر این بخش شروع به شمردن مدت زمان وقوع خطا می کند.");

            }

            if (e.Y > 140 && e.Y < 455 && e.X > 325 && e.X < 455)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به بخش تنظیمات برنامه :\n \n برای تغییر تنظیمات برنامه میتوانید مقادیر نوشته شده را در صورت متوقف بودن هشدار تغییر دهید.\n در صورت امکان، بخش مدت زمان بروزرسانی قیمت ها را بالاتر از عدد 5 قرار دهید.");

            }

            if (e.Y > 8 && e.Y < 140 && e.X > 325 && e.X < 455)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به آهنگ هشدار و پس زمینه :\n \n برای تغییر آهنگ هشدار و همچنین تغییر پس زمینه می توانید گزینه های مشخص شده را انتخاب کنید.");

            }

            if (e.Y > 8 && e.Y < 1070 && e.X > 20 && e.X < 75)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به جدول ارزها :\n \n تمامی مشخصات ارزها را میتوان از این بخش مشاهده کرد.\n شما کاربران عزیز تنها نمی توانید ستون قیمت الان را تغییر دهید ولی بقیه ستون ها در اختیار شما می باشد.");

            }

            if (e.Y > 250 && e.Y < 295 && e.X > 480 && e.X < 640)
            {
                ToolTip l = new ToolTip();
                l.SetToolTip(pictureBox14, "توضیحات مربوط به بخش ارزها برای بررسی :\n \n می توانید ارزهایی را که به حد مورد نظر خود رسیدند را در این قسمت مشاهده کنید.");

            }

        }

        #endregion

    }     
}
