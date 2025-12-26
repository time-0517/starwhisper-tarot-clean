namespace 占卜.Models
{
    public class UserLoginModel
    {
        // 加個 ? 表示這些欄位可能是空的 (例如使用者還沒輸入時)
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}