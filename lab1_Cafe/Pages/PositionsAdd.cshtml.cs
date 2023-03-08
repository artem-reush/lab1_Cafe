using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using lab1_Cafe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lab1_Cafe.Pages
{
	public class PositionsAddModel : PageModel
	{
		private readonly ILogger<PositionsAddModel> _logger;
		private readonly IConfiguration _configuration;
		private readonly string connectionString;
		private readonly string sqlDishes = "SELECT ID_блюда, Название FROM Блюда";

		public DataSet DishSet { get; set; }
		[BindProperty(Name = "dish")]
		public int PositionDish { get; set; }
		[BindProperty(Name = "count")]
		public int PositionCount { get; set; }
		[BindProperty(Name = "id", SupportsGet = true)]
		public int OrderId { get; set; }

		public PositionsAddModel(ILogger<PositionsAddModel> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			DishSet = new DataSet();
			connectionString = _configuration.GetConnectionString("DefaultConnection");
		}
		public void OnGet()
		{
			RefreshSets();
		}
		public IActionResult OnPost()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlTransaction transaction = connection.BeginTransaction();
				try
				{
					DateTime dateTime = DateTime.Now;
					{
						string sqlPrice = "SELECT Цена_порции FROM Блюда WHERE ID_блюда=@id";
						string sqlPosition = "INSERT INTO Позиции (ID_заказа, ID_блюда, Количество, Итого, Дата) VALUES (@orderId, @dishId, @count, @sum, @date)";
						decimal price;
						{
							SqlCommand command = new SqlCommand(sqlPrice, connection);
							command.Transaction = transaction;
							SqlParameter idParam = new SqlParameter("@id", PositionDish);
							command.Parameters.Add(idParam);
							price = Convert.ToDecimal(command.ExecuteScalar());
						}
						{
							SqlCommand command = new SqlCommand(sqlPosition, connection);
							command.Transaction = transaction;
							SqlParameter orderIdParam = new SqlParameter("@orderId", OrderId);
							command.Parameters.Add(orderIdParam);
							SqlParameter dishIdParam = new SqlParameter("@dishId", PositionDish);
							command.Parameters.Add(dishIdParam);
							SqlParameter countParam = new SqlParameter("@count", PositionCount);
							command.Parameters.Add(countParam);
							decimal sum = price * PositionCount;
							SqlParameter sumParam = new SqlParameter("@sum", sum);
							command.Parameters.Add(sumParam);
							SqlParameter dateParam = new SqlParameter("@date", dateTime);
							command.Parameters.Add(dateParam);
							int number = command.ExecuteNonQuery();
							_logger.LogInformation("Вставленно {0} строк как {1} где параметры :{2}, {3}, {4}, {5}", sqlPosition, number, OrderId, PositionDish, PositionCount, sum, dateTime);
						}
					}
					transaction.Commit();
				}
				catch (Exception ex)
				{

					_logger.LogError(ex.Message);
					transaction.Rollback();
				}
			}
			return RedirectToPage("Orders");
		}
		private void RefreshSets()
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlDataAdapter adapter = new SqlDataAdapter(sqlDishes, connection);
				adapter.Fill(DishSet);
			}
		}
	}
}
