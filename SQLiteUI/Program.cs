using DataAccessLibrary;
using DataAccessLibrary.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SQLiteUI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SqliteCRUD sql = new SqliteCRUD(GetConnectionString());

            ReadAllContacts(sql);

            //ReadContact(sql, 2);

            CreateNewContact(sql);

            //UpdateContact(sql);

            //RemovePhoneNumberFromContact(sql, 1, 1);

            Console.WriteLine("Done proccessing sqlite");
            Console.ReadLine();
        }

        private static void RemovePhoneNumberFromContact(SqliteCRUD sql, int contactId, int phoneNumberId)
        {
            sql.RemovePhoneNumberFromContact(contactId, phoneNumberId);
        }
        private static void UpdateContact(SqliteCRUD sql)
        {
            BasicContactModel contact = new BasicContactModel
            {
                Id = 1,
                FirstName = "Rabea",
                LastName = "Abdulgaffar"
            };

            sql.UpdateContactName(contact);
        }
        private static void CreateNewContact(SqliteCRUD sql)
        {
            FullContactModel user = new FullContactModel
            {
                BasicInfo = new BasicContactModel
                {
                    FirstName = "Amjad",
                    LastName = "Bukhari"
                }
            };

            user.EmailAddresses.Add(new EmailAddressModel { EmailAddress = "kamal2Alahli.com" });

            user.PhoneNumbers.Add(new PhoneNumberModel { Id = 1, PhoneNumber = "554537104" });
            user.PhoneNumbers.Add(new PhoneNumberModel { PhoneNumber = "39875645" });

            sql.CreateContact(user);

        }
        private static void ReadAllContacts(SqliteCRUD sql)
        {
            var rows = sql.GetAllContacts();

            foreach (var row in rows)
            {
                Console.WriteLine($"{row.Id}: { row.FirstName } { row.LastName }");
            }
        }

        private static void ReadContact(SqliteCRUD sql, int contactId)
        {
            var contact = sql.GetFullContactById(contactId);

            Console.WriteLine($"{contact.BasicInfo.Id}: { contact.BasicInfo.FirstName } { contact.BasicInfo.LastName }");
        }

        private static string GetConnectionString(string connectionStringName = "Default")
        {
            string output = "";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            output = config.GetConnectionString(connectionStringName);

            return output;
        }
    }
}
