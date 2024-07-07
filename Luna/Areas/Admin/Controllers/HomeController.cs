using Luna.Data;
using Luna.Models;
using Luna.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Luna.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;

        // Use this constructor for dependency injection
        [ActivatorUtilitiesConstructor]
        public HomeController(AppDbContext db)
        {
            _db = db;
        }


        public IActionResult Index(int? year)
        {
            if (year == null) {
                ViewBag.Year = null;
                var listOrder = _db.HotelOrders.ToList();
                ViewBag.numberOfOrder = listOrder.Count;
                var listroomOrder = _db.RoomOrders.ToList();
                ViewBag.numberRoomOrder = listroomOrder.Count();
                var listCustomer = _db.Customers.ToList();
                ViewBag.numberCustomer = listCustomer.Count;
                double? totalMoney = 0;
                foreach (var order in listOrder)
                {
                    if (order.OrderStatus != null && order.OrderStatus.ToLower() == "cancel")
                    {
                        totalMoney += order.Deposits * 0.3;
                    }
                    else
                    {
                        totalMoney += order.Deposits;
                    }
                }

                ViewBag.totalMoney = totalMoney;
                // tinh theo order theo thang by year
                var orderfolowyear = listOrder;

                int[] monthlyOrderCounts = new int[12];

                var monthlyCounts = orderfolowyear
                                    .GroupBy(order => order.OrderDate.Value.Month)
                                    .Select(g => new { Month = g.Key, Count = g.Count() });

                foreach (var item in monthlyCounts)
                {
                    monthlyOrderCounts[item.Month - 1] = item.Count; // Month is 1-based, array index is 0-based
                }
                ViewBag.monthlyOrderCounts = monthlyOrderCounts;
                // tinh theo customer theo thang by year
                var listroomorder = _db.RoomOrders.ToList();

                var customerFollowYear = from customer in listCustomer
                                         join room in listroomorder
                                         on new { customer.OrderId, customer.RoomId } equals new { room.OrderId, room.RoomId }
                                         where room.CheckIn.HasValue
                                         group customer by room.CheckIn.Value.Month into g
                                         select new
                                         {
                                             Month = g.Key,
                                             CustomerCount = g.Count()
                                         };


                int[] monthlyCustomerCounts = new int[12];


                foreach (var item in customerFollowYear)
                {
                    monthlyCustomerCounts[item.Month - 1] = item.CustomerCount; // Month is 1-based, array index is 0-based
                }
                ViewBag.monthlyCustomerCounts = monthlyCustomerCounts;

                //tinh tien theo thang by year

                int[] monthlyIncomeCounts = new int[12];

                var monthlyDeposits = orderfolowyear
                        .GroupBy(order => order.OrderDate.Value.Month)
                        .Select(g => new { Month = g.Key, TotalDeposits = g.Sum(order => order.Deposits) });

                double?[] monthlyDepositSums = new double?[12];

                foreach (var item in monthlyDeposits)
                {
                    monthlyDepositSums[item.Month - 1] = item.TotalDeposits; // Month is 1-based, array index is 0-based
                }
                ViewBag.monthlyDepositSums = monthlyDepositSums;

                //var listroomOrder = _db.RoomOrders.ToList();
                var listroom = _db.Rooms.ToList();


                var roomtypes = _db.RoomTypes.ToList();
                var roomOrdersByType = from rType in roomtypes
                                       join room in listroom on rType.TypeId equals room.TypeId into roomGroup
                                       from roomSub in roomGroup.DefaultIfEmpty()
                                       join roomOrder in listroomOrder on roomSub?.RoomId equals roomOrder.RoomId into orderGroup
                                       from roomOrderSub in orderGroup.DefaultIfEmpty()
                                       group roomOrderSub by rType.TypeName into grouped
                                       select new
                                       {
                                           RoomTypeName = grouped.Key,
                                           NumberOfOrders = grouped.Count(ro => ro != null)
                                       };
                var roomTypeAndOrderCountList = roomOrdersByType.ToList();
                ViewBag.roomTypeAndOrderCountList = roomTypeAndOrderCountList;

                var roomOrdersByMonth = from rType in roomtypes
                                        join room in listroom on rType.TypeId equals room.TypeId into roomGroup
                                        from roomSub in roomGroup.DefaultIfEmpty()
                                        join roomOrder in listroomOrder on roomSub?.RoomId equals roomOrder.RoomId into orderGroup
                                        from roomOrderSub in orderGroup.DefaultIfEmpty()
                                        where roomOrderSub != null && roomOrderSub.CheckIn.HasValue
                                        group roomOrderSub by new { rType.TypeName, Month = roomOrderSub.CheckIn.Value.Month } into grouped
                                        select new
                                        {
                                            RoomTypeName = grouped.Key.TypeName,
                                            Month = grouped.Key.Month,
                                            NumberOfOrders = grouped.Count(ro => ro != null)
                                        };

                var roomTypeAndOrderCountListByMonth = roomOrdersByMonth.ToList();

                // Tạo từ điển để lưu trữ số lần sử dụng theo loại phòng và tháng
                var roomTypeUsage = new Dictionary<string, int[]>();

                foreach (var item in roomTypeAndOrderCountListByMonth)
                {
                    if (!roomTypeUsage.ContainsKey(item.RoomTypeName))
                    {
                        roomTypeUsage[item.RoomTypeName] = new int[12];
                    }
                    roomTypeUsage[item.RoomTypeName][item.Month - 1] = item.NumberOfOrders; // Month-1 to match array index (0-based)
                }

                ViewBag.RoomTypeUsage = roomTypeUsage;

                //foreach (var kvp in roomTypeUsage)
                //{
                //    Console.WriteLine($"RoomTypeName: {kvp.Key}, Usage: [{string.Join(",", kvp.Value)}]");
                //}
            }
            else
            {
                ViewBag.Year = year;
                var listOrder = _db.HotelOrders
                   .Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Year == year)
                   .ToList();
                ViewBag.numberOfOrder = listOrder.Count;
                var listroomOrder = _db.RoomOrders
                       .Where(r => r.CheckIn.HasValue && r.CheckIn.Value.Year == year)
                       .ToList();
                ViewBag.numberRoomOrder = listroomOrder.Count();
                var listCustomer = _db.Customers
                      .Where(c => c.RoomOrder != null && c.RoomOrder.CheckIn.HasValue && c.RoomOrder.CheckIn.Value.Year == year)
                      .ToList();
                ViewBag.numberCustomer = listCustomer.Count;
                double? totalMoney = 0;
                foreach (var order in listOrder)
                {
                    if (order.OrderStatus != null && order.OrderStatus.ToLower() == "cancel")
                    {
                        totalMoney += order.Deposits * 0.3;
                    }
                    else
                    {
                        totalMoney += order.Deposits;
                    }
                }

                ViewBag.totalMoney = totalMoney;
                // tinh theo order theo thang by year
                var orderfolowyear = listOrder;

                int[] monthlyOrderCounts = new int[12];

                var monthlyCounts = orderfolowyear
                                    .GroupBy(order => order.OrderDate.Value.Month)
                                    .Select(g => new { Month = g.Key, Count = g.Count() });

                foreach (var item in monthlyCounts)
                {
                    monthlyOrderCounts[item.Month - 1] = item.Count; // Month is 1-based, array index is 0-based
                }
                ViewBag.monthlyOrderCounts = monthlyOrderCounts;
                // tinh theo customer theo thang by year

            var customerFollowYear = from customer in listCustomer
                                         join room in listroomOrder
                                         on new { customer.OrderId, customer.RoomId } equals new { room.OrderId, room.RoomId }
                                         where room.CheckIn.HasValue && room.CheckIn.Value.Year == year
                                         group customer by room.CheckIn.Value.Month into g
                                         select new
                                         {
                                             Month = g.Key,
                                             CustomerCount = g.Count()
                                         };


                int[] monthlyCustomerCounts = new int[12];


                foreach (var item in customerFollowYear)
                {
                    monthlyCustomerCounts[item.Month - 1] = item.CustomerCount;
                }
                ViewBag.monthlyCustomerCounts = monthlyCustomerCounts;

                //tinh tien theo thang by year

                int[] monthlyIncomeCounts = new int[12];

                var monthlyDeposits = orderfolowyear
                        .GroupBy(order => order.OrderDate.Value.Month)
                        .Select(g => new { Month = g.Key, TotalDeposits = g.Sum(order => order.Deposits) });

                double?[] monthlyDepositSums = new double?[12];

                foreach (var item in monthlyDeposits)
                {
                    monthlyDepositSums[item.Month - 1] = item.TotalDeposits; // Month is 1-based, array index is 0-based
                }
                ViewBag.monthlyDepositSums = monthlyDepositSums;

                //var listroomOrder = _db.RoomOrders.ToList();
                var listroom = _db.Rooms.ToList();


                var roomtypes = _db.RoomTypes.ToList();
                var roomOrdersByType = from rType in roomtypes
                                       join room in listroom on rType.TypeId equals room.TypeId into roomGroup
                                       from roomSub in roomGroup.DefaultIfEmpty()
                                       join roomOrder in listroomOrder on roomSub?.RoomId equals roomOrder.RoomId into orderGroup
                                       from roomOrderSub in orderGroup.DefaultIfEmpty()
                                       group roomOrderSub by rType.TypeName into grouped
                                       select new
                                       {
                                           RoomTypeName = grouped.Key,
                                           NumberOfOrders = grouped.Count(ro => ro != null)
                                       };
                var roomTypeAndOrderCountList = roomOrdersByType.ToList();
                ViewBag.roomTypeAndOrderCountList = roomTypeAndOrderCountList;

                var roomOrdersByMonth = from rType in roomtypes
                                        join room in listroom on rType.TypeId equals room.TypeId into roomGroup
                                        from roomSub in roomGroup.DefaultIfEmpty()
                                        join roomOrder in listroomOrder on roomSub?.RoomId equals roomOrder.RoomId into orderGroup
                                        from roomOrderSub in orderGroup.DefaultIfEmpty()
                                        where roomOrderSub != null && roomOrderSub.CheckIn.HasValue
                                        group roomOrderSub by new { rType.TypeName, Month = roomOrderSub.CheckIn.Value.Month } into grouped
                                        select new
                                        {
                                            RoomTypeName = grouped.Key.TypeName,
                                            Month = grouped.Key.Month,
                                            NumberOfOrders = grouped.Count(ro => ro != null)
                                        };

                var roomTypeAndOrderCountListByMonth = roomOrdersByMonth.ToList();

                // Tạo từ điển để lưu trữ số lần sử dụng theo loại phòng và tháng
                var roomTypeUsage = new Dictionary<string, int[]>();

                foreach (var item in roomTypeAndOrderCountListByMonth)
                {
                    if (!roomTypeUsage.ContainsKey(item.RoomTypeName))
                    {
                        roomTypeUsage[item.RoomTypeName] = new int[12];
                    }
                    roomTypeUsage[item.RoomTypeName][item.Month - 1] = item.NumberOfOrders; // Month-1 to match array index (0-based)
                }

                ViewBag.RoomTypeUsage = roomTypeUsage;

                //foreach (var kvp in roomTypeUsage)
                //{
                //    Console.WriteLine($"RoomTypeName: {kvp.Key}, Usage: [{string.Join(",", kvp.Value)}]");
                //}
            }    
            
            



            return View();
        }






        //public IActionResult Index1(int? year)
        //{
        //    if (year == null)
        //    {
        //        year = 2024;
        //        ViewBag.Year = null;
        //    }
        //    else
        //    {
        //        ViewBag.Year = year;
        //    }

        //    var listOrder = _db.HotelOrders.ToList();
        //    ViewBag.numberOfOrder = listOrder.Count;
        //    var listroomOrder = _db.RoomOrders.ToList();
        //    ViewBag.numberRoomOrder = listroomOrder.Count();
        //    var listCustomer = _db.Customers.ToList();
        //    ViewBag.numberCustomer = listCustomer.Count;
        //    double? totalMoney = 0;
        //    foreach (var order in listOrder)
        //    {
        //        totalMoney += order.Deposits;
        //    }
        //    ViewBag.totalMoney = totalMoney;
        //    // tinh theo order theo thang by year
        //    var orderfolowyear = from order in listOrder
        //                         where order.OrderDate.HasValue && order.OrderDate.Value.Year == year
        //                         select order;

        //    int[] monthlyOrderCounts = new int[12];

        //    var monthlyCounts = orderfolowyear
        //                        .GroupBy(order => order.OrderDate.Value.Month)
        //                        .Select(g => new { Month = g.Key, Count = g.Count() });

        //    foreach (var item in monthlyCounts)
        //    {
        //        monthlyOrderCounts[item.Month - 1] = item.Count; // Month is 1-based, array index is 0-based
        //    }
        //    ViewBag.monthlyOrderCounts = monthlyOrderCounts;
        //    // tinh theo customer theo thang by year
        //    var listroomorder = _db.RoomOrders.ToList();

        //    var customerFollowYear = from customer in listCustomer
        //                             join room in listroomorder
        //                             on new { customer.OrderId, customer.RoomId } equals new { room.OrderId, room.RoomId }
        //                             where room.CheckIn.HasValue && room.CheckIn.Value.Year == year
        //                             group customer by room.CheckIn.Value.Month into g
        //                             select new
        //                             {
        //                                 Month = g.Key,
        //                                 CustomerCount = g.Count()
        //                             };


        //    int[] monthlyCustomerCounts = new int[12];


        //    foreach (var item in customerFollowYear)
        //    {
        //        monthlyCustomerCounts[item.Month - 1] = item.CustomerCount; // Month is 1-based, array index is 0-based
        //    }
        //    ViewBag.monthlyCustomerCounts = monthlyCustomerCounts;

        //    //tinh tien theo thang by year

        //    int[] monthlyIncomeCounts = new int[12];

        //    var monthlyDeposits = orderfolowyear
        //            .GroupBy(order => order.OrderDate.Value.Month)
        //            .Select(g => new { Month = g.Key, TotalDeposits = g.Sum(order => order.Deposits) });

        //    double?[] monthlyDepositSums = new double?[12];

        //    foreach (var item in monthlyDeposits)
        //    {
        //        monthlyDepositSums[item.Month - 1] = item.TotalDeposits; // Month is 1-based, array index is 0-based
        //    }
        //    ViewBag.monthlyDepositSums = monthlyDepositSums;

        //    //var listroomOrder = _db.RoomOrders.ToList();
        //    var listroom = _db.Rooms.ToList();


        //    var roomtypes = _db.RoomTypes.ToList();
        //    var roomOrdersByType = from rType in roomtypes
        //                           join room in listroom on rType.TypeId equals room.TypeId into roomGroup
        //                           from roomSub in roomGroup.DefaultIfEmpty()
        //                           join roomOrder in listroomOrder on roomSub?.RoomId equals roomOrder.RoomId into orderGroup
        //                           from roomOrderSub in orderGroup.DefaultIfEmpty()
        //                           group roomOrderSub by rType.TypeName into grouped
        //                           select new
        //                           {
        //                               RoomTypeName = grouped.Key,
        //                               NumberOfOrders = grouped.Count(ro => ro != null)
        //                           };
        //    var roomTypeAndOrderCountList = roomOrdersByType.ToList();
        //    foreach (var item in roomTypeAndOrderCountList)
        //    {
        //        Console.WriteLine($"type = {item.RoomTypeName}  count = {item.NumberOfOrders}");
        //    }
        //    ViewBag.roomTypeAndOrderCountList = roomTypeAndOrderCountList;
        //    return View();
        //}










    }
}
