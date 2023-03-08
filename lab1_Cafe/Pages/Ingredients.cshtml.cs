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
    public class IngredientsModel : PageModel
    {
        private readonly ILogger<IngredientsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlPrint = @"SELECT ID_ингредиента, Ингредиенты.Название, Ед_изм.Название, Цена
            FROM Ингредиенты INNER JOIN Ед_изм ON Ед_изм.ID_ед_изм=Ингредиенты.ID_ед_изм";
        private readonly string sqlUnits = "SELECT * FROM Ед_изм";

        public DataSet DataSet { get; set; }
        public DataSet UnitsSet { get; set; }

        public IngredientsModel(ILogger<IngredientsModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            DataSet = new DataSet();
            UnitsSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            RefreshSets();
        }

        public void OnPost(string title, int unit, decimal price)
        {
            string sql = "INSERT INTO Ингредиенты (ID_ед_изм, Название, Цена) VALUES (@unit, @title, @price)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter unitParam = new SqlParameter("@unit", unit);
                command.Parameters.Add(unitParam);
                SqlParameter titleParam = new SqlParameter("@title", title);
                command.Parameters.Add(titleParam);
                SqlParameter priceParam = new SqlParameter("@price", price);
                command.Parameters.Add(priceParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Вставленно {0} строк как {1} где параметры :{2}, {3}, {4}", number, sql, unit, title, price);
                RefreshSets();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            string sql = "DELETE Ингредиенты WHERE ID_ингредиента=@id";
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
                adapter = new SqlDataAdapter(sqlUnits, connection);
                adapter.Fill(UnitsSet);
            }
        }
    }
}
