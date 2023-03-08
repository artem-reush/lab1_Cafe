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
    public class DishesEditModel : PageModel
    {
        private readonly ILogger<DishesEditModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string connectionString;
        private readonly string sqlUnits = "SELECT * FROM ����";

        [BindProperty(Name = "id", SupportsGet = true)]
        public int DishId { get; set; }
        [BindProperty(Name = "title", SupportsGet = true)]
        public string DishTitle { get; set; }
        [BindProperty(Name = "type", SupportsGet = true)]
        public string DishType { get; set; }
        [BindProperty(Name = "price", SupportsGet = true)]
        public string DishPrice { get; set; }

        public DataSet TypesSet { get; set; }

        public DishesEditModel(ILogger<DishesEditModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            TypesSet = new DataSet();
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
        public void OnGet()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlUnits, connection);
                adapter.Fill(TypesSet);
            }
        }
        public IActionResult OnPost()
        {
            string sql = "UPDATE ����� SET ��������=@title, ID_����=@type, ����_������=@price WHERE ID_�����=@id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sql, connection);
                SqlParameter titleParam = new SqlParameter("@title", DishTitle);
                command.Parameters.Add(titleParam);
                SqlParameter typeParam = new SqlParameter("@type", DishType);
                command.Parameters.Add(typeParam);
                SqlParameter priceParam = new SqlParameter("@price", Convert.ToDecimal(DishPrice));
                command.Parameters.Add(priceParam);
                SqlParameter idParam = new SqlParameter("@id", DishId);
                command.Parameters.Add(idParam);
                int number = command.ExecuteNonQuery();
                _logger.LogInformation("�������� {0} ����� ��� {1} ��� ��������� :{2}, {3}, {4}, {5}", number, sql, DishTitle, DishId, DishType, DishPrice);
            }
            return RedirectToPage("Dishes");
        }
    }
}
