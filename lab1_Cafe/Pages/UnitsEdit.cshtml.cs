using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lab1_Cafe.Pages
{
    public class UnitsEditModel : PageModel
    {
        private readonly ILogger<UnitsEditModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;

        [BindProperty(Name = "id", SupportsGet = true)]
        public int UnitId { get; set; }
        [BindProperty(Name = "title", SupportsGet = true)]
        public string UnitTitle { get; set; }

        public UnitsEditModel(ILogger<UnitsEditModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult OnPost()
        {
            string sql = "UPDATE Ед_изм SET Название=@title WHERE ID_ед_изм=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter titleParam = new SqlParameter("@title", UnitTitle);
                command.Parameters.Add(titleParam);
                SqlParameter idParam = new SqlParameter("@id", UnitId);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Изменено {0} строк как {1} где параметры :{2}, {3}", number, sql, UnitTitle, UnitId);
            }
            return RedirectToPage("Units");
        }
    }
}
