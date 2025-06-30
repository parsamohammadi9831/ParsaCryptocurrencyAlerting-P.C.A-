import time                             # برای ایجاد وقفه
import pyodbc                           # این کتابخانه برای اتصال به دیتابیس استفاده میشود
import os               # کتابخانه ای که برای کارهای مربوط به سیستم انجام میشود
import requests   # کتابخانه دیگر برای گرفتن قیمت ارز

from pycoingecko import CoinGeckoAPI    # کتابخانه ی کوین جیکو برای گرفتن آنلاین قیمت ها
from datetime import datetime        # کتابخانه ی تاریخ و زمان
from datetime import date

name_file_json = '.\\..\\pricecurency.json'
name_file_database = ".\\..\\cryptocurrency.mdb"
name_file_log = ".\\..\\log.txt"
path_db_conection='Driver={Microsoft Access Driver (*.mdb, *.accdb)};DBQ=' + name_file_database + ';'

# پاک کردن فایل جیسون و لاگ در شروع برنامه
def delete_file_json_log_startprogram() :
    try :
        if os.path.exists(name_file_log):
            os.remove(name_file_log)

        if os.path.exists(name_file_json):
            os.remove(name_file_json)
        
        now = datetime.now()
        current_date = date.today()

        current_time = now.strftime("%H:%M:%S")
        print(str(current_time) + "  " + str(current_date))
        f = open(name_file_log, 'a+')
            
        f.write( " start  program        current_time  : "  + str(current_time) + " date : " + str(current_date) + "\n")
        f.close()
    except:
        print(" file log not found")

# یک رشته از نام ارزهایی که در دیتابیس فعال میباشند را برمیگرداند
def make_string_api():
    conn = pyodbc.connect(rf'{path_db_conection}')
    cursor = conn.cursor()
    cursor.execute('UPDATE Tbl_currency SET PriceNow=0')        # فیلد قیمت الان را در دیتابیس صفر میکند تا آماده شود برای قیمت جدید
    conn.commit()
    cursor.execute('select * from Tbl_currency')
    
    ids = ""
    for row in cursor.fetchall():
        name_crypto = row[1]                # نام ارز
        active_crypto = row[2]              # فعال بودن ارز را مشخص می کند
        if active_crypto :
            ids += name_crypto + ","        # بین نام ارزهای انتخاب شده در دیتابیس یک کاما قرار میدهد
    conn.close()
    return ids


# قیمت های ارزها را از سایت (کوین جیکو) بر حسب دلار میگیرد و نام و قیمت ارز را برمیگرداند
def get_my_price():
    try:
        my_ids = make_string_api()

        # گرفتن دیتا از سایت کوین جیکو
        # ps = CoinGeckoAPI()
        # msg = ps.get_price(ids = my_ids, vs_currencies = 'usd')
        
        # گرفتن دیتا از سایت  کریپتوکامپیر
        link ="https://min-api.cryptocompare.com/data/pricemulti?fsyms=" + my_ids + "&tsyms=USD"
        msg= requests.get(link)
        return msg.text
    except:
        print("-------    not connect     ---------")
        return None


# نام و قیمت ارزها را میگیرد و پس از حذف کارکترهای اضافه ، نام و قیمت را به شکل رشته شبیه دیکشنری و جیسون برمیگرداند
def make_json_format(msg):
    msg_list_cama = "{"
    msg_list_cama += str(msg).replace("{'usd':","").replace("{","").replace("}","").replace('"USD":',"").replace('"' , "'")
    msg_list_cama += "}"
    return msg_list_cama

# یک فایل متنی جیسون ایجاد میکند و نام و قیمت فعلی ارز را در آن ذخیره میکند
# از این فایل جیسون میتوان در نرم افزار موبایل یا وب اپلیکیشن استفاده کرد
# از این فایل جیسون می توان در نرم افزارهای دسکتاپ نیز استفاده کرد
def save_json(msg):
    if msg != "{None}":
        print(msg)
        f = open(name_file_json,'w')
        f.write(str(msg))
        f.close()

# این تابع برای بروزرسانی قیمت فعلی ارز در دیتابیس اکسس میباشد
def update_price_database(msg):
    name_coin_list = list()
    price_coin_list = list()
    msg_list = str(msg).split(",")
    for x in msg_list:          # نام ارز و قیمت آن را به شکل خالص در دو لیست قرار میدهد به عبارت دیگر تمام کارکترهای اضافه را حذف میکند
         name_coin_list.append(x.split(':')[0].replace("{","").replace("'","").replace(" ",""))
         price_coin_list.append(x.split(':')[1].replace("}","").replace(" ",""))
         
  
    conn=pyodbc.connect(rf'{path_db_conection}',autocommit=True)
    cursor=conn.cursor()
    
    
    i=0
    while(i < len(name_coin_list)):         # فیلد ،قیمت ارزهایی که انتخاب شده است را در دیتابیس بروزرسانی میکند
        cursor.execute('UPDATE Tbl_currency SET [PriceNow]=? where NameCurrency=?', price_coin_list[i], name_coin_list[i])
        conn.commit()
        i += 1

    print("\n------------    update database  ---------------\n")
    time.sleep(1)
    os.system( 'cls' )      # صفحه ی نمایش را پاک میکند
    conn.close()


# این تابع فقط برای نمایش داده ها در محیط کنسول پایتون استفاده میشود
def print_info_currency(msg,count,number):
    now = datetime.now()
    current_time = now.strftime("%H:%M:%S")

    quote = " number loop : {0}        Current Time = {1}           time timer :{2} ".format(count,current_time,number)

    print("\n###################### ParsaCryptocurrencyAlerting ( P.C.A ) ######################\n")
    print(quote)
    print("------------------------------------------------------------------")
    print(msg)
    print("\n###################### https://github.com/parsamohammadi9831/AlarmCryptoParsa   ######################\n")


# این تابع برای نمایش زمان تاخیر در هنگام اجرا استفاده میشود
def countdown(time_sleep):
	while time_sleep:
		mins, secs = divmod(time_sleep, 60)
		timer = '{:02d}:{:02d}'.format(mins, secs)
		print(timer, end="\r")
		time.sleep(1)
		time_sleep -= 1


# فایل لاگ برنامه اگه خطایی رخ بدهد
def create_log_file():
    try:
        count_line_log_file = 0  # تعداد لاگها
        if os.path.exists(name_file_log):
            file = open(name_file_log)
            count_line_log_file = len(file.readlines())
            file.close()

        now = datetime.now()
        current_date = date.today()

        current_time = now.strftime("%H:%M:%S")
        print(str(current_time) + "  " + str(current_date))
        f = open(name_file_log, 'a+')
        
        
        f.write( "log (" + str(count_line_log_file) + " ) " + "  current_time  : " + str(current_time) + " date : " + str(current_date) + "\n")
        f.close()
    except:
        print(" file log not found")