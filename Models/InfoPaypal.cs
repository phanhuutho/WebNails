﻿using System;
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
        public string Owner { get; set; } //Email Owner
        public int Amount { get; set; }
        public string Stock { get; set; } //Email Receiver
        public string Email { get; set; } //Email Buyer
        public string Message { get; set; }
        public PaymentStatus Status { get; set; } //0: pending; 1:success
        public bool IsUsed { get; set; }
        public DateTime DateTimeCreate { get; set; }
        public DateTime DateTimeUpdateUsed { get; set; }
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Success = 1
    }
}