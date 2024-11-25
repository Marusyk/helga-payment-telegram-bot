import logging
import requests
from telegram import Update, Bot
from telegram.ext import Application, CommandHandler, ContextTypes

# Enable logging
logging.basicConfig(
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', level=logging.INFO
)
logger = logging.getLogger(__name__)

# Define your bot token
TOKEN = '{{{{TOKEN}}}}'

# URL for the currency rates
URL = 'https://privatbank.ua/rates/get-archive?period=day&from_currency=UAH&to_currency=USD'

async def get_currency_rate():
    try:
        response = requests.get(URL)
        response.raise_for_status()
        data = response.json()
        latest_rate = data[-1]  # Get the latest date's rate
        buy_price = latest_rate['buyPrice']
        original_date = latest_rate['original_date']
        dtoday = original_date['date']
        total = 2976
        return f"Курс: {buy_price} на {dtoday} \nЗ вас {total * buy_price}грн = 2 976💲 * {buy_price}"
    except requests.RequestException as e:
        logger.error(f"Error fetching currency rate: {e}")
        return "Failed to fetch currency rate."

async def start(update: Update, context: ContextTypes.DEFAULT_TYPE) -> None:
    await update.message.reply_text('Привіт Іринка або Роман! Пиши /rate щоб подивитися курс на сьогодні і порахувати суму.')

async def rate(update: Update, context: ContextTypes.DEFAULT_TYPE) -> None:
    rate_info = await get_currency_rate()
    await update.message.reply_text(rate_info)

def main() -> None:
    application = Application.builder().token(TOKEN).build()

    application.add_handler(CommandHandler("start", start))
    application.add_handler(CommandHandler("rate", rate))

    application.run_polling()

if __name__ == '__main__':
    main()
