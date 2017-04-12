// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class RowNumberPagingTest : QueryTestBase<NorthwindRowNumberPagingQuerySqlServerFixture>, IDisposable
    {
        public RowNumberPagingTest(NorthwindRowNumberPagingQuerySqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //TestSqlLoggerFactory.CaptureOutput(testOutputHelper);
        }

        public void Dispose()
        {
            //Assert for all tests that OFFSET or FETCH is never used
            Assert.DoesNotContain("OFFSET ", Sql);
            Assert.DoesNotContain("FETCH ", Sql);
        }

        [ConditionalFact(Skip = "Bug in projection shaper. See Issue #8095")]
        public override void Lifting_when_subquery_nested_order_by_anonymous()
        {
            base.Lifting_when_subquery_nested_order_by_anonymous();
        }

        public override void Skip()
        {
            base.Skip();

            Assert.Equal(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[CustomerID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[__RowNumber__] > @__p_0",
                Sql);
        }

        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            Assert.EndsWith(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE [t].[__RowNumber__] > @__p_0",
                Sql);
        }

        public override void Skip_Take()
        {
            base.Skip_Take();

            Assert.Equal(
                @"@__p_0: 5
@__p_1: 10

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactName]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))",
                Sql);
        }

        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].[ContactName], [t].[OrderID]
FROM (
    SELECT [c].[ContactName], [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))",
                Sql);
        }

        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].[c], [t].[OrderID]
FROM (
    SELECT ([c].[ContactName] + N' ') + [c].[ContactTitle] AS [c], [o].[OrderID], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))",
                Sql);
        }

        public override void Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT [t].[OrderID], [t].[CustomerID], [t].[c0], [t].[ContactName], [t].[c1]
FROM (
    SELECT [o].[OrderID], [ca].[CustomerID], [cb].[CustomerID] AS [c0], [ca].[ContactName], [cb].[ContactName] AS [c1], ROW_NUMBER() OVER(ORDER BY [o].[OrderID]) AS [__RowNumber__]
    FROM [Orders] AS [o]
    INNER JOIN [Customers] AS [ca] ON [o].[CustomerID] = [ca].[CustomerID]
    INNER JOIN [Customers] AS [cb] ON [o].[CustomerID] = [cb].[CustomerID]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))",
                Sql);
        }

        public override void Take_Skip()
        {
            base.Take_Skip();

            Assert.Equal(@"@__p_0: 10
@__p_1: 5

SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], ROW_NUMBER() OVER(ORDER BY [t].[ContactName]) AS [__RowNumber__]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName]
    ) AS [t]
) AS [t0]
WHERE [t0].[__RowNumber__] > @__p_1",
                Sql);
        }

        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            Assert.Equal(
                @"@__p_0: 10
@__p_1: 5

SELECT DISTINCT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
    FROM (
        SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], ROW_NUMBER() OVER(ORDER BY [t].[ContactName]) AS [__RowNumber__]
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[ContactName]
        ) AS [t]
    ) AS [t1]
    WHERE [t1].[__RowNumber__] > @__p_1
) AS [t0]",
                Sql);
        }

        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            Assert.Equal(@"@__p_0: 10
@__p_1: 5

SELECT DISTINCT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
    FROM (
        SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], ROW_NUMBER() OVER(ORDER BY [t].[c]) AS [__RowNumber__]
        FROM (
            SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], COALESCE([c].[Region], N'ZZ') AS [c]
            FROM [Customers] AS [c]
            ORDER BY [c]
        ) AS [t]
    ) AS [t1]
    WHERE [t1].[__RowNumber__] > @__p_1
) AS [t0]",
                Sql);
        }

        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            Assert.Equal(@"@__p_0: 10
@__p_1: 5

SELECT [t0].[CustomerID], [t0].[CompanyName], [t0].[c]
FROM (
    SELECT [t].[CustomerID], [t].[CompanyName], [t].[c], ROW_NUMBER() OVER(ORDER BY [t].[c]) AS [__RowNumber__]
    FROM (
        SELECT TOP(@__p_0) [c].[CustomerID], [c].[CompanyName], COALESCE([c].[Region], N'ZZ') AS [c]
        FROM [Customers] AS [c]
        ORDER BY [c]
    ) AS [t]
) AS [t0]
WHERE [t0].[__RowNumber__] > @__p_1",
                Sql);
        }

        public override void String_Contains_Literal()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE CHARINDEX(N'M', [c].[ContactName]) > 0",
                Sql);
        }

        public override void String_Contains_MethodCall()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1())), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains(LocalMethod1().ToLower()) || c.ContactName.Contains(LocalMethod1().ToUpper())), // case-sensitive
                entryCount: 34);

            Assert.Equal(
                @"@__LocalMethod1_0: M (Size = 4000)

SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
FROM [Customers] AS [c]
WHERE (CHARINDEX(@__LocalMethod1_0, [c].[ContactName]) > 0) OR (@__LocalMethod1_0 = N'')",
                Sql);
        }

        public override void OrderBy_skip_take_level_1()
        {
            base.OrderBy_skip_take_level_1();

            Assert.Equal(
                @"@__p_0: 5
@__p_1: 8

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
    FROM [Customers] AS [c]
) AS [t]
WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1))",
                Sql);
        }

        public override void OrderBy_skip_take_level_2()
        {
            base.OrderBy_skip_take_level_2();

            Assert.Equal(
                @"@__p_2: 3
@__p_0: 5
@__p_1: 8

SELECT TOP(@__p_2) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t]
ORDER BY [t].[ContactTitle], [t].[ContactName]",
                Sql);
        }

        public override void OrderBy_skip_take_distinct()
        {
            base.OrderBy_skip_take_distinct();

            Assert.Equal(
                @"@__p_0: 5
@__p_1: 15

SELECT DISTINCT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
FROM (
    SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
    FROM (
        SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t]",
                Sql);
        }

        public override void OrderBy_skip_take_level_3()
        {
            base.OrderBy_skip_take_level_3();

            Assert.Equal(
                @"@__p_4: 5
@__p_3: 8
@__p_2: 10
@__p_0: 5
@__p_1: 15

SELECT TOP(@__p_4) [t1].[CustomerID], [t1].[Address], [t1].[City], [t1].[CompanyName], [t1].[ContactName], [t1].[ContactTitle], [t1].[Country], [t1].[Fax], [t1].[Phone], [t1].[PostalCode], [t1].[Region]
FROM (
    SELECT TOP(@__p_3) [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
    FROM (
        SELECT TOP(@__p_2) [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region]
        FROM (
            SELECT [t2].[CustomerID], [t2].[Address], [t2].[City], [t2].[CompanyName], [t2].[ContactName], [t2].[ContactTitle], [t2].[Country], [t2].[Fax], [t2].[Phone], [t2].[PostalCode], [t2].[Region]
            FROM (
                SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[ContactTitle], [c].[ContactName]) AS [__RowNumber__]
                FROM [Customers] AS [c]
            ) AS [t2]
            WHERE ([t2].[__RowNumber__] > @__p_0) AND ([t2].[__RowNumber__] <= (@__p_0 + @__p_1))
        ) AS [t]
        ORDER BY [t].[ContactTitle], [t].[ContactName]
    ) AS [t0]
    ORDER BY [t0].[ContactTitle], [t0].[ContactName]
) AS [t1]
ORDER BY [t1].[ContactTitle], [t1].[ContactName]",
                Sql);
        }

        public override void Skip_Take_Any()
        {
            base.Skip_Take_Any();

            Assert.Equal(
                @"@__p_0: 5
@__p_1: 10

SELECT CASE
    WHEN EXISTS (
        SELECT [t].[c]
        FROM (
            SELECT 1 AS [c], ROW_NUMBER() OVER(ORDER BY [c].[ContactName]) AS [__RowNumber__]
            FROM [Customers] AS [c]
        ) AS [t]
        WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1)))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Skip_Take_All()
        {
            base.Skip_Take_All();

            Assert.Equal(
                @"@__p_0: 5
@__p_1: 10

SELECT CASE
    WHEN NOT EXISTS (
        SELECT [t].[c]
        FROM (
            SELECT 1 AS [c], ROW_NUMBER() OVER(ORDER BY [c].[ContactName]) AS [__RowNumber__]
            FROM [Customers] AS [c]
            WHERE LEN([c].[CustomerID]) <> 5
        ) AS [t]
        WHERE ([t].[__RowNumber__] > @__p_0) AND ([t].[__RowNumber__] <= (@__p_0 + @__p_1)))
    THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT)
END",
                Sql);
        }

        public override void Skip_Count()
        {
            base.Skip_Count();

            Assert.Equal(
                @"@__p_0: 7

SELECT COUNT(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]",
                Sql);
        }

        public override void Skip_LongCount()
        {
            base.Skip_LongCount();

            Assert.Equal(
                @"@__p_0: 7

SELECT COUNT_BIG(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY @@RowCount) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]",
                Sql);
        }

        public override void OrderBy_Skip_Count()
        {
            base.OrderBy_Skip_Count();

            Assert.Equal(
                @"@__p_0: 7

SELECT COUNT(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY [c].[Country]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]",
                Sql);
        }

        public override void OrderBy_Skip_LongCount()
        {
            base.OrderBy_Skip_LongCount();

            Assert.Equal(
                @"@__p_0: 7

SELECT COUNT_BIG(*)
FROM (
    SELECT [t0].*
    FROM (
        SELECT [c].*, ROW_NUMBER() OVER(ORDER BY [c].[Country]) AS [__RowNumber__]
        FROM [Customers] AS [c]
    ) AS [t0]
    WHERE [t0].[__RowNumber__] > @__p_0
) AS [t]",
                Sql);
        }

        public override void Include_with_orderby_skip_preserves_ordering()
        {
            base.Include_with_orderby_skip_preserves_ordering();

            Assert.Equal(
                @"@__p_0: 40
@__p_1: 5

SELECT [t0].[CustomerID], [t0].[Address], [t0].[City], [t0].[CompanyName], [t0].[ContactName], [t0].[ContactTitle], [t0].[Country], [t0].[Fax], [t0].[Phone], [t0].[PostalCode], [t0].[Region]
FROM (
    SELECT [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region], ROW_NUMBER() OVER(ORDER BY [c].[City], [c].[CustomerID]) AS [__RowNumber__]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] <> N'VAFFE'
) AS [t0]
WHERE ([t0].[__RowNumber__] > @__p_0) AND ([t0].[__RowNumber__] <= (@__p_0 + @__p_1))

@__p_0: 40
@__p_1: 5

SELECT [c.Orders].[OrderID], [c.Orders].[CustomerID], [c.Orders].[EmployeeID], [c.Orders].[OrderDate]
FROM [Orders] AS [c.Orders]
INNER JOIN (
    SELECT [t1].[CustomerID], [t1].[City]
    FROM (
        SELECT [c0].[CustomerID], [c0].[City], ROW_NUMBER() OVER(ORDER BY [c0].[City], [c0].[CustomerID]) AS [__RowNumber__]
        FROM [Customers] AS [c0]
        WHERE [c0].[CustomerID] <> N'VAFFE'
    ) AS [t1]
    WHERE ([t1].[__RowNumber__] > @__p_0) AND ([t1].[__RowNumber__] <= (@__p_0 + @__p_1))
) AS [t] ON [c.Orders].[CustomerID] = [t].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]",
                Sql);
        }

        public override void GroupJoin_customers_orders_count_preserves_ordering()
        {
            base.GroupJoin_customers_orders_count_preserves_ordering();

            Assert.Equal(
                @"@__p_0: 5

SELECT [t].[CustomerID], [t].[Address], [t].[City], [t].[CompanyName], [t].[ContactName], [t].[ContactTitle], [t].[Country], [t].[Fax], [t].[Phone], [t].[PostalCode], [t].[Region], [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate]
FROM (
    SELECT TOP(@__p_0) [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
    FROM [Customers] AS [c]
    WHERE [c].[CustomerID] <> N'VAFFE'
    ORDER BY [c].[City]
) AS [t]
LEFT JOIN [Orders] AS [o] ON [t].[CustomerID] = [o].[CustomerID]
ORDER BY [t].[City], [t].[CustomerID]",
                Sql);
        }

        private const string FileLineEnding = @"
";

        private static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
