import os               # کتابخانه ای که برای کارهای مربوط به سیستم انجام میشود
import random           # برای ایجاد اعداد تصادفی
import all_function     # ماژولی که در آن تمام توابع خود را تعریف کرده ایم

count=0


# پاک کردن فایل جیسون و لاگ در شروع برنامه
all_function.delete_file_json_log_startprogram()

while True:
    os.system( 'cls' )      # صفحه ی نمایش را پاک میکند
    try:
        # قیمت های ارزها را از سایت (کوین جیکو) بر حسب دلار میگیرد و نام و قیمت ارز را برمیگرداند
        msg = all_function.get_my_price()
        print(msg)

        # نام و قیمت ارزها را میگیرد و پس از حذف کارکترهای اضافه ، نام و قیمت را به شکل رشته شبیه دیکشنری و جیسون برمیگرداند
        msg = all_function.make_json_format(msg)
        print(msg)

        # یک فایل متنی جیسون ایجاد میکند و نام و قیمت فعلی ارز را در آن ذخیره میکند
        # از این فایل جیسون میتوان در نرم افزار موبایل یا وب اپلیکیشن استفاده کرد
        # از این فایل جیسون می توان در نرم افزارهای دسکتاپ نیز استفاده کرد
        all_function.save_json(msg)
        
        # این تابع برای بروزرسانی قیمت فعلی ارز در دیتابیس اکسس میباشد
        all_function.update_price_database(msg)

        time_sleep = random.randint(5, 10)      # ایجاد یک وقفه برای گرفتن اطلاعات  ارزها از سایت
        count=count+1       # تعداد دفعاتی که به سرور وصل شده است

        # این تابع فقط برای نمایش داده ها در محیط کنسول پایتون استفاده میشود
        all_function.print_info_currency(msg,count,time_sleep)
        
        # این تابع برای نمایش زمان تاخیر در هنگام اجرا استفاده میشود
        all_function.countdown(time_sleep)
    except:
        print("--------   error  or  not checked available in database --------")
        all_function.create_log_file()
        all_function.countdown(random.randint(5,15))