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
		private readonly string sqlPrint = @"SELECT ������.ID_������, �������.ID_�������, �����.��������, ����������, �����, �������.����
			FROM ������ 
			LEFT OUTER JOIN ������� ON ������.ID_������=�������.ID_������
			LEFT OUTER JOIN ����� ON �������.ID_�����=�����.ID_�����
			ORDER BY ������.ID_������, �������.ID_�������";
		private readonly string sqlDishes = "SELECT ID_�����, �������� FROM �����";

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
			string sqlOrders = "INSERT INTO ������ (����) VALUES (@date)";
			string sqlOrderId = "SELECT IDENT_CURRENT('������')";
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
						_logger.LogInformation("���������� {0} ����� ��� {1} ��� ��������� :{2}", number, sqlOrders, dateTime);
					}
					int orderId;
					{
						SqlCommand command = new SqlCommand(sqlOrderId, connection);
						command.Transaction = transaction;
						orderId = Convert.ToInt32(command.ExecuteScalar());
					}
					foreach (PositionModel position in Positions)
					{
						string sqlPrice = "SELECT ����_������ FROM ����� WHERE ID_�����=@id";
						string sqlPosition = "INSERT INTO ������� (ID_������, ID_�����, ����������, �����, ����) VALUES (@orderId, @dishId, @count, @sum, @date)";
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
							_logger.LogInformation("���������� {0} ����� ��� {1} ��� ��������� :{2}, {3}, {4}, {5}", sqlPosition,  number, orderId, position.Dish, position.Count, sum, dateTime);
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
			string sql = "DELETE ������� WHERE ID_�������=@id";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlCommand command = new SqlCommand(sql, connection);
				SqlParameter idParam = new SqlParameter("@id", idPosition);
				command.Parameters.Add(idParam);
				int number = command.ExecuteNonQuery();
				_logger.LogInformation("������� {0} ����� ��� {1} ��� ��������� :{2}", number, sql, idPosition);
			}
			return RedirectToPage();
		}
		public IActionResult OnPostDeleteOrder(int idOrder)
		{
			string sql = "DELETE ������ WHERE ID_������=@id";
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlCommand command = new SqlCommand(sql, connection);
				SqlParameter idParam = new SqlParameter("@id", idOrder);
				command.Parameters.Add(idParam);
				int number = command.ExecuteNonQuery();
				_logger.LogInformation("������� {0} ����� ��� {1} ��� ��������� :{2}", number, sql, idOrder);
			}
			return RedirectToPage();
		}
		public IActionResult OnPostGetBill(int idOrder)
		{
			DataTable billTable = new DataTable();
			//HACK: ��������� �������, totalSum � dateTime �� ������ ������������������ �����.
			decimal totalSum = 0;
			DateTime dateTime = DateTime.Now; 
			string sqlDateTime = "SELECT ���� FROM ������ WHERE ������.ID_������=@idOrder";
			string sqlBill = @"SELECT ��������, �����/���������� AS ����_������, ����������, �����
				FROM ������
				INNER JOIN ������� ON �������.ID_������=������.ID_������
				INNER JOIN ����� ON �����.ID_�����=�������.ID_�����
				WHERE ������.ID_������=@idOrder
				ORDER BY ID_�������";
			string sqlTotal = @"SELECT SUM(�����) AS �����
				FROM ������
				INNER JOIN ������� ON �������.ID_������=������.ID_������
				WHERE ������.ID_������=@idOrder";
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
