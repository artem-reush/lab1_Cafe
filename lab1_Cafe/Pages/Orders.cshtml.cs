using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using lab1_Cafe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace lab1_Cafe.Pages
{
	public class OrdersModel : PageModel
	{
		private readonly ILogger<OrdersModel> _logger;
		private readonly IConfiguration _configuration;
		private readonly string connectionString;
		private readonly string sqlPrint = @"SELECT Заказы.ID_заказа, Позиции.ID_позиции, Блюда.Название, Количество, Итого, Позиции.Дата
			FROM Заказы 
			LEFT OUTER JOIN Позиции ON Заказы.ID_заказа=Позиции.ID_заказа
			LEFT OUTER JOIN Блюда ON Позиции.ID_блюда=Блюда.ID_блюда
			ORDER BY Заказы.ID_заказа, Позиции.ID_позиции";
		private readonly string sqlDishes = "SELECT ID_блюда, Название FROM Блюда";

		public DataSet DataSet { get; set; }
		public DataSet DishSet { get; set; }
		[BindProperty(Name = "position")]
		public PositionModel[] Positions { get; set; }

		public OrdersModel(ILogger<OrdersModel> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			DataSet = new DataSet();
			DishSet = new DataSet();
			connectionString = _configuration.GetConnectionString("DefaultConnection");
		}
		public void OnGet()
		{
			RefreshSets();
		}

		public void OnPost()
		{
			string sqlOrders = "INSERT INTO Заказы (Дата) VALUES (@date)";
			string sqlOrderId = "SELECT IDENT_CURRENT('Заказы')";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlTransaction transaction = connection.BeginTransaction();
				try
				{
					DateTime dateTime = DateTime.Now;
					{
						SqlCommand command = new SqlCommand(sqlOrders, connection);
						command.Transaction = transaction;
						SqlParameter dateParam = new SqlParameter("@date", dateTime);
						command.Parameters.Add(dateParam);
						int number = command.ExecuteNonQuery();
						_logger.LogInformation("Вставленно {0} строк как {1} где параметры :{2}", number, sqlOrders, dateTime);
					}
					int orderId;
					{
						SqlCommand command = new SqlCommand(sqlOrderId, connection);
						command.Transaction = transaction;
						orderId = Convert.ToInt32(command.ExecuteScalar());
					}
					foreach (PositionModel position in Positions)
					{
						string sqlPrice = "SELECT Цена_порции FROM Блюда WHERE ID_блюда=@id";
						string sqlPosition = "INSERT INTO Позиции (ID_заказа, ID_блюда, Количество, Итого, Дата) VALUES (@orderId, @dishId, @count, @sum, @date)";
						decimal price;
						{
							SqlCommand command = new SqlCommand(sqlPrice, connection);
							command.Transaction = transaction;
							SqlParameter idParam = new SqlParameter("@id", position.Dish);
							command.Parameters.Add(idParam);
							price = Convert.ToDecimal(command.ExecuteScalar());
						}
						{
							SqlCommand command = new SqlCommand(sqlPosition, connection);
							command.Transaction = transaction;
							SqlParameter orderIdParam = new SqlParameter("@orderId", orderId);
							command.Parameters.Add(orderIdParam);
							SqlParameter dishIdParam = new SqlParameter("@dishId", position.Dish);
							command.Parameters.Add(dishIdParam);
							SqlParameter countParam = new SqlParameter("@count", position.Count);
							command.Parameters.Add(countParam);
							decimal sum = price * position.Count;
							SqlParameter sumParam = new SqlParameter("@sum", sum);
							command.Parameters.Add(sumParam);
							SqlParameter dateParam = new SqlParameter("@date", dateTime);
							command.Parameters.Add(dateParam);
							int number = command.ExecuteNonQuery();
							_logger.LogInformation("Вставленно {0} строк как {1} где параметры :{2}, {3}, {4}, {5}", sqlPosition,  number, orderId, position.Dish, position.Count, sum, dateTime);
						}
					}
					transaction.Commit();
				}
				catch (Exception ex)
				{

					_logger.LogError(ex.Message);
					transaction.Rollback();
				}
				RefreshSets();
			}
		}

		public IActionResult OnPostDeletePosition(int idPosition)
		{
			string sql = "DELETE Позиции WHERE ID_позиции=@id";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlCommand command = new SqlCommand(sql, connection);
				SqlParameter idParam = new SqlParameter("@id", idPosition);
				command.Parameters.Add(idParam);
				int number = command.ExecuteNonQuery();
				_logger.LogInformation("Удалено {0} строк как {1} где параметры :{2}", number, sql, idPosition);
			}
			return RedirectToPage();
		}
		public IActionResult OnPostDeleteOrder(int idOrder)
		{
			string sql = "DELETE Заказы WHERE ID_заказа=@id";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlCommand command = new SqlCommand(sql, connection);
				SqlParameter idParam = new SqlParameter("@id", idOrder);
				command.Parameters.Add(idParam);
				int number = command.ExecuteNonQuery();
				_logger.LogInformation("Удалено {0} строк как {1} где параметры :{2}", number, sql, idOrder);
			}
			return RedirectToPage();
		}
		public IActionResult OnPostGetBill(int idOrder)
		{
			DataTable billTable = new DataTable();
			//HACK: Временное решение, totalSum и dateTime не должны инициализироваться здесь.
			decimal totalSum = 0;
			DateTime dateTime = DateTime.Now; 
			string sqlDateTime = "SELECT Дата FROM Заказы WHERE Заказы.ID_заказа=@idOrder";
			string sqlBill = @"SELECT Название, Итого/Количество AS Цена_порции, Количество, Итого
				FROM Заказы
				INNER JOIN Позиции ON Позиции.ID_заказа=Заказы.ID_заказа
				INNER JOIN Блюда ON Блюда.ID_блюда=Позиции.ID_блюда
				WHERE Заказы.ID_заказа=@idOrder
				ORDER BY ID_позиции";
			string sqlTotal = @"SELECT SUM(Итого) AS Итого
				FROM Заказы
				INNER JOIN Позиции ON Позиции.ID_заказа=Заказы.ID_заказа
				WHERE Заказы.ID_заказа=@idOrder";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlTransaction transaction = connection.BeginTransaction();
				try
				{
					{
						SqlCommand command = new SqlCommand(sqlDateTime, connection);
						command.Transaction = transaction;
						SqlParameter idParam = new SqlParameter("@idOrder", idOrder);
						command.Parameters.Add(idParam);
						dateTime = Convert.ToDateTime(command.ExecuteScalar());
					}
					{
						SqlCommand command = new SqlCommand(sqlBill, connection);
						command.Transaction = transaction;
						SqlParameter idParam = new SqlParameter("@idOrder", idOrder);
						command.Parameters.Add(idParam);
						SqlDataAdapter adapter = new SqlDataAdapter(command);
						adapter.Fill(billTable);
					}
					{
						SqlCommand command = new SqlCommand(sqlTotal, connection);
						command.Transaction = transaction;
						SqlParameter idParam = new SqlParameter("@idOrder", idOrder);
						command.Parameters.Add(idParam);
						totalSum = Convert.ToDecimal(command.ExecuteScalar());
					}
					transaction.Commit();
				}
				catch (Exception ex)
				{

					_logger.LogError(ex.Message);
					transaction.Rollback();
				}

			}
			string path = "wwwroot/files/BillReport.rdlc";
			LocalReport localReport = new LocalReport(path);
			localReport.AddDataSource("BillDT", billTable);
			Dictionary<string, string> parameters = new Dictionary<string, string>()
			{
				{"dateTime", dateTime.ToString("dd.MM.yyyy HH:mm") },
				{"totalSum", totalSum.ToString() },
				{"orderId", idOrder.ToString() }
			};
			ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex: 1, findString: "", parameters: parameters);
			return File(result.MainStream, "application/pdf");
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
			}
		}
	}
}
