using DataAccessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataAccessLibrary
{
    public class SqliteCRUD
    {
        private readonly string connectionString;
        private SqliteDataAccess db = new SqliteDataAccess();

        public SqliteCRUD(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<BasicContactModel> GetAllContacts()
        {
            string sql = "select Id, FirstName, LastName from Contacts";

            return db.LoadData<BasicContactModel, dynamic>(sql, null, connectionString);
        }

        public FullContactModel GetFullContactById(int id)
        {
            string sql = "select Id, FirstName, LastName from Contacts where Id =@Id";

            FullContactModel output = new FullContactModel();

            output.BasicInfo = db.LoadData<BasicContactModel, dynamic>(sql, new { Id = id }, connectionString).First();

            if (output.BasicInfo == null)
            {
                return null;
            }

            sql = @"select e.* 
                    from EmailAddresses e
                    inner
                    join ContactEmail ce on ce.EmailAddressId = e.Id
                    where ce.ContactId = @Id";

            output.EmailAddresses = db.LoadData<EmailAddressModel, dynamic>(sql, new { Id = id }, connectionString);

            sql = @"select p.* 
                    from PhoneNumbers p
                    inner join ContactPhoneNumbers cp on cp.PhoneNumberId = p.Id
                    where cp.ContactId = @Id";

            output.PhoneNumbers = db.LoadData<PhoneNumberModel, dynamic>(sql, new { Id = id }, connectionString);

            return output;
        }

        public void CreateContact(FullContactModel contact)
        {
            // Save Basic Contact
            string sql = "insert into Contacts (FirstName, LastName) values (@FirstName, @LastName);";

            db.SaveData(sql, new
            {
                contact.BasicInfo.FirstName,
                contact.BasicInfo.LastName
            },
                connectionString);

            //get the ID of it 
            sql = "select Id from Contacts where FirstName = @FirstName and LastName = @LastName;";
            int contactId = db.LoadData<IdLookupModel, dynamic>(
                sql,
                new
                {
                    contact.BasicInfo.FirstName,
                    contact.BasicInfo.LastName
                },
                connectionString).First().Id;

            foreach (var number in contact.PhoneNumbers)
            {
                if (number.Id == 0)
                {
                    sql = "insert into PhoneNumbers (PhoneNumber) values (@PhoneNumber);";
                    db.SaveData(sql, new { number.PhoneNumber }, connectionString);

                    sql = "select Id from PhoneNumbers where PhoneNumber = @PhoneNumber;";
                    number.Id = db.LoadData<IdLookupModel, dynamic>(
                        sql,
                         new { number.PhoneNumber },
                         connectionString).First().Id;
                }

                sql = "insert into ContactPhoneNumbers (ContactId, PhoneNumberId) values" +
                    "(@ContactId, @PhoneNumberId);";
                db.SaveData(sql, new { ContactId = contactId, PhoneNumberId = number.Id },
                    connectionString);
            }

            foreach (var email in contact.EmailAddresses)
            {
                if (email.Id == 0)
                {
                    sql = "insert into EmailAddresses (EmailAddress) values (@EmailAddress);";
                    db.SaveData(sql, new { email.EmailAddress }, connectionString);

                    sql = "select Id from EmailAddresses where EmailAddress = @EmailAddress;";
                    email.Id = db.LoadData<IdLookupModel, dynamic>(
                        sql,
                        new { email.EmailAddress },
                        connectionString).First().Id;
                }

                sql = "insert into ContactEmail (ContactId, EmailAddressId) values " +
                    "(@ContactId, @EmailAddressId);";
                db.SaveData(sql, new { ContactId = contactId, EmailAddressId = email.Id },
                    connectionString);

            }

        }

        public void UpdateContactName(BasicContactModel contact)
        {
            string sql = "update Contacts set FirstName = @FirstName, LastName = @LastName where Id = @Id;";
            db.SaveData(sql, contact, connectionString);
        }

        public void RemovePhoneNumberFromContact(int contactId, int phoneNumberId)
        {
            //Find all the links of this number
            //if 1 than del link and number
            //if > 1 then del link fro contact

            string sql = "select Id, ContactId, PhoneNumberId from ContactPhoneNumbers where PhoneNumberId = @PhoneNumberId;";
            var links = db.LoadData<ContactPhoneNumberModel, dynamic>(
                sql,
                new { PhoneNumberId = phoneNumberId },
                connectionString);

            sql = "delete from ContactPhoneNumbers where PhoneNumberId = @PhoneNumberId and ContactId = @ContactId;";
            db.SaveData(sql, new { PhoneNumberId = phoneNumberId, ContactId = contactId },
                connectionString);

            if (links.Count == 1)
            {
                sql = "delete from PhoneNumbers where Id = @PhoneNumberId;";
                db.SaveData(sql, new { PhoneNumberId = phoneNumberId }, connectionString);
            }
        }
    }
}
