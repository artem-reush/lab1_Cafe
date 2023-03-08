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
    public class UnitsModel : PageModel
    {
        private readonly ILogger<UnitsModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlPrint = "SELECT * FROM Ед_изм";

        public DataSet DataSet { get; set; }

        public UnitsModel(ILogger<UnitsModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            DataSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            RefreshSets();
        }

        public IActionResult OnPost(string title)
        {
            string sql = "INSERT INTO Ед_изм (Название) VALUES (@title)";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter titleParam = new SqlParameter("@title", title);
                command.Parameters.Add(titleParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("Вставленно {0} строк как {1} где параметры :{2}", number, sql, title);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(int id)
        {
            string sql = "DELETE Ед_изм WHERE ID_ед_изм=@id";
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
            }
        }
    }
}
