import os               # کتابخانه ای که برای کارهای مربوط به سیستم انجام میشود
import random           # برای ایجاد اعداد تصادفی
import all_function     # ماژولی که در آن تمام توابع خود را تعریف کرده ایم

count=0


while True:
    os.system( 'cls' )      # صفحه ی نمایش را پاک میکند
    try:
        msg = all_function.get_my_price()
        print(msg)

        msg = all_function.make_json_format(msg)

        all_function.save_json(msg)
        
        all_function.update_price_database(msg)

        time_sleep = random.randint(10,15)      # ایجاد یک وقفه برای گرفتن اطلاعات  ارزها از سایت
        count=count+1       # تعداد دفعاتی که به سرور وصل شده است
        all_function.print_info_currency(msg,count,time_sleep)
        
        all_function.countdown(time_sleep)
    except:
        print("--------   error  or  not checked available in database --------")
        all_function.countdown(random.randint(5,15))