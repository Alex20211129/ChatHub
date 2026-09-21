-- 修正已經在正式站建立的 TestUser1~5 帳號：把 UserName / NormalizedUserName 改成跟 Email 一致
-- （AccountController.Login 是拿 Email 去查 UserName，兩者要一樣才能登入）。
-- 在 VPS 的 SSMS 上，先切換到 ChatRoomSysDb 資料庫再執行這份 script。

SET QUOTED_IDENTIFIER ON;

UPDATE AspNetUsers
SET UserName = Email,
    NormalizedUserName = NormalizedEmail
WHERE Email IN (
    'TestUser1@example.com',
    'TestUser2@example.com',
    'TestUser3@example.com',
    'TestUser4@example.com',
    'TestUser5@example.com'
);
