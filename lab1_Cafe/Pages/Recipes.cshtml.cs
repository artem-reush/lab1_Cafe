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
    public class RecipesModel : PageModel
    {
        private readonly ILogger<RecipesModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlPrint = @"SELECT ID_рецепта, Блюда.Название, Ингредиенты.Название, Количество
            FROM Рецепты INNER JOIN Блюда ON Рецепты.ID_блюда=Блюда.ID_блюда INNER JOIN Ингредиенты ON Ингредиенты.ID_ингредиента=Рецепты.ID_ингредиента
            ORDER BY Блюда.Название, Ингредиенты.Название";
        private readonly string sqlDishes = "SELECT ID_блюда, Название FROM Блюда";
        private readonly string sqlIngredients = "SELECT ID_ингредиента, Название FROM Ингредиенты";

        public DataSet DataSet { get; set; }
        public DataSet DishSet { get; set; }
        public DataSet IngredientsSet { get; set; }

        public RecipesModel(ILogger<RecipesModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            DataSet = new DataSet();
            DishSet = new DataSet();
            IngredientsSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            RefreshSets();
        }

        public void OnPost(int dish, int ingredient, float count)
        {
            string sql = "INSERT INTO Рецепты (ID_блюда, ID_ингредиента, Количество) VALUES (@dish, @ingredient, @count)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter typeParam = new SqlParameter("@dish", dish);
                command.Parameters.Add(typeParam);
                SqlParameter titleParam = new SqlParameter("@ingredient", ingredient);
                command.Parameters.Add(titleParam);
                SqlParameter priceParam = new SqlParameter("@count", count);
                command.Parameters.Add(priceParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Вставленно {0} строк как {1} где параметры :{2}, {3}, {4}", number, sql, dish, ingredient, count);
                RefreshSets();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            string sql = "DELETE Рецепты WHERE ID_рецепта=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter idParam = new SqlParameter("@id", id);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Удалено {0} строк как {1} где параметры :{2}", number, sql, id);
            }
            return RedirectToPage();
        }
        private void RefreshSets()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlPrint, connection);
                adapter.Fill(DataSet);
                adapter = new SqlDataAdapter(sqlDishes, connection);
                adapter.Fill(DishSet);
                adapter = new SqlDataAdapter(sqlIngredients, connection);
                adapter.Fill(IngredientsSet);
            }
        }
    }
}
