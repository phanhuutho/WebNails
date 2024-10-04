using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebNails.Models
{
    public class InfoPaypal
    {
        public Guid ID { get; set; }
        public int Nail_ID { get; set; }
        public string Transactions { get; set; }
        public string Code { get; set; }
        public string Owner { get; set; } //Email Owner
        public float Amount { get; set; }
        public string Stock { get; set; } //Email Receiver
        public string Email { get; set; } //Email Buyer
        public string NameReceiver { get; set; } //Name Receiver
        public string NameBuyer { get; set; } //Name Buyer
        public string Message { get; set; }
        public PaymentStatus Status { get; set; } //0: pending; 1:success
        public bool IsUsed { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public DateTime DateTimeUpdateUsed { get; set; }
        public string CodeSaleOff { get; set; }
        public int AmountReal { get; set; }
        public int ValidCode { get; set; }
        public string DescriptionValidCode { get; set; }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1
    }

    public class Paypal_IPN_Response
    {
        public string mc_gross { get; set; }
        public string protection_eligibility { get; set; }
        public string address_status { get; set; }
        public string payer_id { get; set; }
        public string tax { get; set; }
        public string address_street { get; set; }
        public string payment_date { get; set; }
        public string payment_status { get; set; }
        public string charset { get; set; }
        public string address_zip { get; set; }
        public string first_name { get; set; }
        public string mc_fee { get; set; }
        public string address_country_code { get; set; }
        public string address_name { get; set; }
        public string notify_version { get; set; }
        public string custom { get; set; }
        public string payer_status { get; set; }
        public string address_country { get; set; }
        public string address_city { get; set; }
        public string quantity { get; set; }
        public string verify_sign { get; set; }
        public string payer_email { get; set; }
        public string txn_id { get; set; }
        public string payment_type { get; set; }
        public string last_name { get; set; }
        public string address_state { get; set; }
        public string receiver_email { get; set; }
        public string payment_fee { get; set; }
        public string receiver_id { get; set; }
        public string txn_type { get; set; }
        public string item_name { get; set; }
        public string mc_currency { get; set; }
        public string item_number { get; set; }
        public string residence_country { get; set; }
        public string test_ipn { get; set; }
        public string handling_amount { get; set; }
        public string transaction_subject { get; set; }
        public string payment_gross { get; set; }
        public string shipping { get; set; }
    }
}