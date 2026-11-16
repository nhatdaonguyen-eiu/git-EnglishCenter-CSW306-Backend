using Microsoft.EntityFrameworkCore;

namespace EnglishCenterManagement_CSW306_Project.Services
{
    public static class SaveChangesExtensions
    {
        // Lưu thay đổi nhưng không ném lỗi 500 khi DB từ chối (vd hai người thao tác cùng lúc
        // làm vi phạm unique index). Trả false để Service trả về 409 Conflict.
        public static async Task<bool> TrySaveChangesAsync(this DbContext context)
        {
            try
            {
                await context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
