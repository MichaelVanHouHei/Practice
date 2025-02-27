﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FakeDataGenerator.Models;

[Table("member")]
[Index("AvgBet", Name = "IX_member_Avg_bet")]
[Index("CardTier", Name = "IX_member_Card_Tier")]
[Index("IsActive", Name = "IX_member_Is_Active")]
[Index("JoinDate", Name = "IX_member_Join_Date")]
public partial class Member
{
    [Key]
    [Column("Member_ID")]
    public int MemberId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Column("Card_Tier")]
    [StringLength(20)]
    [Unicode(false)]
    public string CardTier { get; set; }
    // public CardTier CardTier { get; set; } C# enum harm performance wise 

    [Column("Is_Active")]
    public int IsActive { get; set; }

    [Column("Join_Date")]
    public DateOnly JoinDate { get; set; }

    [Column("Avg_bet")]
    public int? AvgBet { get; set; }
}
//public enum CardTier
//{
//    Mass,
//    VIP
//}