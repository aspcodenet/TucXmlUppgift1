using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.EntityFrameworkCore;
using TucXmlUppgift1.Models;

namespace TucXmlUppgift1
{
    class Program
    {
        static void Main(string[] args)
        {
            //1 Skapa en kolumn i Products som heter ExternalId nvarchar(30)
            var client = new HttpClient();
            var s = client.GetStringAsync("https://schoolbusiness.blob.core.windows.net/sharedfiles/AvantLinkProducts.xml")
                .Result;
            var doc = XDocument.Parse(s);

            var us = new CultureInfo("en-US");

            var context = new northwindContext();

            foreach (var element in doc.XPathSelectElements("//Products/Product"))
            {
                var id = element.XPathSelectElement("SKU").Value;
                var category = element.XPathSelectElement("Category").Value;
                var name = element.XPathSelectElement("Product_Name").Value;
                if (name.Length > 40)
                {
                    Console.WriteLine("Vad ska vi göra nu då???");
                    // Typiska problem vid INTEGRATION mellan olika system
                    //...
                    continue;
                }
                var price = decimal.Parse(element.XPathSelectElement("Retail_Price").Value,
                    NumberStyles.Number,
                    us);
                Console.WriteLine($"{id} {name} {category}");

                if (!context.Categories.Any(r => r.CategoryName == category))
                {
                    context.Categories.Add(new Category
                    {
                        CategoryName = category,
                    });
                    context.SaveChanges();
                }

                var cat = context.Categories.First(e => e.CategoryName == category);

                var prod = context.Products.FirstOrDefault(r => r.ExternalId == id);
                if (prod == null)
                {
                    context.Products.Add(new Product
                    {
                        ExternalId = id,
                        Category = cat,
                        ProductName = name,
                        UnitPrice = price,
                    });
                }
                else
                {
                    prod.ProductName = name;
                    prod.Category = cat;
                    prod.ExternalId = id;
                    prod.UnitPrice = price;

                }

                context.SaveChanges();
            }

        }
    }
}
