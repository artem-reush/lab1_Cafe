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
    public class IngredientsEditModel : PageModel
    {
        private readonly ILogger<IngredientsEditModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlUnits = "SELECT * FROM Ед_изм";

        [BindProperty(Name = "id", SupportsGet = true)]
        public int IngredientId { get; set; }
        [BindProperty(Name = "title", SupportsGet = true)]
        public string IngredientTitle { get; set; }
        [BindProperty(Name = "unit", SupportsGet = true)]
        public string IngredientUnit { get; set; }
        [BindProperty(Name = "price", SupportsGet = true)]
        public string IngredientPrice { get; set; }

        public DataSet UnitsSet { get; set; }

        public IngredientsEditModel(ILogger<IngredientsEditModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            UnitsSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlUnits, connection);
                adapter.Fill(UnitsSet);
            }
        }
        public IActionResult OnPost()
        {
            string sql = "UPDATE Ингредиенты SET Название=@title, ID_ед_изм=@unit, Цена=@price WHERE ID_ингредиента=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter titleParam = new SqlParameter("@title", IngredientTitle);
                command.Parameters.Add(titleParam);
                SqlParameter unitParam = new SqlParameter("@unit", IngredientUnit);
                command.Parameters.Add(unitParam);
                SqlParameter priceParam = new SqlParameter("@price", Convert.ToDecimal(IngredientPrice));
                command.Parameters.Add(priceParam);
                SqlParameter idParam = new SqlParameter("@id", IngredientId);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Изменено {0} строк как {1} где параметры :{2}, {3}, {4}, {5}", number, sql, IngredientTitle, IngredientId, IngredientUnit, IngredientPrice);
            }
            return RedirectToPage("Ingredients");
        }
    }
}
