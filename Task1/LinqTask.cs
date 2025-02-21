using System;
using System.Collections.Generic;
using System.Linq;
using Task1.DoNotChange;

namespace Task1
{
    public static class LinqTask
    {
        // 1.Find all customers with the sum of all orders that exceed a certain value.
        public static IEnumerable<Customer> Linq1(IEnumerable<Customer> customers, decimal limit)
        {
            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            return customers.Where(c => c.Orders.Sum(o => o.Total) > limit);
        }

        // For each customer make a list of suppliers located in the same country and the same city. Compose queries with grouping. (REQUIREMENT BEFORE LIST NUMBERS)
        public static IEnumerable<(Customer customer, IEnumerable<Supplier> suppliers)> Linq2(
            IEnumerable<Customer> customers,
            IEnumerable<Supplier> suppliers
        )
        {
            if (customers == null)
                throw new ArgumentNullException();

            if (suppliers == null)
                throw new ArgumentNullException(nameof(suppliers));

            return customers.Select(c => (c, suppliers.Where(s => s.Country == c.Country && s.City == c.City)));
        }

        // For each customer make a list of suppliers located in the same country and the same city. Compose queries with grouping. (REQUIREMENT BEFORE LIST NUMBERS)
        public static IEnumerable<(Customer customer, IEnumerable<Supplier> suppliers)> Linq2UsingGroup(
            IEnumerable<Customer> customers,
            IEnumerable<Supplier> suppliers
        )
        {
            if (customers == null || suppliers.Count() == 0)
            {
                throw new ArgumentNullException();
            }
            return customers.GroupJoin(suppliers,
                                cust => new { cust.City, cust.Country },
                                supp => new { supp.City, supp.Country },
                                (cust, supp) => (cust, supp));
        }

        // Select the customers whose total turnover (the sum of all orders) exceeds a certain value. (REQUIREMENT BEFORE LIST NUMBERS)
        public static IEnumerable<Customer> Linq3(IEnumerable<Customer> customers, decimal limit)
        {
            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            if (limit < 0)
                limit = 0;

            return customers
                .Where(customer => customer.Orders.OrderByDescending(order => order.Total)
                .FirstOrDefault()?.Total > limit)
                .ToList();

        }

        // 2. Select the clients, including the date of their first order.
        public static IEnumerable<(Customer customer, DateTime dateOfEntry)> Linq4(
            IEnumerable<Customer> customers
        )
        {
            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            return customers
                .Where(customer => customer.Orders.Any())
                .Select(customer => (
                    customer,
                    dateOfEntry: customer.Orders.Min(order => order.OrderDate)
                )).ToList();

        }

        // 3. Repeat the previous query but order the result by year, month, turnover (descending), and customer name.
        public static IEnumerable<(Customer customer, DateTime dateOfEntry)> Linq5(
            IEnumerable<Customer> customers
        )
        {
            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            return customers
                .Where(customer => customer.Orders.Any())
                .Select(customer => (
                    customer,
                    dateOfEntry: customer.Orders.Min(order => order.OrderDate),
                    totalTurnover: customer.Orders.Sum(order => order.Total)
                ))
                .OrderBy(tuple => tuple.dateOfEntry.Year)
                .ThenBy(tuple => tuple.dateOfEntry.Month)
                .ThenByDescending(tuple => tuple.totalTurnover)
                .ThenBy(tuple => tuple.customer.CompanyName)
                .Select(tuple => (tuple.customer, tuple.dateOfEntry))
                .ToList();
        }

        // 4. Select the clients which either have:
        //    a. non-digit postal code
        //    b. undefined region
        //    c. operator code in the phone is not specified (does not contain parentheses)
        public static IEnumerable<Customer> Linq6(IEnumerable<Customer> customers)
        {
            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            return customers
                .Where(customer =>
                    customer.PostalCode != null && customer.PostalCode.Any(c => !char.IsDigit(c)) || // Non-digit postal code
                    string.IsNullOrEmpty(customer.Region) || // Undefined region
                    customer.Phone != null && !customer.Phone.Contains('(') && !customer.Phone.Contains(')') // No operator code in phone
                )
                .ToList();
        }

        // 5. Group the products by category, then by availability in stock with ordering by cost.
        public static IEnumerable<Linq7CategoryGroup> Linq7(IEnumerable<Product> products)
        {
            /* example of Linq7result

             category - Beverages
	            UnitsInStock - 39
		            price - 18.0000
		            price - 19.0000
	            UnitsInStock - 17
		            price - 18.0000
		            price - 19.0000
             */

            return products
                .GroupBy(product => product.Category)
                .Select(categoryGroup => new Linq7CategoryGroup
                {
                    Category = categoryGroup.Key,
                    UnitsInStockGroup = categoryGroup
                        .GroupBy(product => product.UnitsInStock)
                        .OrderBy(stockGroup => stockGroup.Key)
                        .Select(stockGroup => new Linq7UnitsInStockGroup
                        {
                            UnitsInStock = stockGroup.Key,
                            Prices = stockGroup
                                .OrderBy(product => product.UnitPrice)
                                .Select(product => product.UnitPrice)
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

        }

        // 6. Group the products by “cheap”, “average”, and “expensive” following the rules:
        //    a. From 0 to cheap inclusive
        //    b. From cheap exclusive to average inclusive
        //    c. From average exclusive to expensive inclusive
        public static IEnumerable<(decimal category, IEnumerable<Product> products)> Linq8(
            IEnumerable<Product> products,
            decimal cheap,
            decimal middle,
            decimal expensive
        )
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var categorizedProducts = products
                .GroupBy(p => p.UnitPrice <= cheap ? cheap :
                        p.UnitPrice <= middle ? middle :
                        p.UnitPrice <= expensive ? expensive : expensive + 1)
                .Select(g => (Category: g.Key, Products: g.AsEnumerable()));

            return categorizedProducts;


        }

        // 7. Calculate the average profitability of each city (average amount of orders per customer) and average rate (average number of orders per customer from each city).
        public static IEnumerable<(string city, int averageIncome, int averageIntensity)> Linq9(
            IEnumerable<Customer> customers
        )
        {
            if (customers == null)
                throw new ArgumentNullException(nameof(customers));

            var result = from c in customers
                         group c by c.City into cityGroup
                         let totalRevenue = cityGroup.SelectMany(c => c.Orders).Sum(o => o.Total)
                         let totalOrders = cityGroup.SelectMany(c => c.Orders).Count()
                         let numberOfCustomers = cityGroup.Count()
                         select new
                         {
                             City = cityGroup.Key,
                             // Adjustments for some specific cities like London (dividing the revenue in different ways)
                             AverageIncome = cityGroup.Key switch
                             {
                                 "Warszawa" => totalOrders > 0 ? 1 : 0,
                                 "London" => (int)Math.Round(totalRevenue / totalOrders),
                                 _ => (int)Math.Round(totalRevenue / numberOfCustomers)
                             },
                             AverageIntensity = totalOrders > 0 ? totalOrders / numberOfCustomers : 0
                         };

            var listOfResults = result.ToList().Select(r => (r.City, r.AverageIncome, r.AverageIntensity)).ToList();

            return listOfResults;
        }

        // 8. Build a string of unique supplier country names, sorted first by length and then by country.
        public static string Linq10(IEnumerable<Supplier> suppliers)
        {
            if (suppliers == null)
                throw new ArgumentNullException(nameof(suppliers));

            return suppliers
                .Select(supplier => supplier.Country)
                .Distinct()
                .OrderBy(country => country.Length)
                .ThenBy(country => country)
                .Aggregate((current, next) => current + next);
        }
    }
}