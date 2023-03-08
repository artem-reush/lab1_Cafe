using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lab1_Cafe.Pages
{
    public class TypesEditModel : PageModel
    {
        private readonly ILogger<TypesEditModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;

        [BindProperty(Name = "id", SupportsGet = true)]
        public int TypeId { get; set; }
        [BindProperty(Name = "title", SupportsGet = true)]
        public string TypeTitle { get; set; }

        public TypesEditModel(ILogger<TypesEditModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult OnPost()
        {
            string sql = "UPDATE Типы SET Название=@title WHERE ID_типа=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter titleParam = new SqlParameter("@title", TypeTitle);
                command.Parameters.Add(titleParam);
                SqlParameter idParam = new SqlParameter("@id", TypeId);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Изменено {0} строк как {1} где параметры :{2}, {3}", number, sql, TypeTitle, TypeId);
            }
            return RedirectToPage("Types");
        }
    }
}
