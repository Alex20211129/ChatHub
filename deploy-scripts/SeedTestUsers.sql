-- 建立 5 個測試帳號（TestUser1~5），密碼皆為 @TestUser{n}，Email 已標記為驗證過，不需要收信。
-- 登入用的帳號欄位是 Email（AccountController.Login 用 PasswordSignInAsync(model.Email, ...) 查 UserName，
-- 所以 UserName 必須跟 Email 完全一樣，不能只放短暱稱，否則會登入失敗）。
-- 在 VPS 的 SSMS 上，先切換到 ChatRoomSysDb 資料庫再執行這份 script。

SET QUOTED_IDENTIFIER ON;

DECLARE @MemberRoleId nvarchar(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'Member');

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = 'TESTUSER1@EXAMPLE.COM')
BEGIN
    INSERT INTO AspNetUsers (Id, AccessFailedCount, ConcurrencyStamp, CreatedAt, CurrentHashAlgorithm, Email, EmailConfirmed, IsDisabled, LastLoginAt, LockoutEnabled, LockoutEnd, LoginCount, NormalizedEmail, NormalizedUserName, PasswordHash, PhoneNumber, PhoneNumberConfirmed, SecurityStamp, TwoFactorEnabled, UserName)
    VALUES ('637c4501-54be-4c5d-9e69-ef866612abdd', 0, '3fabb4b1-9172-45d3-bafa-e47ced00bb1c', '2026-09-21 05:32:46.2142536', 2, 'TestUser1@example.com', 1, 0, NULL, 1, NULL, 0, 'TESTUSER1@EXAMPLE.COM', 'TESTUSER1@EXAMPLE.COM', 'QAXQmLO4vsdie3v7Ri5JxDMxTRkUrLwIf1dUdDS/0h6IydhHP3+iz611M+H8Q2Db ; 4 ; 65536 ; 8', NULL, 0, 'KYEET3CO2OSLOXJT2WOAANVR3IBXNM7V', 0, 'TestUser1@example.com');

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('637c4501-54be-4c5d-9e69-ef866612abdd', @MemberRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = 'TESTUSER2@EXAMPLE.COM')
BEGIN
    INSERT INTO AspNetUsers (Id, AccessFailedCount, ConcurrencyStamp, CreatedAt, CurrentHashAlgorithm, Email, EmailConfirmed, IsDisabled, LastLoginAt, LockoutEnabled, LockoutEnd, LoginCount, NormalizedEmail, NormalizedUserName, PasswordHash, PhoneNumber, PhoneNumberConfirmed, SecurityStamp, TwoFactorEnabled, UserName)
    VALUES ('1e5da2cc-de41-4f61-97c1-6b3e4ec6f95c', 0, 'ab7b98db-c466-4f76-9236-a9c0721698e2', '2026-09-21 05:32:46.6603176', 2, 'TestUser2@example.com', 1, 0, NULL, 1, NULL, 0, 'TESTUSER2@EXAMPLE.COM', 'TESTUSER2@EXAMPLE.COM', '69XAfBdPGraTdr4Yyhpooq3xtiN+mrZ9NW9vnhZSMdYwbL2YHGQgReGyyCk4ZSEh ; 4 ; 65536 ; 8', NULL, 0, '6SBRK5YUF6BRD4ZZXSXFPQ37CU3CJIMH', 0, 'TestUser2@example.com');

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('1e5da2cc-de41-4f61-97c1-6b3e4ec6f95c', @MemberRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = 'TESTUSER3@EXAMPLE.COM')
BEGIN
    INSERT INTO AspNetUsers (Id, AccessFailedCount, ConcurrencyStamp, CreatedAt, CurrentHashAlgorithm, Email, EmailConfirmed, IsDisabled, LastLoginAt, LockoutEnabled, LockoutEnd, LoginCount, NormalizedEmail, NormalizedUserName, PasswordHash, PhoneNumber, PhoneNumberConfirmed, SecurityStamp, TwoFactorEnabled, UserName)
    VALUES ('2e6b36b5-47ce-448d-8755-ca64120b2bad', 0, '6cdec30b-0875-4203-85c1-1d803d87044d', '2026-09-21 05:32:46.8404066', 2, 'TestUser3@example.com', 1, 0, NULL, 1, NULL, 0, 'TESTUSER3@EXAMPLE.COM', 'TESTUSER3@EXAMPLE.COM', 'LcXNPvc1M1WORT4tHboU7AtH8hHJYyABtDr7zkhiSPtAXrbDfiUfKW9fGBhI90Zm ; 4 ; 65536 ; 8', NULL, 0, 'I4MC7PFC23FEB23KOKQCEDUNH3GBZZYK', 0, 'TestUser3@example.com');

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('2e6b36b5-47ce-448d-8755-ca64120b2bad', @MemberRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = 'TESTUSER4@EXAMPLE.COM')
BEGIN
    INSERT INTO AspNetUsers (Id, AccessFailedCount, ConcurrencyStamp, CreatedAt, CurrentHashAlgorithm, Email, EmailConfirmed, IsDisabled, LastLoginAt, LockoutEnabled, LockoutEnd, LoginCount, NormalizedEmail, NormalizedUserName, PasswordHash, PhoneNumber, PhoneNumberConfirmed, SecurityStamp, TwoFactorEnabled, UserName)
    VALUES ('cf4d7718-25b9-4753-83ca-f89cefb722e2', 0, '68102f53-5d51-4ee4-8cb1-86ba70798255', '2026-09-21 05:32:47.0463094', 2, 'TestUser4@example.com', 1, 0, NULL, 1, NULL, 0, 'TESTUSER4@EXAMPLE.COM', 'TESTUSER4@EXAMPLE.COM', 'lPjtqXHTYrs8Iu83rLZhdzePzrUpMFwb6zpLmCqeI0GNKsWyR29baSd39BEI4ojt ; 4 ; 65536 ; 8', NULL, 0, 'Y5KTT7HW52IOUNDKZEHPG7GKGD4UK46R', 0, 'TestUser4@example.com');

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('cf4d7718-25b9-4753-83ca-f89cefb722e2', @MemberRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedUserName = 'TESTUSER5@EXAMPLE.COM')
BEGIN
    INSERT INTO AspNetUsers (Id, AccessFailedCount, ConcurrencyStamp, CreatedAt, CurrentHashAlgorithm, Email, EmailConfirmed, IsDisabled, LastLoginAt, LockoutEnabled, LockoutEnd, LoginCount, NormalizedEmail, NormalizedUserName, PasswordHash, PhoneNumber, PhoneNumberConfirmed, SecurityStamp, TwoFactorEnabled, UserName)
    VALUES ('501aa6e0-53c1-499a-9e69-4fb34279242d', 0, '9cee42f7-8288-4904-b1a4-e4ec1e619b1f', '2026-09-21 05:32:47.3679695', 2, 'TestUser5@example.com', 1, 0, NULL, 1, NULL, 0, 'TESTUSER5@EXAMPLE.COM', 'TESTUSER5@EXAMPLE.COM', 'NIDnUZ2AfNpCLs27DmwZLFhlX1rb/ZhYrfZWLtw1NlNsjlL7zetczugGp9M1smQL ; 4 ; 65536 ; 8', NULL, 0, 'V3U6RQCAX2BUSCDTRNDYN6CPA6ZKNTHA', 0, 'TestUser5@example.com');

    INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('501aa6e0-53c1-499a-9e69-4fb34279242d', @MemberRoleId);
END;
