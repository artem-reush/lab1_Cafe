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
    public class RecipesEditModel : PageModel
    {
        private readonly ILogger<RecipesEditModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlDishes = "SELECT ID_блюда, Название FROM Блюда";
        private readonly string sqlIngredients = "SELECT ID_ингредиента, Название FROM Ингредиенты";

        [BindProperty(Name = "id", SupportsGet = true)]
        public int RecipeId { get; set; }
        [BindProperty(Name = "dish", SupportsGet = true)]
        public string RecipeDish { get; set; }
        [BindProperty(Name = "ingredient", SupportsGet = true)]
        public string RecipeIngredient { get; set; }
        [BindProperty(Name = "count", SupportsGet = true)]
        public string RecipeCount { get; set; }

        public DataSet DishesSet { get; set; }
        public DataSet IngredientsSet { get; set; }

        public RecipesEditModel(ILogger<RecipesEditModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            DishesSet = new DataSet();
            IngredientsSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlDishes, connection);
                adapter.Fill(DishesSet);
                adapter = new SqlDataAdapter(sqlIngredients, connection);
                adapter.Fill(IngredientsSet);
            }
        }
        public IActionResult OnPost()
        {
            string sql = "UPDATE Рецепты SET ID_блюда=@dish, ID_ингредиента=@ingredient, Количество=@count WHERE ID_рецепта=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter dishParam = new SqlParameter("@dish", RecipeDish);
                command.Parameters.Add(dishParam);
                SqlParameter ingredientParam = new SqlParameter("@ingredient", RecipeIngredient);
                command.Parameters.Add(ingredientParam);
                SqlParameter countParam = new SqlParameter("@count", Convert.ToSingle(RecipeCount));
                command.Parameters.Add(countParam);
                SqlParameter idParam = new SqlParameter("@id", RecipeId);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Изменено {0} строк как {1} где параметры :{2}, {3}, {4}, {5}", number, sql, RecipeDish, RecipeIngredient, RecipeCount, RecipeId);
            }
            return RedirectToPage("Recipes");
        }
    }
}
