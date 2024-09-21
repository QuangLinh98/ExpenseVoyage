SELECT TOP (1000) [UserId]
      ,[Username]
      ,[Email]
      ,[Password]
      ,[Address]
      ,[Phone]
      ,[IsNewUser]
      ,[Role]
      ,[ImagePath]
      ,[EmailConfirmed]
      ,[EmailConfirmationToken]
      ,[ResetPasswordToken]
      ,[ResetPasswordTokenExpiration]
  FROM [ExpenseVoyageProject].[dbo].[Users]

  INSERT INTO [ExpenseVoyageProject].[dbo].[Users] (
    [Username],
    [Email],
    [Password],
    [Address],
    [Phone],
    [IsNewUser],
    [Role],
    [ImagePath],
    [EmailConfirmed],
    [EmailConfirmationToken],
    [ResetPasswordToken],
    [ResetPasswordTokenExpiration]
)
VALUES (
    'Minh Anh',  -- Username của tài khoản
    'minhanh@gmail.com',  -- Email của tài khoản
    'AQAAAAEAACcQAAAAEHVTXCfroJJ/plN70Rqz35dVB2Oy4D1p7M4TSiOFpnhUthGTNdoTwBTkn3KVku0Onw==',  -- Mật khẩu đã được mã hóa (bạn cần phải băm mật khẩu trước khi lưu trữ)
    '123 Admin Street',  -- Địa chỉ của tài khoản (có thể bỏ qua nếu không cần)
    '123-456-7890',  -- Số điện thoại của tài khoản (có thể bỏ qua nếu không cần)
    0,  -- IsNewUser (0 hoặc 1 tùy thuộc vào việc tài khoản mới hay không)
    'SuperAdmin',  -- Vai trò của tài khoản
    NULL,  -- ImagePath (có thể để NULL nếu không có ảnh)
    1,  -- EmailConfirmed (0 hoặc 1 tùy thuộc vào việc email đã được xác nhận hay chưa)
    NULL,  -- EmailConfirmationToken (có thể để NULL nếu không có mã token xác nhận email)
    NULL,  -- ResetPasswordToken (có thể để NULL nếu không có mã token đặt lại mật khẩu)
    NULL  -- ResetPasswordTokenExpiration (có thể để NULL nếu không có thời gian hết hạn token)
);
