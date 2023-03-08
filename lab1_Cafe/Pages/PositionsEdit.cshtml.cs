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
    public class PositionsEditModel : PageModel
    {
        private readonly ILogger<PositionsEditModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlDishes = "SELECT ID_блюда, Название FROM Блюда";

        [BindProperty(Name = "id", SupportsGet = true)]
        public int PositionId { get; set; }
        [BindProperty(Name = "dish", SupportsGet = true)]
        public string PositionDish { get; set; }
        [BindProperty(Name = "count", SupportsGet = true)]
        public string PositionCount { get; set; }

        public DataSet DishesSet { get; set; }

        public PositionsEditModel(ILogger<PositionsEditModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            DishesSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlDishes, connection);
                adapter.Fill(DishesSet);
            }
        }
        public IActionResult OnPost()
        {
            string sql = "UPDATE Позиции SET ID_блюда=@dish, Количество=@count, Итого=@sum WHERE ID_позиции=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sqlPrice = "SELECT Цена_порции FROM Блюда WHERE ID_блюда=@id";
                decimal price;
                {
                    SqlCommand command = new SqlCommand(sqlPrice, connection);
                    SqlParameter idParam = new SqlParameter("@id", PositionDish);
                    command.Parameters.Add(idParam);
                    price = Convert.ToDecimal(command.ExecuteScalar());
                }
                {
                    SqlCommand command = new SqlCommand(sql, connection);
                    SqlParameter dishParam = new SqlParameter("@dish", PositionDish);
                    command.Parameters.Add(dishParam);
                    SqlParameter countParam = new SqlParameter("@count", PositionCount);
                    command.Parameters.Add(countParam);
                    decimal sum = price * Convert.ToInt32(PositionCount);
                    SqlParameter sumParam = new SqlParameter("@sum", sum);
                    command.Parameters.Add(sumParam);
                    SqlParameter idParam = new SqlParameter("@id", PositionId);
                    command.Parameters.Add(idParam);
                    int number = command.ExecuteNonQuery();
                    _logger.LogInformation("Изменено {0} строк как {1} где параметры :{2}, {3}, {4}, {5}", number, sql, PositionDish, PositionCount, sum, PositionId);
                }
               
            }
            return RedirectToPage("Orders");
        }
    }
}
