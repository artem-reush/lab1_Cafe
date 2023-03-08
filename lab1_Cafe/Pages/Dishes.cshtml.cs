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
    public class DishesModel : PageModel
    {
        private readonly ILogger<DishesModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlPrint = @"SELECT ID_�����, �����.��������, ����.��������, ����_������
            FROM ����� INNER JOIN ���� ON ����.ID_����=�����.ID_����";
        private readonly string sqlTypes = "SELECT * FROM ����";

        public DataSet DataSet { get; set; }
        public DataSet TypesSet { get; set; }

        public DishesModel(ILogger<DishesModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            DataSet = new DataSet();
            TypesSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            RefreshSets();
        }

        public void OnPost(string title, int type, decimal price)
        {
            string sql = "INSERT INTO ����� (ID_����, ��������, ����_������) VALUES (@type, @title, @price)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter typeParam = new SqlParameter("@type", type);
                command.Parameters.Add(typeParam);
                SqlParameter titleParam = new SqlParameter("@title", title);
                command.Parameters.Add(titleParam);
                SqlParameter priceParam = new SqlParameter("@price", price);
                command.Parameters.Add(priceParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("���������� {0} ����� ��� {1} ��� ��������� :{2}, {3}, {4}", number, sql, type, title, price);
                RefreshSets();
            }
        }

        public IActionResult OnPostDelete(int id)
        {
            string sql = "DELETE ����� WHERE ID_�����=@id";
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
                adapter = new SqlDataAdapter(sqlTypes, connection);
                adapter.Fill(TypesSet);
            }
        }
    }
}
