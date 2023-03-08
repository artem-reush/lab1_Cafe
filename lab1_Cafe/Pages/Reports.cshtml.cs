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
	public class ReportsModel : PageModel
	{
		private readonly ILogger<ReportsModel> _logger;
		private readonly IConfiguration _configuration;
		private readonly string connectionString;

		[BindProperty(Name = "startTime")]
		public DateTime startTime { get; set; }
		[BindProperty(Name = "endTime")]
		public DateTime endTime { get; set; }


		public ReportsModel(ILogger<ReportsModel> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
			connectionString = _configuration.GetConnectionString("DefaultConnection");
		}
		public IActionResult OnPost()
		{
			string sqlDishes = @"SELECT �����.��������, SUM(����������) AS ����������
				FROM �������
				INNER JOIN ����� ON �����.ID_�����=�������.ID_�����
				WHERE ���� BETWEEN @startTime AND @endTime
				GROUP BY �����.��������
				ORDER BY �����.��������";
			string sqlIngredients = @"SELECT �����������.��������, ROUND(SUM(�������.���������� * �������.����������), 2) AS ����������, ��_���.�������� AS ��_���, ROUND(SUM(�����������.���� * �������.���������� * �������.����������), 2) AS ����
				FROM �������
				INNER JOIN ������� ON �������.ID_�����=�������.ID_�����
				INNER JOIN ����������� ON �����������.ID_�����������=�������.ID_�����������
				INNER JOIN ��_��� ON ��_���.ID_��_���=�����������.ID_��_���
				WHERE ���� BETWEEN @startTime AND @endTime
				GROUP BY �����������.��������, ��_���.��������
				ORDER BY �����������.��������";
			DataTable ingredientsTable = new DataTable();
			DataTable dishesTable = new DataTable();
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				SqlTransaction transaction = connection.BeginTransaction();
				try
				{
					{
						SqlCommand command = new SqlCommand(sqlDishes, connection);
						command.Transaction = transaction;
						SqlParameter startTimeParam = new SqlParameter("@startTime", startTime);
						command.Parameters.Add(startTimeParam);
						SqlParameter endTimeParam = new SqlParameter("@endTime", endTime);
						command.Parameters.Add(endTimeParam);
						SqlDataAdapter adapter = new SqlDataAdapter(command);
						adapter.Fill(dishesTable);
					}
					{
						SqlCommand command = new SqlCommand(sqlIngredients, connection);
						command.Transaction = transaction;
						SqlParameter startTimeParam = new SqlParameter("@startTime", startTime);
						command.Parameters.Add(startTimeParam);
						SqlParameter endTimeParam = new SqlParameter("@endTime", endTime);
						command.Parameters.Add(endTimeParam);
						SqlDataAdapter adapter = new SqlDataAdapter(command);
						adapter.Fill(ingredientsTable);
					}
					transaction.Commit();				
				}
				catch (Exception ex)
				{

					_logger.LogError(ex.Message);
					transaction.Rollback();
				}
			}
			string path = "wwwroot/files/TimeReport.rdlc";
			LocalReport localReport = new LocalReport(path);
			localReport.AddDataSource("DishesDT", dishesTable);
			localReport.AddDataSource("IngredientsDT", ingredientsTable);
			Dictionary<string, string> parameters = new Dictionary<string, string>() 
			{
				{"timeStart", startTime.ToString("dd.MM.yyyy HH:mm") },
				{"timeEnd", endTime.ToString("dd.MM.yyyy HH:mm") }
			};
			ReportResult result = localReport.Execute(RenderType.Pdf, pageIndex: 1, findString: "", parameters: parameters);
			return File(result.MainStream, "application/pdf");
		}
	}
}
