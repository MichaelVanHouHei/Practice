IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Issurance')
CREATE DATABASE Issurance;

GO

USE Issurance;
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'member')
CREATE TABLE member (
  Member_ID INT PRIMARY KEY IDENTITY(1,1),
  Name NVARCHAR(100) NOT NULL,
  Card_Tier VARCHAR(20) NOT NULL CHECK(Card_Tier IN ('Mass', 'VIP')),
  Is_Active INT NOT NULL CHECK(Is_Active IN (0, 1)),
  Join_Date DATE NOT NULL,
  Avg_bet INT
);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_member_Card_Tier')
CREATE INDEX IX_member_Card_Tier ON member (Card_Tier);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_member_Is_Active')
CREATE INDEX IX_member_Is_Active ON member (Is_Active);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_member_Avg_bet')
CREATE INDEX IX_member_Avg_bet ON member (Avg_bet);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_member_Join_Date')
CREATE INDEX IX_member_Join_Date ON member (Join_Date);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'prize')
CREATE TABLE prize (
  Prize_Code VARCHAR(100) PRIMARY KEY,
  DESCRIPTION VARCHAR(100) NOT NULL
);

IF NOT EXISTS (SELECT * FROM prize WHERE Prize_Code = 'PRIZE_FA')
INSERT INTO prize (Prize_Code, DESCRIPTION)
VALUES ('PRIZE_FA', 'F&B $1,000 Coupon');

IF NOT EXISTS (SELECT * FROM prize WHERE Prize_Code = 'PRIZE_NEW_MEMBER')
INSERT INTO prize (Prize_Code, DESCRIPTION)
VALUES ('PRIZE_NEW_MEMBER', 'Free Drink');

IF NOT EXISTS (SELECT * FROM prize WHERE Prize_Code = 'PRIZE_F10')
INSERT INTO prize (Prize_Code, DESCRIPTION)
VALUES ('PRIZE_F10', 'F&B $10 Coupon');