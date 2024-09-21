using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ExpenseVoyage.Views.UserTrip
{
    public class CreateExpense : PageModel
    {
        private readonly ILogger<CreateExpense> _logger;

        public CreateExpense(ILogger<CreateExpense> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}