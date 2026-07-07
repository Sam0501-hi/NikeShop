-- Tạo bảng ProductImages
CREATE TABLE [dbo].[ProductImages] (
    [ProductImageId] INT IDENTITY(1,1) NOT NULL,
    [ProductId] INT NOT NULL,
    [ImagePath] NVARCHAR(MAX) NOT NULL,
    CONSTRAINT [PK_ProductImages] PRIMARY KEY ([ProductImageId]),
    CONSTRAINT [FK_ProductImages_Products_ProductId] FOREIGN KEY ([ProductId])
        REFERENCES [dbo].[Products]([ProductId]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_ProductImages_ProductId] ON [dbo].[ProductImages]([ProductId]);
GO

-- Đánh dấu cho EF Core biết migration này đã được áp dụng,
-- để lần sau chạy Update-Database không bị áp dụng lại / báo lỗi trùng bảng
IF NOT EXISTS (
    SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20260706150000_AddProductImagesTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260706150000_AddProductImagesTable', '10.0.9');
END
GO