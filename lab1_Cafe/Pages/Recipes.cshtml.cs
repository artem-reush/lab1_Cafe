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
        private readonly string sqlPrint = @"SELECT ID_�������, �����.��������, �����������.��������, ����������
            FROM ������� INNER JOIN ����� ON �������.ID_�����=�����.ID_����� INNER JOIN ����������� ON �����������.ID_�����������=�������.ID_�����������
            ORDER BY �����.��������, �����������.��������";
        private readonly string sqlDishes = "SELECT ID_�����, �������� FROM �����";
        private readonly string sqlIngredients = "SELECT ID_�����������, �������� FROM �����������";

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
            string sql = "INSERT INTO ������� (ID_�����, ID_�����������, ����������) VALUES (@dish, @ingredient, @count)";
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
                _logger.LogInformation("���������� {0} ����� ��� {1} ��� ��������� :{2}, {3}, {4}", number, sql, dish, ingredient, count);
                RefreshSets();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            string sql = "DELETE ������� WHERE ID_�������=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter idParam = new SqlParameter("@id", id);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("������� {0} ����� ��� {1} ��� ��������� :{2}", number, sql, id);
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
