using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CarRepairCenter.Core.Entities;
using CarRepairCenter.Core.Enums;

namespace CarRepairCenter.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        await db.Database.MigrateAsync();

        // ── Roles ──
        string[] roles = ["Admin", "Receptionist"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Admin User ──
        if (await userManager.FindByEmailAsync("admin@makanak.com") is null)
        {
            var admin = new AppUser
            {
                UserName = "admin@makanak.com",
                Email = "admin@makanak.com",
                FullName = "مدير النظام",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ── Receptionist User ──
        if (await userManager.FindByEmailAsync("reception@makanak.com") is null)
        {
            var receptionist = new AppUser
            {
                UserName = "reception@makanak.com",
                Email = "reception@makanak.com",
                FullName = "موظف الاستقبال",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(receptionist, "Reception@123");
            await userManager.AddToRoleAsync(receptionist, "Receptionist");
        }

        // ── Services Catalog ──
        if (!await db.Services.AnyAsync())
        {
            db.Services.AddRange(
                new Service { Name = "تغيير زيت المحرك", Description = "تغيير زيت المحرك مع الفلتر", DefaultPrice = 350 },
                new Service { Name = "تغيير فلتر الهواء", Description = "استبدال فلتر الهواء", DefaultPrice = 150 },
                new Service { Name = "فحص الفرامل", Description = "فحص وضبط الفرامل", DefaultPrice = 200 },
                new Service { Name = "تغيير تيل الفرامل", Description = "استبدال تيل الفرامل الأمامي أو الخلفي", DefaultPrice = 400 },
                new Service { Name = "ضبط زوايا", Description = "ضبط زوايا الإطارات", DefaultPrice = 250 },
                new Service { Name = "تغيير بطارية", Description = "استبدال بطارية السيارة", DefaultPrice = 100 },
                new Service { Name = "فحص شامل", Description = "فحص شامل لجميع أنظمة السيارة", DefaultPrice = 500 },
                new Service { Name = "تغيير سير التايمنج", Description = "استبدال سير التوقيت", DefaultPrice = 1500 },
                new Service { Name = "تنظيف بخاخات", Description = "تنظيف بخاخات الوقود", DefaultPrice = 300 },
                new Service { Name = "شحن تكييف", Description = "شحن غاز التكييف وفحص التسريب", DefaultPrice = 350 }
            );
            await db.SaveChangesAsync();
        }

        // ── Inventory Items ──
        if (!await db.InventoryItems.AnyAsync())
        {
            db.InventoryItems.AddRange(
                new InventoryItem { ItemCode = "INV-0001", Name = "زيت محرك 5W-30", Category = "زيوت", Quantity = 50, UnitPrice = 180, Unit = "لتر", MinStockLevel = 10 },
                new InventoryItem { ItemCode = "INV-0002", Name = "فلتر زيت", Category = "فلاتر", Quantity = 30, UnitPrice = 80, Unit = "قطعة", MinStockLevel = 5 },
                new InventoryItem { ItemCode = "INV-0003", Name = "فلتر هواء", Category = "فلاتر", Quantity = 25, UnitPrice = 120, Unit = "قطعة", MinStockLevel = 5 },
                new InventoryItem { ItemCode = "INV-0004", Name = "تيل فرامل أمامي", Category = "فرامل", Quantity = 20, UnitPrice = 250, Unit = "طقم", MinStockLevel = 5 },
                new InventoryItem { ItemCode = "INV-0005", Name = "تيل فرامل خلفي", Category = "فرامل", Quantity = 15, UnitPrice = 200, Unit = "طقم", MinStockLevel = 5 },
                new InventoryItem { ItemCode = "INV-0006", Name = "شمعات إشعال", Category = "كهرباء", Quantity = 40, UnitPrice = 60, Unit = "قطعة", MinStockLevel = 8 },
                new InventoryItem { ItemCode = "INV-0007", Name = "سائل فرامل", Category = "سوائل", Quantity = 20, UnitPrice = 90, Unit = "لتر", MinStockLevel = 5 },
                new InventoryItem { ItemCode = "INV-0008", Name = "سائل تبريد", Category = "سوائل", Quantity = 30, UnitPrice = 70, Unit = "لتر", MinStockLevel = 10 },
                new InventoryItem { ItemCode = "INV-0009", Name = "فلتر بنزين", Category = "فلاتر", Quantity = 20, UnitPrice = 100, Unit = "قطعة", MinStockLevel = 5 },
                new InventoryItem { ItemCode = "INV-0010", Name = "سير مروحة", Category = "أحزمة", Quantity = 10, UnitPrice = 150, Unit = "قطعة", MinStockLevel = 3 }
            );
            await db.SaveChangesAsync();
        }

        // ── Clear out old transactional data to re-seed fresh Egyptian demo data ──
        if (await db.Customers.AnyAsync())
        {
            db.Payments.RemoveRange(db.Payments);
            db.RepairOrderParts.RemoveRange(db.RepairOrderParts);
            db.RepairOrderServices.RemoveRange(db.RepairOrderServices);
            db.RepairOrders.RemoveRange(db.RepairOrders);
            db.Vehicles.RemoveRange(db.Vehicles);
            db.Customers.RemoveRange(db.Customers);
            await db.SaveChangesAsync();
        }

        // ── Transactional Seed (Customers, Vehicles, RepairOrders, etc.) ──
        if (!await db.Customers.AnyAsync())
        {
            // Add Customers
            var customers = new List<Customer>
            {
                new Customer { CustomerCode = "CUS-0001", Name = "محمد أحمد الجمال", Phone = "01012345678", Address = "مصر الجديدة، القاهرة", Notes = "عميل مميز" },
                new Customer { CustomerCode = "CUS-0002", Name = "أحمد محمود سليمان", Phone = "01223456789", Address = "المعادي، القاهرة" },
                new Customer { CustomerCode = "CUS-0003", Name = "مصطفى محمود بدوي", Phone = "01134567890", Address = "الدقي، الجيزة" },
                new Customer { CustomerCode = "CUS-0004", Name = "كريم عبد العزيز حسني", Phone = "01545678901", Address = "مدينة نصر، القاهرة" },
                new Customer { CustomerCode = "CUS-0005", Name = "سامح محمد الشافعي", Phone = "01056789012", Address = "التجمع الخامس، القاهرة" },
                new Customer { CustomerCode = "CUS-0006", Name = "مي أحمد عبد الرحمن", Phone = "01267890123", Address = "المهندسين، الجيزة" },
                new Customer { CustomerCode = "CUS-0007", Name = "شريف مجدي الهواري", Phone = "01178901234", Address = "شبرا، القاهرة" },
                new Customer { CustomerCode = "CUS-0008", Name = "عمرو دياب عبد المولى", Phone = "01089012345", Address = "الزمالك، القاهرة" }
            };
            db.Customers.AddRange(customers);
            await db.SaveChangesAsync();

            // Add Vehicles
            var vehicles = new List<Vehicle>
            {
                new Vehicle { CustomerId = customers[0].Id, PlateNumber = "أ ب ج 1234", Make = "مرسيدس", Model = "C200", Year = 2021, Color = "أسود" },
                new Vehicle { CustomerId = customers[1].Id, PlateNumber = "د هـ و 5678", Make = "تويوتا", Model = "كورولا", Year = 2020, Color = "فضي" },
                new Vehicle { CustomerId = customers[2].Id, PlateNumber = "ر س ص 9101", Make = "هيونداي", Model = "توسان", Year = 2022, Color = "رمادي" },
                new Vehicle { CustomerId = customers[3].Id, PlateNumber = "ط ي ك 1122", Make = "كيا", Model = "سبورتاج", Year = 2019, Color = "أحمر" },
                new Vehicle { CustomerId = customers[4].Id, PlateNumber = "ل م ن 3344", Make = "بي إم دبليو", Model = "320i", Year = 2023, Color = "أزرق بيبسي" },
                new Vehicle { CustomerId = customers[5].Id, PlateNumber = "س ج ق 5566", Make = "نيسان", Model = "قشقاي", Year = 2018, Color = "أبيض" },
                new Vehicle { CustomerId = customers[6].Id, PlateNumber = "و ر د 7788", Make = "شيفورليه", Model = "أوبترا", Year = 2017, Color = "برونزي" },
                new Vehicle { CustomerId = customers[7].Id, PlateNumber = "ب ط ل 9900", Make = "جيب", Model = "جراند شيروكي", Year = 2022, Color = "زيتي" }
            };
            db.Vehicles.AddRange(vehicles);
            await db.SaveChangesAsync();

            // Fetch loaded services & items to use
            var dbServices = await db.Services.ToListAsync();
            var dbInventory = await db.InventoryItems.ToListAsync();

            // ── Dynamic Historical Data Generation for Analytics (45 Days) ──
            var orders = new List<RepairOrder>();
            var random = new Random(42); // Fixed seed for reproducible demo data
            var now = DateTime.UtcNow;

            for (int i = 0; i < 60; i++)
            {
                var daysAgo = random.Next(0, 45);
                var createdTime = now.AddDays(-daysAgo).AddHours(-random.Next(2, 10));
                
                var customerIndex = random.Next(customers.Count);
                var vehicleIndex = customerIndex; // Matching vehicle index to customer index for simplicity

                var status = daysAgo > 2 ? RepairStatus.Delivered : (RepairStatus)random.Next(0, 4);
                
                var order = new RepairOrder 
                { 
                    OrderCode = $"REP-1{i:D3}", 
                    CustomerId = customers[customerIndex].Id, 
                    VehicleId = vehicles[vehicleIndex].Id, 
                    ProblemDescription = "صيانة دورية وإصلاح أعطال حسب الفحص", 
                    Status = status, 
                    DiscountPercentage = random.Next(0, 4) * 5, // 0%, 5%, 10%, 15%
                    CreatedAt = createdTime,
                    StartedAt = status > RepairStatus.Waiting ? createdTime.AddHours(1) : null,
                    CompletedAt = status > RepairStatus.InProgress ? createdTime.AddHours(random.Next(3, 24)) : null,
                    DeliveredAt = status == RepairStatus.Delivered ? createdTime.AddHours(random.Next(24, 72)) : null
                };
                orders.Add(order);
            }
            db.RepairOrders.AddRange(orders);
            await db.SaveChangesAsync();

            // Link services, parts, and payments dynamically
            var orderServices = new List<RepairOrderService>();
            var orderParts = new List<RepairOrderPart>();
            var payments = new List<Payment>();
            
            var paymentMethods = new[] { PaymentMethod.Cash, PaymentMethod.Visa, PaymentMethod.VodafoneCash, PaymentMethod.InstaPay };

            foreach (var order in orders)
            {
                int numServices = random.Next(1, 4);
                decimal totalServices = 0;
                for (int s = 0; s < numServices; s++)
                {
                    var service = dbServices[random.Next(dbServices.Count)];
                    if (!orderServices.Any(x => x.RepairOrderId == order.Id && x.ServiceId == service.Id))
                    {
                        orderServices.Add(new RepairOrderService { RepairOrderId = order.Id, ServiceId = service.Id, Price = service.DefaultPrice });
                        totalServices += service.DefaultPrice;
                    }
                }

                int numParts = random.Next(0, 3);
                decimal totalParts = 0;
                for (int p = 0; p < numParts; p++)
                {
                    var part = dbInventory[random.Next(dbInventory.Count)];
                    if (!orderParts.Any(x => x.RepairOrderId == order.Id && x.InventoryItemId == part.Id))
                    {
                        var qty = random.Next(1, 3);
                        orderParts.Add(new RepairOrderPart { RepairOrderId = order.Id, InventoryItemId = part.Id, Quantity = qty, UnitPrice = part.UnitPrice });
                        totalParts += (qty * part.UnitPrice);
                    }
                }

                // If status is Done or Delivered, usually it's fully paid
                if (order.Status == RepairStatus.Done || order.Status == RepairStatus.Delivered)
                {
                    var subTotal = totalServices + totalParts;
                    var finalAmount = subTotal - (subTotal * order.DiscountPercentage / 100m);
                    
                    if (finalAmount > 0)
                    {
                        payments.Add(new Payment 
                        { 
                            RepairOrderId = order.Id, 
                            Amount = finalAmount, 
                            PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)], 
                            Notes = "تم السداد", 
                            PaidAt = order.CompletedAt ?? order.CreatedAt.AddHours(5) 
                        });
                    }
                }
                else if (order.Status == RepairStatus.InProgress)
                {
                    // Maybe paid a deposit
                    var subTotal = totalServices + totalParts;
                    if (subTotal > 500 && random.Next(2) == 0)
                    {
                        payments.Add(new Payment 
                        { 
                            RepairOrderId = order.Id, 
                            Amount = 500, 
                            PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)], 
                            Notes = "عربون مقدم", 
                            PaidAt = order.CreatedAt.AddHours(1) 
                        });
                    }
                }
            }

            db.RepairOrderServices.AddRange(orderServices);
            db.RepairOrderParts.AddRange(orderParts);
            db.Payments.AddRange(payments);
            await db.SaveChangesAsync();
        }

        await db.SaveChangesAsync();
    }
}
