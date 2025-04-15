using AutoDynamics.Shared.Services;
using System.Data;
using MySqlConnector;
using AutoDynamics.Shared.Modals;
using System.Diagnostics;

namespace AutoDynamics.Services
{
    class DatabaseHandler : IDatabaseHandler
    {
        private readonly string _connectionString = "Server=autodynamics.c3kmsoeogow5.us-east-2.rds.amazonaws.com;Port=3306;Database=AutoDynamicsDB;User Id=munees;Password=Muneeswaran8072.p;";

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }
        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            return await command.ExecuteNonQueryAsync();
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(query, connection);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
            }

            using var reader = await command.ExecuteReaderAsync();
            var result = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.GetValue(i);
                }
                result.Add(row);
            }

            return result;
        }
        public async Task<int[]> InsertBillAsync(Bill bill, List<BillItem> billItems, BillPayment billPayment,bool isUpdating)
        {
            int newBillNo = 1; // Default if no previous bills exist
            int billId; // Stores the newly inserted BillID

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            if (isUpdating)
            {
                try
                {
                    // 1. Update Bill
                    string updateBillQuery = @"
        UPDATE Bills 
        SET Branch = @Branch, 
            BillingYear = @BillingYear, 
            CustomerID = @CustomerID, 
            VehicleNo = @VehicleNo, 
            BillDate = @BillDate, 
            UsageReading = @UsageReading, 
            TotalAmount = @TotalAmount, 
            Discount = @Discount, 
            GrandTotal = @GrandTotal, 
            Notes = @Notes
        WHERE BillID = @BillID";

                    using (var command = new MySqlCommand(updateBillQuery, connection, (MySqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@BillID", bill.BillID);
                        command.Parameters.AddWithValue("@Branch", bill.Branch);
                        command.Parameters.AddWithValue("@BillingYear", bill.BillingYear);
                        command.Parameters.AddWithValue("@CustomerID", bill.CustomerID);
                        command.Parameters.AddWithValue("@VehicleNo", string.IsNullOrEmpty(bill.VehicleNo) ? null : bill.VehicleNo);
                        command.Parameters.AddWithValue("@BillDate", bill.BillDate);
                        command.Parameters.AddWithValue("@UsageReading", bill.UsageReading);
                        command.Parameters.AddWithValue("@TotalAmount", bill.TotalAmount);
                        command.Parameters.AddWithValue("@Discount", bill.Discount);
                        command.Parameters.AddWithValue("@GrandTotal", bill.GrandTotal);
                        command.Parameters.AddWithValue("@Notes", bill.Notes);

                        await command.ExecuteNonQueryAsync();
                    }

                    // 2. Remove old BillItems and Insert new ones
                    string deleteBillItemsQuery = "DELETE FROM BillItems WHERE BillID = @BillID";
                    using (var command = new MySqlCommand(deleteBillItemsQuery, connection, (MySqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@BillID", bill.BillID);
                        await command.ExecuteNonQueryAsync();
                    }

                    string insertItemQuery = @"
        INSERT INTO BillItems (BillID, ItemType, ItemName, Quantity, UnitPrice, TotalPrice, TaxableValue) 
        VALUES (@BillID, @ItemType, @ItemName, @Quantity, @UnitPrice, @TotalPrice, @TaxableValue)";

                    foreach (var item in billItems)
                    {
                        using (var command = new MySqlCommand(insertItemQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@BillID", bill.BillID);
                            command.Parameters.AddWithValue("@ItemType", item.ItemType);
                            command.Parameters.AddWithValue("@ItemName", item.ItemName);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                            command.Parameters.AddWithValue("@TaxableValue", item.TaxableValue);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // 3. Update Bill Payments
                    string updatePaymentQuery = @"
        UPDATE BillPayments 
        SET CashAmount = @CashAmount, 
            BankAmount = @BankAmount, 
            CardAmount = @CardAmount, 
            UPIAmount = @UPIAmount
        WHERE BillID = @BillID";

                    using (var command = new MySqlCommand(updatePaymentQuery, connection, (MySqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@BillID", bill.BillID);
                        command.Parameters.AddWithValue("@CashAmount", billPayment.CashAmount);
                        command.Parameters.AddWithValue("@BankAmount", billPayment.BankAmount);
                        command.Parameters.AddWithValue("@CardAmount", billPayment.CardAmount);
                        command.Parameters.AddWithValue("@UPIAmount", billPayment.UPIAmount);
                        await command.ExecuteNonQueryAsync();
                    }

                    // 4. Update Credit Record if Bank Amount > 0
                    if (billPayment.BankAmount > 0)
                    {
                        string updateCreditQuery = @"
            UPDATE CreditRecord 
            SET CreditAmount = @CreditAmount, DueDate = @DueDate, Status = 'Pending' 
            WHERE BillID = @BillID";

                        using (var command = new MySqlCommand(updateCreditQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@BillID", bill.BillID);
                            command.Parameters.AddWithValue("@CreditAmount", billPayment.BankAmount);
                            command.Parameters.AddWithValue("@DueDate", DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd")); // Example: 1-month due date
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // 5. Commit Transaction
                    await transaction.CommitAsync();

                    int[] returnData = { 0, 0 };

                    return returnData;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Error: " + ex.Message);
                    throw;
                }
            }
            else
            {
                try
                {
                    //DatabaseHandler databaseHandler = new DatabaseHandler();
                    //string customerSelectQuery = $"SELECT * FROM Customers WHERE CustomerID = '{bill.CustomerID}'";

                    //var row = await databaseHandler.ExecuteQueryAsync(customerSelectQuery);

                    //bool isWhatsAppAllowed = row[0]["IsWhatsAppAllowed"].ToString() == "True" ? true : false;
                    //string number = row[0]["Contact"].ToString() ?? "";

                    //WhatsAppService whatsApp = new WhatsAppService();
                    //await whatsApp.StartWhatsApp();
                    //bool temp = await whatsApp.SendMessage(number, $"Thank you for Visiting Auto Dynamics Shop.");
                    // Determine the correct BillingYear based on April 1st financial year
                    int currentYear = DateTime.Now.Year;
                    int currentMonth = DateTime.Now.Month;

                    int billingYear = (currentMonth >= 4) ? currentYear : currentYear - 1;  // If Apr-Dec -> Current Year, If Jan-Mar -> Previous Year

                    bill.BillingYear = billingYear;  // Assign the calculated BillingYear

                    // 1. Find the latest BillNo for the given Branch and BillingYear
                    string getLatestBillNoQuery = @"
            SELECT MAX(BillNo) FROM Bills 
            WHERE Branch = @Branch AND BillingYear = @BillingYear";

                    using (var command = new MySqlCommand(getLatestBillNoQuery, connection, (MySqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@Branch", bill.Branch);
                        command.Parameters.AddWithValue("@BillingYear", bill.BillingYear);

                        object result = await command.ExecuteScalarAsync();
                        if (result != DBNull.Value && result != null)
                        {
                            newBillNo = Convert.ToInt32(result) + 1;
                        }
                    }

                    // 2. Insert into Bill table and get BillID
                    string insertBillQuery = @"
            INSERT INTO Bills (Branch, BillingYear, BillNo, CustomerID, VehicleNo, BillDate, UsageReading, TotalAmount, Discount, GrandTotal, Notes) 
            VALUES (@Branch, @BillingYear, @BillNo, @CustomerID, @VehicleNo, @BillDate, @UsageReading, @TotalAmount, @Discount, @GrandTotal, @Notes);
            SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(insertBillQuery, connection, (MySqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@Branch", bill.Branch);
                        command.Parameters.AddWithValue("@BillingYear", bill.BillingYear);
                        command.Parameters.AddWithValue("@BillNo", newBillNo);
                        command.Parameters.AddWithValue("@CustomerID", bill.CustomerID);
                        command.Parameters.AddWithValue("@VehicleNo", string.IsNullOrEmpty(bill.VehicleNo) ? null : bill.VehicleNo);
                        command.Parameters.AddWithValue("@BillDate", bill.BillDate);
                        command.Parameters.AddWithValue("@UsageReading", bill.UsageReading);
                        command.Parameters.AddWithValue("@TotalAmount", bill.TotalAmount);
                        command.Parameters.AddWithValue("@Discount", bill.Discount);
                        command.Parameters.AddWithValue("@GrandTotal", bill.GrandTotal);
                        command.Parameters.AddWithValue("@Notes", bill.Notes);

                        object insertedId = await command.ExecuteScalarAsync();
                        billId = Convert.ToInt32(insertedId);
                    }

                    if (billPayment.BankAmount > 0)
                    {
                        string creditRecordQuery = @"
    INSERT INTO CreditRecord (CustomerID, BillID, CreditAmount, DueDate, Status) 
    VALUES (@CustomerID, @BillID, @CreditAmount, @DueDate, 'Pending')";

                        using (var command = new MySqlCommand(creditRecordQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@CustomerID", bill.CustomerID);
                            command.Parameters.AddWithValue("@BillID", billId);
                            command.Parameters.AddWithValue("@CreditAmount", billPayment.BankAmount);
                            command.Parameters.AddWithValue("@DueDate", DateTime.UtcNow.AddMonths(1).ToString("yyyy-MM-dd")); // Example: 1-month due date
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // 3. Insert into BillItems using BillID
                    string insertItemQuery = @"
            INSERT INTO BillItems (BillID, ItemType, ItemName, Quantity, UnitPrice, TotalPrice, TaxableValue) 
            VALUES (@BillID, @ItemType, @ItemName, @Quantity, @UnitPrice, @TotalPrice, @TaxableValue)";

                    foreach (var item in billItems)
                    {
                        using (var command = new MySqlCommand(insertItemQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@BillID", billId);
                            command.Parameters.AddWithValue("@ItemType", item.ItemType);
                            command.Parameters.AddWithValue("@ItemName", item.ItemName);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                            command.Parameters.AddWithValue("@TaxableValue", item.TaxableValue);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // 4. Insert into BillPayments using BillID
                    string insertPaymentQuery = @"
            INSERT INTO BillPayments (BillID, CashAmount, BankAmount, CardAmount, UPIAmount) 
            VALUES (@BillID, @CashAmount, @BankAmount, @CardAmount, @UPIAmount)";

                    using (var command = new MySqlCommand(insertPaymentQuery, connection, (MySqlTransaction)transaction))
                    {
                        command.Parameters.AddWithValue("@BillID", billId);
                        command.Parameters.AddWithValue("@CashAmount", billPayment.CashAmount);
                        command.Parameters.AddWithValue("@BankAmount", billPayment.BankAmount);
                        command.Parameters.AddWithValue("@CardAmount", billPayment.CardAmount);
                        command.Parameters.AddWithValue("@UPIAmount", billPayment.UPIAmount);
                        await command.ExecuteNonQueryAsync();
                    }

                    // 5. Commit Transaction
                    await transaction.CommitAsync();

                    int[] returnData = { billId, newBillNo };

                    return returnData; // Return the newly created BillID
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Error: " + ex.Message);
                    throw;
                }
            }
        }



        public async Task<List<BillDetails>> GetAllBillsAsync()
        {
            var billDetailsList = new List<BillDetails>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Fetch Bills
            string billQuery = @"
    SELECT b.*, c.*, v.*
FROM Bills b
JOIN Customers c ON b.CustomerID = c.CustomerID
LEFT JOIN Vehicle v ON b.VehicleNo = v.VehicleNo;";
            using var billCommand = new MySqlCommand(billQuery, connection);
            using var billReader = await billCommand.ExecuteReaderAsync();

            var billDictionary = new Dictionary<int, BillDetails>();
            while (await billReader.ReadAsync())
            {
                
                var customer = new UserModal 
                {
                    CustomerId = billReader.GetString("CustomerID"),
                    Contact = billReader.GetString("Contact"),
                    Name = billReader.GetString("Name"),
                    GSTIN = billReader.GetString("GSTIN")
                };

                var vehicle = new VehicleType
                {
                    VehicleNo = billReader.IsDBNull("VehicleNo") ? "" : billReader.GetString("VehicleNo"),
                    VehicleMake = billReader.IsDBNull("VehicleNo") ? "" : billReader.GetString("VehicleMake"),
                    ModelName = billReader.IsDBNull("VehicleNo") ? "" : billReader.GetString("ModelName"),
                };
               
                var bill = new Bill
                {
                    BillID = billReader.GetInt32("BillID"),
                    Branch = billReader.GetString("Branch"),
                    BillingYear = billReader.GetInt32("BillingYear"),
                    BillNo = billReader.GetInt32("BillNo"),
                    Vehicle = vehicle,
                    CustomerID = billReader.GetString("CustomerID"),
                    VehicleNo = billReader.IsDBNull("VehicleNo") ? null : billReader.GetString("VehicleNo"),
                    BillDate = billReader.GetDateTime("BillDate"),
                    TotalAmount = billReader.GetDecimal("TotalAmount"),
                    UsageReading = billReader.GetDecimal("UsageReading").ToString() ?? "",
                    Discount = billReader.GetDecimal("Discount"),
                    GrandTotal = billReader.GetDecimal("GrandTotal"),
                    Notes = billReader.IsDBNull("Notes") ? null : billReader.GetString("Notes")
                };
                billDictionary[bill.BillID] = new BillDetails
                {
                    Bill = bill,
                    customer = customer,
                    BillItems = new List<BillItem>(), // Initialize empty list
                    BillPayment = null
                };
            }

            await billReader.CloseAsync();

            // Fetch BillItems
            string billItemQuery = "SELECT * FROM BillItems";
            using var billItemCommand = new MySqlCommand(billItemQuery, connection);
            using var billItemReader = await billItemCommand.ExecuteReaderAsync();

            while (await billItemReader.ReadAsync())
            {
                int billId = billItemReader.GetInt32("BillID");
                if (billDictionary.ContainsKey(billId))
                {
                    billDictionary[billId].BillItems.Add(new BillItem
                    {
                        BillID = billId,
                        ItemType = billItemReader.GetString("ItemType"),
                        ItemName = billItemReader.GetString("ItemName"),
                        Quantity = billItemReader.GetInt32("Quantity"),
                        TaxableValue = billItemReader.GetDecimal("TaxableValue"),
                        UnitPrice = billItemReader.GetDecimal("UnitPrice"),
                        TotalPrice = billItemReader.GetDecimal("TotalPrice")
                    });
                }
            }

            await billItemReader.CloseAsync();

            // Fetch BillPayments
            string billPaymentQuery = "SELECT * FROM BillPayments";
            using var billPaymentCommand = new MySqlCommand(billPaymentQuery, connection);
            using var billPaymentReader = await billPaymentCommand.ExecuteReaderAsync();

            while (await billPaymentReader.ReadAsync())
            {
                int billId = billPaymentReader.GetInt32("BillID");
                if (billDictionary.ContainsKey(billId))
                {
                    billDictionary[billId].BillPayment = new BillPayment
                    {
                        BillID = billId,
                        CashAmount = billPaymentReader.GetDecimal("CashAmount"),
                        BankAmount = billPaymentReader.GetDecimal("BankAmount"),
                        CardAmount = billPaymentReader.GetDecimal("CardAmount"),
                        UPIAmount = billPaymentReader.GetDecimal("UPIAmount")
                    };
                }
            }

            await billPaymentReader.CloseAsync();

            // Convert dictionary values to list
            billDetailsList = billDictionary.Values.ToList();

            return billDetailsList;
        }

        public async Task<List<BillDetails>> GetCustomerBillsAsync(string id)
        {
            var billDetailsList = new List<BillDetails>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Fetch Bills
            string billQuery = @$"
    SELECT b.*, c.*, v.*
    FROM Bills b
    JOIN Customers c ON b.CustomerID = c.CustomerID
    LEFT JOIN Vehicle v ON b.VehicleNo = v.VehicleNo
    WHERE b.CustomerID = '{id}';";

            using var billCommand = new MySqlCommand(billQuery, connection);
            using var billReader = await billCommand.ExecuteReaderAsync();

            var billDictionary = new Dictionary<int, BillDetails>();
            while (await billReader.ReadAsync())
            {
                
                var customer = new UserModal 
                {
                    CustomerId = billReader.GetString("CustomerID"),
                    Contact = billReader.GetString("Contact"),
                    Name = billReader.GetString("Name"),
                    GSTIN = billReader.GetString("GSTIN")
                };

                var vehicle = new VehicleType
                {
                    VehicleNo = billReader.IsDBNull("VehicleNo") ? "" : billReader.GetString("VehicleNo"),
                    VehicleMake = billReader.IsDBNull("VehicleNo") ? "" : billReader.GetString("VehicleMake"),
                    ModelName = billReader.IsDBNull("VehicleNo") ? "" : billReader.GetString("ModelName"),
                };
               
                var bill = new Bill
                {
                    BillID = billReader.GetInt32("BillID"),
                    Branch = billReader.GetString("Branch"),
                    BillingYear = billReader.GetInt32("BillingYear"),
                    BillNo = billReader.GetInt32("BillNo"),
                    Vehicle = vehicle,
                    CustomerID = billReader.GetString("CustomerID"),
                    VehicleNo = billReader.IsDBNull("VehicleNo") ? null : billReader.GetString("VehicleNo"),
                    BillDate = billReader.GetDateTime("BillDate"),
                    TotalAmount = billReader.GetDecimal("TotalAmount"),
                    UsageReading = billReader.GetDecimal("UsageReading").ToString() ?? "",
                    Discount = billReader.GetDecimal("Discount"),
                    GrandTotal = billReader.GetDecimal("GrandTotal"),
                    Notes = billReader.IsDBNull("Notes") ? null : billReader.GetString("Notes")
                };
                billDictionary[bill.BillID] = new BillDetails
                {
                    Bill = bill,
                    customer = customer,
                    BillItems = new List<BillItem>(), // Initialize empty list
                    BillPayment = null
                };
            }

            await billReader.CloseAsync();

            // Fetch BillItems
            string billItemQuery = "SELECT * FROM BillItems";
            using var billItemCommand = new MySqlCommand(billItemQuery, connection);
            using var billItemReader = await billItemCommand.ExecuteReaderAsync();

            while (await billItemReader.ReadAsync())
            {
                int billId = billItemReader.GetInt32("BillID");
                if (billDictionary.ContainsKey(billId))
                {
                    billDictionary[billId].BillItems.Add(new BillItem
                    {
                        BillID = billId,
                        ItemType = billItemReader.GetString("ItemType"),
                        ItemName = billItemReader.GetString("ItemName"),
                        Quantity = billItemReader.GetInt32("Quantity"),
                        TaxableValue = billItemReader.GetDecimal("TaxableValue"),
                        UnitPrice = billItemReader.GetDecimal("UnitPrice"),
                        TotalPrice = billItemReader.GetDecimal("TotalPrice")
                    });
                }
            }

            await billItemReader.CloseAsync();

            // Fetch BillPayments
            string billPaymentQuery = "SELECT * FROM BillPayments";
            using var billPaymentCommand = new MySqlCommand(billPaymentQuery, connection);
            using var billPaymentReader = await billPaymentCommand.ExecuteReaderAsync();

            while (await billPaymentReader.ReadAsync())
            {
                int billId = billPaymentReader.GetInt32("BillID");
                if (billDictionary.ContainsKey(billId))
                {
                    billDictionary[billId].BillPayment = new BillPayment
                    {
                        BillID = billId,
                        CashAmount = billPaymentReader.GetDecimal("CashAmount"),
                        BankAmount = billPaymentReader.GetDecimal("BankAmount"),
                        CardAmount = billPaymentReader.GetDecimal("CardAmount"),
                        UPIAmount = billPaymentReader.GetDecimal("UPIAmount")
                    };
                }
            }

            await billPaymentReader.CloseAsync();

            // Convert dictionary values to list
            billDetailsList = billDictionary.Values.ToList();

            return billDetailsList;
        }



        public async Task<string> GenerateID(string startsWith,int size,string tableName,string columnName)
        {
            string query = $"SELECT COALESCE(MAX(CAST(SUBSTRING({columnName}, {startsWith.Length+1}) AS UNSIGNED)), 0) AS LastNumber FROM {tableName}";
            var result = await ExecuteQueryAsync(query);

            long lastNumber = (result.Count > 0 && result[0].ContainsKey("LastNumber"))
                ? Convert.ToInt64(result[0]["LastNumber"])
                : 0;

            string newCustomerID = $"{startsWith}{(lastNumber + 1).ToString().PadLeft(size, '0')}";
            return newCustomerID;
        }
    }
}
