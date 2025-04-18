﻿using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using Shared.Models;
using System.Collections.Generic;
using System.Data;

namespace AdminApp.Controllers
{
    public class ProductController
    {
        private DatabaseHelper db = new DatabaseHelper();

        public List<Product> GetAllProducts()
        {
            string query = "SELECT id_product, name_product, id_line, id_brand, description, material, price, imageUrl, releasedate, status FROM product";
            DataTable dt = db.ExecuteQuery(query);
            List<Product> products = new List<Product>();
            foreach (DataRow row in dt.Rows)
            {
                products.Add(new Product
                    {
                        IdProduct = Convert.ToInt32(row["id_product"]),
                        NameProduct = row["name_product"].ToString(),
                        IdLine = Convert.ToInt32(row["id_line"]),
                        IdBrand = Convert.ToInt32(row["id_brand"]),
                        Description = row["description"].ToString(),
                        Material = row["material"].ToString(),
                        Price = Convert.ToDecimal(row["price"]),
                        ImageUrl = row["imageUrl"].ToString(),
                        Releasedate = Convert.ToDateTime(row["releasedate"]),
                        Status = row["status"].ToString()
                });
             }
            return products;
        }

        public Product GetProductById(int idProduct)
        {
            string query = "SELECT * FROM product WHERE id_product = @id";
            MySqlParameter[] parameters = new MySqlParameter[]
            {
                new MySqlParameter("@id", idProduct)
            };
            DataTable table = db.ExecuteQuery(query, parameters);
            if (table.Rows.Count > 0)
            {
                DataRow row = table.Rows[0];
                return new Product
                {
                    IdProduct = Convert.ToInt32(row["id_product"]),
                    NameProduct = row["name_product"].ToString(),
                    IdLine = Convert.ToInt32(row["id_line"]),
                    IdBrand = Convert.ToInt32(row["id_brand"]),
                    Description = row["description"].ToString(),
                    Material = row["material"].ToString(),
                    Price = Convert.ToDecimal(row["price"]),
                    ImageUrl = row["imageUrl"].ToString(),
                    Releasedate = Convert.ToDateTime(row["releasedate"]),
                    Status = row["status"].ToString()
                };
            }
            return null;
        }

        public bool AddProduct(Product product)
        {
            string query = "INSERT INTO product (name_product, id_line, id_brand, description, material, price, imageUrl, releasedate, status) " +
                           "VALUES (@name_product, @id_line, @id_brand, @description, @material, @price, @imageUrl, @releasedate, @status)";

            MySqlParameter[] parameters = new MySqlParameter[9];
            parameters[0] = new MySqlParameter("name_product", product.NameProduct);
            parameters[1] = new MySqlParameter("id_line", product.IdLine);
            parameters[2] = new MySqlParameter("id_brand", product.IdBrand);
            parameters[3] = new MySqlParameter("description", product.Description);
            parameters[4] = new MySqlParameter("material", product.Material);
            parameters[5] = new MySqlParameter("price", product.Price);
            parameters[6] = new MySqlParameter("imageUrl", product.ImageUrl);
            parameters[7] = new MySqlParameter("releasedate", product.Releasedate);
            parameters[8] = new MySqlParameter("status", product.Status);

            bool result = db.ExecuteNonQuery(query, parameters);
            if (result)
            {
                // Lấy ID sản phẩm vừa thêm
                int productId = db.GetLastInsertId();
                // Thêm ảnh vào bảng image
                string queryImage = "INSERT INTO image (id_product, imageUrl, isPrimary) VALUES (@id_product, @imageUrl, 1)";
                MySqlParameter[] imageParams = new MySqlParameter[2];
                imageParams[0] = new MySqlParameter("id_product", productId);
                imageParams[1] = new MySqlParameter("imageUrl", product.ImageUrl);

                return db.ExecuteNonQuery(queryImage, imageParams);
            }

            return false;
        }
        public bool UpdateProduct(Product product)
        {
            string query = @"UPDATE product
                            SET name_product = @name_product, id_line = @id_line, id_brand = @id_brand, 
                                description = @description, material = @material, price = @price, 
                                releasedate = @releasedate, status = @status , imageUrl = @imageUrl
                            WHERE id_product = @id_product";
            MySqlParameter[] parameters = new MySqlParameter[10];
            parameters[0] = new MySqlParameter("@name_product", product.NameProduct);
            parameters[1] = new MySqlParameter("@id_line", product.IdLine);
            parameters[2] = new MySqlParameter("@id_brand", product.IdBrand);
            parameters[3] = new MySqlParameter("@description", product.Description);
            parameters[4] = new MySqlParameter("@material", product.Material);
            parameters[5] = new MySqlParameter("@price", product.Price);
            parameters[6] = new MySqlParameter("@releasedate", product.Releasedate);
            parameters[7] = new MySqlParameter("@status", product.Status);
            parameters[8] = new MySqlParameter("@imageUrl", product.ImageUrl);
            parameters[9] = new MySqlParameter("@id_product", product.IdProduct);
            
            return db.ExecuteNonQuery(query, parameters);
        }
        public bool DeleteProduct(int idProduct)
        {
            string checkQuery = @"
            SELECT COUNT(*) 
            FROM invoicedetail d
            JOIN invoice i ON d.id_invoice = i.id_invoice
            JOIN product_variant v ON d.id_variant = v.id_variant
            WHERE v.id_product = @id_product AND i.status != 'Hoàn thành'";

            MySqlParameter[] checkParams = new MySqlParameter[]
            {
                new MySqlParameter("@id_product", idProduct)
            };
            int count = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

            if (count == 0)
            {
                string deleteVariantQuery = "DELETE FROM product_variant WHERE id_product = @id_product";
                string deleteProductQuery = "DELETE FROM product WHERE id_product = @id_product";

                db.ExecuteNonQuery(deleteVariantQuery, checkParams); // Xoá variant
                return db.ExecuteNonQuery(deleteProductQuery, checkParams); // Xoá product
            }
            return false;
        }

    }
}
