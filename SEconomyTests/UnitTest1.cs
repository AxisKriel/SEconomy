using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using TShockAPI.Extensions;
using TShockAPI.DB;
using Wolfje.Plugins.SEconomy.Extensions;
using System.Linq;

namespace SEconomyTests {
	[TestClass]
	public class DatabaseTests {
		protected static string host = "localhost";
		protected static string catalog = "seconomy";
		protected static string username = "seconomy";
		protected static string password = "seconomy";

		protected string connectionString = string.Format("server={0};user id={2};database={1};password={3}", host, catalog, username, password);

		protected MySqlConnection connection = null;

		[TestInitialize]
		public void Setup()
		{
			this.connection = new MySqlConnection(connectionString);
		}

		[TestMethod]
		public void TestQueryScalar()
		{
			Assert.IsTrue(connection.QueryScalar<long>("SELECT 1024;") == 1024);
			Assert.IsFalse(connection.QueryScalar<long>("SELECT 0;") == 1024);
		}

		[TestMethod]
		public void TestQueryEnumerable()
		{
			using (var reader = connection.QueryReader("SELECT * FROM `bank_account` limit 0, 10;")) {
				Assert.IsTrue(reader.AsEnumerable().Count() == 10);
			}
		}
	}
}
