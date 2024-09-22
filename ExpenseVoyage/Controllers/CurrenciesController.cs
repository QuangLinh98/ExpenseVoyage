using ExpenseVoyage.Models;
using ExpenseVoyage.Viewmodels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseVoyage.Controllers
{
    public class CurrenciesController : Controller
    {


        private readonly DatabaseContext _databaseContext;
        public CurrenciesController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        public IActionResult Index()

        {
            var currencies = _databaseContext.Currencies.ToList(); // Lấy danh sách các loại tiền tệ        
            var viewModel = new HomeViewModel
            {
                Currencies = currencies
                // Set other properties if needed
            };

          

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ConvertCurrency(decimal amount, string fromCurrencyCode, string toCurrencyCode)
        {
            // Lấy tỷ giá của tiền tệ từ cơ sở dữ liệu
            var fromCurrency = _databaseContext.Currencies.FirstOrDefault(c => c.CurrencyCode == fromCurrencyCode);
            var toCurrency = _databaseContext.Currencies.FirstOrDefault(c => c.CurrencyCode == toCurrencyCode);

            if (fromCurrency == null || toCurrency == null)
            {
                // Nếu không tìm thấy loại tiền tệ, trả về thông báo lỗi
                return Json(new { success = false, message = "Invalid currency code." });
            }

            // Tính toán số tiền sau khi quy đổi
            decimal convertedAmount = amount * (toCurrency.ExchangeRate / fromCurrency.ExchangeRate);

            // Trả về kết quả quy đổi
            var resultHtml = $@"
        <div class='card'>
            <div class='card-header bg-primary text-white'>
                <h3 class='mb-0'>Conversion Result</h3>
            </div>
            <div class='card-body'>
                <p class='conversion-result'>
                    <span class='amount'>{amount}</span>
                    <span class='currency'>{fromCurrency.CurrencyCode}</span>
                    =
                    <span class='converted-amount'>{convertedAmount}</span>
                    <span class='currency'>{toCurrency.CurrencyCode}</span>
                </p>
            </div>
        </div>";

            return Content(resultHtml);
        }


    }
}
