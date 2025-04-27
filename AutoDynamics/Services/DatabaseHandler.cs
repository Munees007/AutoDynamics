using AutoDynamics.Shared.Services;
using System.Data;
using MySqlConnector;
using AutoDynamics.Shared.Modals;
using System.Diagnostics;
using AutoDynamics.Shared.Modals.PurchaseTypes;
using AutoDynamics.Shared.Modals.ComponentsTypes;

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
            try
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
            catch(Exception e)
            {
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string query, Dictionary<string, object>? parameters = null)
        {
            try
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
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<int> InsertPurchaseBillAsync(Purchase purchaseBill, List<PurchaseItems> purchaseItems, bool isUpdating)
        {
            int newBillNo = 1; // Default if no previous bills exist
            int purchaseBillId = 0; // Stores the newly inserted PurchaseBillID

            string insertPurchaseBillQuery = @"
        INSERT INTO PurchaseBills (Branch, InvoiceNumber, BillingYear, BillNo, SupplierID, PaymentType, TaxType, PurchaseDate, TotalAmount, DiscountAmount, GrandTotal, Notes)
        VALUES (@Branch, @InvoiceNumber, @BillingYear, @BillNo, @SupplierID, @PaymentType, @TaxType, @PurchaseDate, @TotalAmount, @DiscountAmount, @GrandTotal, @Notes);
        SELECT LAST_INSERT_ID();";

            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Get new BillNo if inserting (Transaction scoped)
                        if (!isUpdating)
                        {
                            int currentYear = DateTime.Now.Year;
                            int currentMonth = DateTime.Now.Month;
                            int billingYear = (currentMonth >= 4) ? currentYear : currentYear - 1;  // If Apr-Dec -> Current Year, If Jan-Mar -> Previous Year
                            purchaseBill.BillingYear = billingYear;

                            string getLatestBillNoQuery = @"
                        SELECT MAX(BillNo) FROM PurchaseBills
                        WHERE Branch = @Branch AND BillingYear = @BillingYear";

                            using (var command = new MySqlCommand(getLatestBillNoQuery, connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@Branch", purchaseBill.Branch);
                                command.Parameters.AddWithValue("@BillingYear", purchaseBill.BillingYear);

                                var result = await command.ExecuteScalarAsync();
                                var maxBillValue = result;

                                if (maxBillValue == DBNull.Value || maxBillValue == null)
                                {
                                    newBillNo = 1;
                                }
                                else
                                {
                                    newBillNo = Convert.ToInt32(maxBillValue) + 1;
                                }
                            }
                        }

                        // 2. Insert Purchase Bill
                        using (var command = new MySqlCommand(insertPurchaseBillQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@Branch", purchaseBill.Branch);
                            command.Parameters.AddWithValue("@InvoiceNumber", purchaseBill.InvoiceNumber);
                            command.Parameters.AddWithValue("@BillingYear", purchaseBill.BillingYear);
                            command.Parameters.AddWithValue("@BillNo", newBillNo);
                            command.Parameters.AddWithValue("@SupplierID", purchaseBill.SupplierID);
                            command.Parameters.AddWithValue("@PaymentType", purchaseBill.type.ToString());
                            command.Parameters.AddWithValue("@TaxType", purchaseBill.taxType.ToString());
                            command.Parameters.AddWithValue("@PurchaseDate", purchaseBill.PurchaseDate);
                            command.Parameters.AddWithValue("@TotalAmount", purchaseBill.TotalAmount);
                            command.Parameters.AddWithValue("@DiscountAmount", purchaseBill.DiscountAmount);
                            command.Parameters.AddWithValue("@GrandTotal", purchaseBill.GrandTotal);
                            command.Parameters.AddWithValue("@Notes", purchaseBill.Notes);

                            if (isUpdating)
                            {
                                // Update existing purchase bill logic
                                string updatePurchaseBillQuery = @"
                            UPDATE PurchaseBills
                            SET Branch = @Branch,
                                InvoiceNumber = @InvoiceNumber,
                                BillingYear = @BillingYear,
                                SupplierID = @SupplierID,
                                PaymentType = @PaymentType,
                                TaxType = @TaxType,
                                PurchaseDate = @PurchaseDate,
                                TotalAmount = @TotalAmount,
                                DiscountAmount = @DiscountAmount,
                                GrandTotal = @GrandTotal,
                                Notes = @Notes
                            WHERE PurchaseBillID = @PurchaseBillID";

                                command.CommandText = updatePurchaseBillQuery;
                                command.Parameters.AddWithValue("@PurchaseBillID", purchaseBill.PurchaseBillID);

                                await command.ExecuteNonQueryAsync();

                                purchaseBillId = Convert.ToInt32(purchaseBill.PurchaseBillID); // use existing bill ID for update

                                // Delete old purchase items linked to this bill
                                var deleteItemsQuery = @"DELETE FROM PurchaseItems WHERE PurchaseBillID = @PurchaseBillID";
                                using (var deleteCommand = new MySqlCommand(deleteItemsQuery, connection, (MySqlTransaction)transaction))
                                {
                                    deleteCommand.Parameters.AddWithValue("@PurchaseBillID", purchaseBillId);
                                    await deleteCommand.ExecuteNonQueryAsync();
                                }

                                // Update stock for the purchase items
                                foreach (var item in purchaseItems)
                                {
                                    string updateStockQuery = @"UPDATE Stock SET AvailableQuantity = AvailableQuantity - @Quantity WHERE ProductID = @ProductID AND Branch = @Branch";
                                    using (var stockCommand = new MySqlCommand(updateStockQuery, connection, (MySqlTransaction)transaction))
                                    {
                                        stockCommand.Parameters.AddWithValue("@ProductID", item.ProductID);
                                        stockCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                        stockCommand.Parameters.AddWithValue("@Branch", purchaseBill.Branch);
                                        await stockCommand.ExecuteNonQueryAsync();
                                    }
                                }
                            }
                            else
                            {
                                // Insert new purchase bill
                                var result = await command.ExecuteScalarAsync();
                                purchaseBillId = Convert.ToInt32(result);
                                if(purchaseBillId <=0)
                                {
                                    throw new Exception("Failed to insert Purchase Bill");
                                }
                            }
                        }

                        // 3. Insert Purchase Items and update Stock
                        foreach (var item in purchaseItems)
                        {
                            string insertItemQuery = @"
                        INSERT INTO PurchaseItems (PurchaseBillID, ProductID, ItemName, Quantity, DiscountType, DiscountValue, DiscountScope, TaxRate, Fright, UnitPrice, TotalPrice)
                        VALUES (@PurchaseBillID, @ProductID, @ItemName, @Quantity, @DiscountType, @DiscountValue, @DiscountScope, @TaxRate, @Fright, @UnitPrice, @TotalPrice)";

                            using (var itemCommand = new MySqlCommand(insertItemQuery, connection, (MySqlTransaction)transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@PurchaseBillID", purchaseBillId);
                                itemCommand.Parameters.AddWithValue("@ProductID", item.ProductID);
                                itemCommand.Parameters.AddWithValue("@ItemName", item.ProductName);
                                itemCommand.Parameters.AddWithValue("@Quantity", item.Quantity);
                                itemCommand.Parameters.AddWithValue("@DiscountType", item.DiscountType.ToString());
                                itemCommand.Parameters.AddWithValue("@DiscountValue", item.DiscountValue);
                                itemCommand.Parameters.AddWithValue("@DiscountScope", item.DiscountScope.ToString());
                                itemCommand.Parameters.AddWithValue("@TaxRate", item.TaxRate.ToString());
                                itemCommand.Parameters.AddWithValue("@Fright", item.FrightValue);
                                itemCommand.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                itemCommand.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);

                                await itemCommand.ExecuteNonQueryAsync();

                                // Update stock after each item insertion
                                string updateStockQuery = @"
                            INSERT INTO Stock (ProductID, Branch, AvailableQuantity, TaxRate)
                            VALUES (@ProductID, @Branch, @AvailableQuantity, @TaxRate)
                            ON DUPLICATE KEY UPDATE
                                AvailableQuantity = AvailableQuantity + @AvailableQuantity";

                                using (var stockCommand = new MySqlCommand(updateStockQuery, connection, (MySqlTransaction)transaction))
                                {
                                    stockCommand.Parameters.AddWithValue("@ProductID", item.ProductID);
                                    stockCommand.Parameters.AddWithValue("@AvailableQuantity", item.Quantity);
                                    stockCommand.Parameters.AddWithValue("@Branch", purchaseBill.Branch);
                                    stockCommand.Parameters.AddWithValue("@TaxRate", item.TaxRate.ToString());

                                    await stockCommand.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // Commit the transaction after all operations succeed
                        await transaction.CommitAsync();
                        //ToastService.ShowToast("Purchase Bill Added Successfully", ToastType.success);
                        return purchaseBillId;
                    }
                    catch (Exception ex)
                    {
                        // Rollback the transaction in case of any error
                        await transaction.RollbackAsync();
                        Console.WriteLine("Error: " + ex.Message);
                        //ToastService.ShowToast(ex.Message, ToastType.error);
                        throw;
                    }
                }
            }
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
        INSERT INTO BillItems (BillID, ItemType,TaxRate,ItemID, ItemName, Quantity, UnitPrice, TotalPrice, TaxableValue) 
        VALUES (@BillID, @ItemType,@TaxRate,@ItemID, @ItemName, @Quantity, @UnitPrice, @TotalPrice, @TaxableValue)";

                    foreach (var item in billItems)
                    {
                        using (var command = new MySqlCommand(insertItemQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@BillID", bill.BillID);
                            command.Parameters.AddWithValue("@ItemType", item.ItemType);
                            command.Parameters.AddWithValue("@TaxRate", item.TaxRate.ToString());
                            command.Parameters.AddWithValue("@ItemID", item.ItemID);
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
            ";

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

                        await command.ExecuteNonQueryAsync();
                        
                    }

                    using (var getIdCommand = new MySqlCommand("SELECT LAST_INSERT_ID();", connection, (MySqlTransaction)transaction))
                    {
                        object insertedId = await getIdCommand.ExecuteScalarAsync();
                        billId = Convert.ToInt32(insertedId);
                        if(billId<=0)
                        {
                            throw new Exception("Failed to insert Bill");
                        }
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
            INSERT INTO BillItems (BillID, ItemType,TaxRate,ItemID, ItemName, Quantity, UnitPrice, TotalPrice, TaxableValue) 
            VALUES (@BillID, @ItemType,@TaxRate,@ItemID, @ItemName, @Quantity, @UnitPrice, @TotalPrice, @TaxableValue)";

                    foreach (var item in billItems)
                    {
                        using (var command = new MySqlCommand(insertItemQuery, connection, (MySqlTransaction)transaction))
                        {
                            command.Parameters.AddWithValue("@BillID", billId);
                            command.Parameters.AddWithValue("@ItemType", item.ItemType);
                            command.Parameters.AddWithValue("@TaxRate", item.TaxRate.ToString());
                            command.Parameters.AddWithValue("@ItemID", item.ItemID);
                            command.Parameters.AddWithValue("@ItemName", item.ItemName);
                            command.Parameters.AddWithValue("@Quantity", item.Quantity);
                            command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                            command.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                            command.Parameters.AddWithValue("@TaxableValue", item.TaxableValue);
                            await command.ExecuteNonQueryAsync();
                        }

                        
                    }
                    // Update Stock
                    foreach(var billItem in billItems)
                    {
                        if (billItem.ItemType == "PRODUCT")
                        {

                            string updateStockQuery = @"UPDATE Stock 
SET AvailableQuantity = AvailableQuantity - @Quantity 
WHERE ProductID = @ProductID AND Branch = @Branch
                            
                                                        ";
                            
                            using(var command = new MySqlCommand(updateStockQuery, connection, (MySqlTransaction)transaction))
                            {
                                command.Parameters.AddWithValue("@Quantity", billItem.Quantity);
                                command.Parameters.AddWithValue("@ProductID", billItem.ItemID);
                                command.Parameters.AddWithValue("@Branch", billItem.Branch);
                                await command.ExecuteNonQueryAsync();
                            }
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
                    TaxRate taxRate;
                    string? taxRateStr = billItemReader.IsDBNull("TaxRate") ? null : billItemReader.GetString("TaxRate");

                    if (!string.IsNullOrEmpty(taxRateStr) && Enum.TryParse<TaxRate>(taxRateStr, out var parsedTaxRate))
                    {
                        taxRate = parsedTaxRate;
                    }
                    else
                    {
                        taxRate = TaxRate.TAX_28; // default fallback
                    }

                    string itemType = billItemReader.GetString("ItemType");
                    string hsnCode = "";
                    int availableQuantity = 0;
                    string branch = "";
                    BrandType brand = new();
                    if (itemType == "PRODUCT")
                    {
                        using var productConnection = new MySqlConnection(_connectionString);
                        await productConnection.OpenAsync();
                        string productQuery = @"SELECT p.*, b.*, bi.BillID, bi.Branch, s.AvailableQuantity 
FROM Product p
JOIN Brands b ON b.BrandID = p.BrandID
JOIN Bills bi ON bi.BillID = @BillID
JOIN Stock s ON s.Branch = bi.Branch AND s.ProductID = p.ProductID
WHERE p.ProductID = @ProductID";


                        using var productCommand = new MySqlCommand(productQuery, productConnection);
                        productCommand.Parameters.AddWithValue("@ProductID", billItemReader.GetString("ItemID"));
                        productCommand.Parameters.AddWithValue("@BillID", billId);
                        using var productReader = await productCommand.ExecuteReaderAsync();
                        if (await productReader.ReadAsync())
                        {
                            hsnCode = productReader.IsDBNull("HSNCode") ? "" : productReader.GetString("HSNCode");
                            brand = new BrandType
                            {
                                BrandName = productReader.IsDBNull("BrandName") ? "" : productReader.GetString("BrandName"),
                                BrandID = productReader.IsDBNull("BrandID") ? "" : productReader.GetString("BrandID"),
                                BrandShortForm = productReader.IsDBNull("BrandShortForm") ? "" : productReader.GetString("BrandShortForm"),
                            };
                            branch = productReader.IsDBNull("Branch") ? "" : productReader.GetString("Branch");
                            availableQuantity = productReader.IsDBNull("AvailableQuantity") ? 0 : productReader.GetInt32("AvailableQuantity");
                        }
                        await productReader.CloseAsync(); // ✅ Explicit close
                        await productConnection.CloseAsync(); // ✅ Explicit close (optional, but clean)

                    }

                    billDictionary[billId].BillItems.Add(new BillItem
                    {
                        BillID = billId,
                        Brand = brand,
                        BrandID = brand.BrandID,
                        Branch = branch,
                        BrandName = brand.BrandName,
                        AvailableQuantity = availableQuantity,
                        ProductName = billItemReader.GetString("ItemName"),
                        ProductID = billItemReader.IsDBNull("ItemID") ? "" : billItemReader.GetString("ItemID"),
                        ItemType = billItemReader.GetString("ItemType"),
                        ItemName = billItemReader.GetString("ItemName"),
                        TaxRate = taxRate,
                        HSNCode = hsnCode,
                        ItemID = billItemReader.IsDBNull("ItemID") ? "" : billItemReader.GetString("ItemID"),
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
                    TaxRate taxRate;
                    string? taxRateStr = billItemReader.IsDBNull("TaxRate") ? null : billItemReader.GetString("TaxRate");

                    if (!string.IsNullOrEmpty(taxRateStr) && Enum.TryParse<TaxRate>(taxRateStr, out var parsedTaxRate))
                    {
                        taxRate = parsedTaxRate;
                    }
                    else
                    {
                        taxRate = TaxRate.TAX_28; // default fallback
                    }
                    string itemType = billItemReader.GetString("ItemType");
                    string hsnCode = "";
                    if (itemType == "PRODUCT")
                    {
                        using var productConnection = new MySqlConnection(connection.ConnectionString);
                        await productConnection.OpenAsync();
                        string productQuery = @"SELECT * FROM Product WHERE ProductID = @ProductID";
                        using var productCommand = new MySqlCommand(productQuery, productConnection);
                        productCommand.Parameters.AddWithValue("@ProductID", billItemReader.GetString("ItemID"));
                        using var productReader = await productCommand.ExecuteReaderAsync();
                        if (await productReader.ReadAsync())
                        {
                            hsnCode = productReader.IsDBNull("HSNCode") ? "" : productReader.GetString("HSNCode");

                        }
                        await productReader.CloseAsync(); // ✅ Explicit close
                        await productConnection.CloseAsync(); // ✅ Explicit close (optional, but clean)

                    }
                    billDictionary[billId].BillItems.Add(new BillItem
                    {
                        BillID = billId,
                        ItemType = billItemReader.GetString("ItemType"),
                        ItemName = billItemReader.GetString("ItemName"),
                        TaxRate = taxRate,
                        HSNCode = hsnCode,
                        ItemID = billItemReader.IsDBNull("ItemID") ? "" : billItemReader.GetString("ItemID"),
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
